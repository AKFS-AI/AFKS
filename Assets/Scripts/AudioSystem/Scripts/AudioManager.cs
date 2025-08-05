using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using AFKS.Shared.Events;
using AFKS.Shared.Interfaces;
using AFKS.Shared.Utils;

namespace AFKS.AudioSystem
{
    /// <summary>
    /// 오디오 시스템을 관리하는 매니저
    /// </summary>
    public class AudioManager : MonoBehaviour, ISaveable
    {
                [Header("🔊 오디오 소스")]
        [SerializeField, Tooltip("배경음악(BGM) 전용 오디오 소스")] private AudioSource bgmSource;
        [SerializeField, Tooltip("효과음(SFX) 전용 오디오 소스들")] private AudioSource[] sfxSources;
        [SerializeField, Tooltip("환경음(Ambient) 전용 오디오 소스")] private AudioSource ambientSource;
        
        [Header("⚙️ 설정")]
        [SerializeField, Range(1, 16), Tooltip("동시 재생 가능한 최대 효과음 개수")] private int maxSfxSources = 8;
        [SerializeField, Range(0.1f, 5f), Tooltip("BGM 크로스페이드 전환 시간 (초)")] private float crossfadeDuration = 1f;

#pragma warning disable CS0414 // 향후 오디오 풀링 기능 확장을 위해 보관
        [SerializeField, Tooltip("오디오 풀링 시스템 활성화 (향후 확장용)")] private bool enableAudioPooling = true;
#pragma warning restore CS0414
        
        [Header("🎵 볼륨 조절")]
        [SerializeField, Range(0f, 1f), Tooltip("전체 마스터 볼륨")] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("배경음악 볼륨")] private float bgmVolume = 0.7f;
        [SerializeField, Range(0f, 1f), Tooltip("효과음 볼륨")] private float sfxVolume = 0.8f;
        [SerializeField, Range(0f, 1f), Tooltip("환경음 볼륨")] private float ambientVolume = 0.5f;
        
        [Header("📡 이벤트")]
        [SerializeField, Tooltip("BGM 변경 시 발생하는 게임 이벤트")] private GameEvent onBGMChanged;
        [SerializeField, Tooltip("볼륨 변경 시 발생하는 게임 이벤트")] private GameEvent onVolumeChanged;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<AudioClip> OnBGMChanged = new GameEvent<AudioClip>();
        public static readonly GameEvent<string> OnSFXPlayed = new GameEvent<string>();
        public static readonly GameEvent<float> OnVolumeChanged = new GameEvent<float>();
        
        // === PROPERTIES ===
        public float MasterVolume 
        { 
            get => masterVolume; 
            set { masterVolume = Mathf.Clamp01(value); UpdateAllVolumes(); } 
        }
        
        public float BGMVolume 
        { 
            get => bgmVolume; 
            set { bgmVolume = Mathf.Clamp01(value); UpdateBGMVolume(); } 
        }
        
        public float SFXVolume 
        { 
            get => sfxVolume; 
            set { sfxVolume = Mathf.Clamp01(value); UpdateSFXVolume(); } 
        }
        
        public float AmbientVolume 
        { 
            get => ambientVolume; 
            set { ambientVolume = Mathf.Clamp01(value); UpdateAmbientVolume(); } 
        }
        
        public bool IsPlayingBGM => bgmSource != null && bgmSource.isPlaying;
        public AudioClip CurrentBGM => bgmSource?.clip;
        public string SaveID => "AudioManager";
        
        // === PRIVATE FIELDS ===
        private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
        private Queue<AudioSource> availableSfxSources = new Queue<AudioSource>();
        private List<AudioSource> activeSfxSources = new List<AudioSource>();
        private Coroutine bgmFadeCoroutine;
        
        // === CACHE MANAGEMENT ===
        private const int MAX_AUDIO_CACHE_SIZE = 50; // 최대 캐시 크기 제한
        
        // === SINGLETON ACCESS ===
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<AudioManager>();
                return instance;
            }
        }
        
        // === UNITY LIFECYCLE ===
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            UpdateSFXSourcePool();
        }
        
        // === INITIALIZATION ===
        private void InitializeAudioManager()
        {
            CreateAudioSources();
            LoadAudioSettings();
            Debug.Log("[오디오매니저] 초기화 완료");
        }
        
        /// <summary>
        /// 오디오 소스 생성
        /// </summary>
        private void CreateAudioSources()
        {
            // BGM 소스 생성
            if (bgmSource == null)
            {
                GameObject bgmObject = new GameObject("BGM_AudioSource");
                bgmObject.transform.SetParent(transform);
                bgmSource = bgmObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }
            
            // 앰비언트 소스 생성
            if (ambientSource == null)
            {
                GameObject ambientObject = new GameObject("Ambient_AudioSource");
                ambientObject.transform.SetParent(transform);
                ambientSource = ambientObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
            
            // SFX 소스들 생성
            CreateSFXSources();
        }
        
        /// <summary>
        /// SFX 오디오 소스들 생성
        /// </summary>
        private void CreateSFXSources()
        {
            sfxSources = new AudioSource[maxSfxSources];
            
            for (int i = 0; i < maxSfxSources; i++)
            {
                GameObject sfxObject = new GameObject($"SFX_AudioSource_{i}");
                sfxObject.transform.SetParent(transform);
                
                AudioSource source = sfxObject.AddComponent<AudioSource>();
                source.loop = false;
                source.playOnAwake = false;
                
                sfxSources[i] = source;
                availableSfxSources.Enqueue(source);
            }
        }
        
        /// <summary>
        /// 오디오 설정 로드
        /// </summary>
        private void LoadAudioSettings()
        {
            // PlayerPrefs에서 볼륨 설정 로드
            masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("Audio_BGMVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 0.8f);
            ambientVolume = PlayerPrefs.GetFloat("Audio_AmbientVolume", 0.5f);
            
            UpdateAllVolumes();
        }
        
        // === BGM CONTROL ===
        
        /// <summary>
        /// BGM 재생
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="fadeIn">페이드 인 여부</param>
        /// <param name="loop">반복 재생 여부</param>
        public void PlayBGM(AudioClip clip, bool fadeIn = true, bool loop = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("[오디오매니저] BGM 클립이 null입니다");
                return;
            }
            
            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
            }
            
            if (fadeIn && IsPlayingBGM)
            {
                bgmFadeCoroutine = StartCoroutine(CrossfadeBGM(clip, loop));
            }
            else
            {
                PlayBGMImmediate(clip, loop);
            }
        }
        
        /// <summary>
        /// BGM 즉시 재생
        /// </summary>
        private void PlayBGMImmediate(AudioClip clip, bool loop)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = bgmVolume * masterVolume;
            bgmSource.Play();
            
            onBGMChanged?.Raise();
            OnBGMChanged.Raise(clip);
            
            Debug.Log($"[오디오매니저] BGM 재생: {clip.name}");
        }
        
        /// <summary>
        /// BGM 크로스페이드
        /// </summary>
        private IEnumerator CrossfadeBGM(AudioClip newClip, bool loop)
        {
            float startVolume = bgmSource.volume;
            
            // 페이드 아웃
            float elapsedTime = 0f;
            while (elapsedTime < crossfadeDuration * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / (crossfadeDuration * 0.5f));
                yield return null;
            }
            
            // 새 클립으로 변경
            bgmSource.clip = newClip;
            bgmSource.loop = loop;
            bgmSource.Play();
            
            // 페이드 인
            elapsedTime = 0f;
            float targetVolume = bgmVolume * masterVolume;
            while (elapsedTime < crossfadeDuration * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / (crossfadeDuration * 0.5f));
                yield return null;
            }
            
            bgmSource.volume = targetVolume;
            
            onBGMChanged?.Raise();
            OnBGMChanged.Raise(newClip);
            
            Debug.Log($"[오디오매니저] BGM 크로스페이드: {newClip.name}");
        }
        
        /// <summary>
        /// BGM 정지
        /// </summary>
        /// <param name="fadeOut">페이드 아웃 여부</param>
        public void StopBGM(bool fadeOut = true)
        {
            if (!IsPlayingBGM) return;
            
            if (fadeOut)
            {
                if (bgmFadeCoroutine != null)
                    StopCoroutine(bgmFadeCoroutine);
                bgmFadeCoroutine = StartCoroutine(FadeOutBGM());
            }
            else
            {
                bgmSource.Stop();
            }
        }
        
        /// <summary>
        /// BGM 페이드 아웃
        /// </summary>
        private IEnumerator FadeOutBGM()
        {
            float startVolume = bgmSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < crossfadeDuration)
            {
                elapsedTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / crossfadeDuration);
                yield return null;
            }
            
            bgmSource.Stop();
            bgmSource.volume = bgmVolume * masterVolume;
        }
        
        // === SFX CONTROL ===
        
        /// <summary>
        /// SFX 재생
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="volume">볼륨 (0~1)</param>
        /// <param name="pitch">피치</param>
        /// <returns>재생에 사용된 AudioSource</returns>
        public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("[오디오매니저] SFX 클립이 null입니다");
                return null;
            }
            
            AudioSource source = GetAvailableSFXSource();
            if (source == null)
            {
                Debug.LogWarning("[오디오매니저] 사용가능한 SFX 소스가 없습니다");
                return null;
            }
            
            source.clip = clip;
            source.volume = volume * sfxVolume * masterVolume;
            source.pitch = pitch;
            source.Play();
            
            activeSfxSources.Add(source);
            
            OnSFXPlayed.Raise(clip.name);
            
            Debug.Log($"[오디오매니저] SFX 재생: {clip.name}");
            return source;
        }
        
        /// <summary>
        /// 위치 기반 SFX 재생
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="position">재생 위치</param>
        /// <param name="volume">볼륨</param>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            
            AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume * masterVolume);
            OnSFXPlayed.Raise(clip.name);
        }
        
        /// <summary>
        /// 모든 SFX 정지
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var source in activeSfxSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                }
            }
            
            activeSfxSources.Clear();
            
            // 모든 소스를 사용 가능한 풀로 돌려보냄
            availableSfxSources.Clear();
            for (int i = 0; i < sfxSources.Length; i++)
            {
                availableSfxSources.Enqueue(sfxSources[i]);
            }
        }
        
        // === AMBIENT CONTROL ===
        
        /// <summary>
        /// 앰비언트 사운드 재생
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="fadeIn">페이드 인 여부</param>
        public void PlayAmbient(AudioClip clip, bool fadeIn = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("[오디오매니저] 엠비언트 클립이 null입니다");
                return;
            }
            
            if (fadeIn && ambientSource.isPlaying)
            {
                StartCoroutine(CrossfadeAmbient(clip));
            }
            else
            {
                ambientSource.clip = clip;
                ambientSource.volume = ambientVolume * masterVolume;
                ambientSource.Play();
            }
            
            Debug.Log($"[오디오매니저] 엠비언트 재생: {clip.name}");
        }
        
        /// <summary>
        /// 앰비언트 사운드 크로스페이드
        /// </summary>
        private IEnumerator CrossfadeAmbient(AudioClip newClip)
        {
            float startVolume = ambientSource.volume;
            
            // 페이드 아웃
            float elapsedTime = 0f;
            while (elapsedTime < crossfadeDuration * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / (crossfadeDuration * 0.5f));
                yield return null;
            }
            
            // 새 클립으로 변경
            ambientSource.clip = newClip;
            ambientSource.Play();
            
            // 페이드 인
            elapsedTime = 0f;
            float targetVolume = ambientVolume * masterVolume;
            while (elapsedTime < crossfadeDuration * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / (crossfadeDuration * 0.5f));
                yield return null;
            }
            
            ambientSource.volume = targetVolume;
        }
        
        /// <summary>
        /// 앰비언트 사운드 정지
        /// </summary>
        public void StopAmbient(bool fadeOut = true)
        {
            if (!ambientSource.isPlaying) return;
            
            if (fadeOut)
            {
                StartCoroutine(FadeOutAmbient());
            }
            else
            {
                ambientSource.Stop();
            }
        }
        
        /// <summary>
        /// 앰비언트 사운드 페이드 아웃
        /// </summary>
        private IEnumerator FadeOutAmbient()
        {
            float startVolume = ambientSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < crossfadeDuration)
            {
                elapsedTime += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / crossfadeDuration);
                yield return null;
            }
            
            ambientSource.Stop();
            ambientSource.volume = ambientVolume * masterVolume;
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 사용 가능한 SFX 소스 반환
        /// </summary>
        private AudioSource GetAvailableSFXSource()
        {
            if (availableSfxSources.Count > 0)
            {
                return availableSfxSources.Dequeue();
            }
            
            // 재생이 끝난 소스 찾기
            foreach (var source in sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// SFX 소스 풀 업데이트
        /// </summary>
        private void UpdateSFXSourcePool()
        {
            for (int i = activeSfxSources.Count - 1; i >= 0; i--)
            {
                var source = activeSfxSources[i];
                if (source == null || !source.isPlaying)
                {
                    activeSfxSources.RemoveAt(i);
                    if (source != null)
                    {
                        availableSfxSources.Enqueue(source);
                    }
                }
            }
        }
        
        // === VOLUME CONTROL ===
        
        /// <summary>
        /// 모든 볼륨 업데이트
        /// </summary>
        private void UpdateAllVolumes()
        {
            UpdateBGMVolume();
            UpdateSFXVolume();
            UpdateAmbientVolume();
            
            onVolumeChanged?.Raise();
            OnVolumeChanged.Raise(masterVolume);
            
            SaveAudioSettings();
        }
        
        private void UpdateBGMVolume()
        {
            if (bgmSource != null)
                bgmSource.volume = bgmVolume * masterVolume;
        }
        
        private void UpdateSFXVolume()
        {
            foreach (var source in sfxSources)
            {
                if (source != null && source.isPlaying)
                {
                    // 원래 볼륨 비율 유지하면서 업데이트
                    float originalRatio = source.volume / (sfxVolume * masterVolume);
                    source.volume = originalRatio * sfxVolume * masterVolume;
                }
            }
        }
        
        private void UpdateAmbientVolume()
        {
            if (ambientSource != null)
                ambientSource.volume = ambientVolume * masterVolume;
        }
        
        /// <summary>
        /// 오디오 설정 저장
        /// </summary>
        private void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat("Audio_MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("Audio_BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("Audio_SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("Audio_AmbientVolume", ambientVolume);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 오디오 캐시에 클립 추가 (메모리 누수 방지)
        /// </summary>
        private void AddToAudioCache(string key, AudioClip clip)
        {
            if (audioCache.ContainsKey(key)) return;
            
            // 캐시 크기 제한 확인
            if (audioCache.Count >= MAX_AUDIO_CACHE_SIZE)
            {
                // 가장 오래된 캐시 항목 제거 (간단한 FIFO 방식)
                var firstKey = System.Linq.Enumerable.First(audioCache.Keys);
                audioCache.Remove(firstKey);
                Debug.Log($"[오디오매니저] 캐시 한계 도달, 제거: {firstKey}");
            }
            
            audioCache[key] = clip;
            Debug.Log($"[오디오매니저] 캐시에 추가: {key}");
        }
        
        /// <summary>
        /// 오디오 캐시 정리
        /// </summary>
        public void ClearAudioCache()
        {
            audioCache.Clear();
            Debug.Log("[오디오매니저] 오디오 캐시 청소 완료");
        }
        
        // === SAVE SYSTEM ===
        public string GetSaveData()
        {
            AudioSaveData saveData = new AudioSaveData
            {
                masterVolume = this.masterVolume,
                bgmVolume = this.bgmVolume,
                sfxVolume = this.sfxVolume,
                ambientVolume = this.ambientVolume,
                currentBGMName = CurrentBGM?.name ?? ""
            };
            
            return JsonUtility.ToJson(saveData);
        }
        
        public void LoadSaveData(string data)
        {
            if (data.IsNullOrEmpty()) return;
            
            try
            {
                AudioSaveData saveData = JsonUtility.FromJson<AudioSaveData>(data);
                
                masterVolume = saveData.masterVolume;
                bgmVolume = saveData.bgmVolume;
                sfxVolume = saveData.sfxVolume;
                ambientVolume = saveData.ambientVolume;
                
                UpdateAllVolumes();
                
                Debug.Log("[오디오매니저] 저장 데이터 로드 성공");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[오디오매니저] 저장 데이터 로드 실패: {e.Message}");
            }
        }
    }
    
    // === SAVE DATA ===
    [System.Serializable]
    public class AudioSaveData
    {
        public float masterVolume;
        public float bgmVolume;
        public float sfxVolume;
        public float ambientVolume;
        public string currentBGMName;
    }
}