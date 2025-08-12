using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using AFKS.Shared.Events;
using AFKS.Shared.Interfaces;
using AFKS.Shared.Utils;
using AFKS.Shared.Core;

namespace AFKS.AudioSystem
{
    /// <summary>
    /// 오디오 시스템을 관리하는 매니저
    /// BaseSingleton을 상속받아 싱글톤 패턴 구현
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : BaseSingleton<AudioManager>, ISaveable
    {
                [Header("🔊 오디오 소스")]
        [SerializeField, Tooltip("배경음악(BGM) 전용 오디오 소스")] private AudioSource bgmSource;
        [SerializeField, Tooltip("효과음(SFX) 전용 오디오 소스들")] private AudioSource[] sfxSources;
        [SerializeField, Tooltip("환경음(Ambient) 전용 오디오 소스")] private AudioSource ambientSource;
        
        [Header("⚙️ 설정")]
        [SerializeField, Range(1, 16), Tooltip("동시 재생 가능한 최대 효과음 개수")] private int maxSfxSources = 8;
        [SerializeField, Range(0.1f, 5f), Tooltip("BGM 크로스페이드 전환 시간 (초)")] private float crossfadeDuration = 1f;
        
        [Header("🎵 볼륨 조절")]
        [SerializeField, Range(0f, 1f), Tooltip("전체 마스터 볼륨")] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("배경음악 볼륨")] private float bgmVolume = 0.7f;
        [SerializeField, Range(0f, 1f), Tooltip("효과음 볼륨")] private float sfxVolume = 0.8f;
        [SerializeField, Range(0f, 1f), Tooltip("환경음 볼륨")] private float ambientVolume = 0.5f;
        
        // 이벤트는 전역 EventBus 또는 정적 GameEvent<T>를 사용
        
        // === RUNTIME EVENTS ===
        [System.Obsolete("Use EventBus.BGMChanged instead")] public static readonly GameEvent<AudioClip> OnBGMChanged = new GameEvent<AudioClip>();
        [System.Obsolete("Use EventBus.SFXPlayed instead")] public static readonly GameEvent<string> OnSFXPlayed = new GameEvent<string>();
        [System.Obsolete("Use EventBus.VolumeChanged instead")] public static readonly GameEvent<float> OnVolumeChanged = new GameEvent<float>();
        
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
        private readonly Queue<string> audioCacheKeys = new Queue<string>();
        private Queue<AudioSource> availableSfxSources = new Queue<AudioSource>();
        private List<AudioSource> activeSfxSources = new List<AudioSource>();
        private Coroutine bgmFadeCoroutine;
        
        // === CACHE MANAGEMENT ===
        private int MAX_AUDIO_CACHE_SIZE = 50; // 최대 캐시 크기 제한 (GameConfig로 덮어쓰기 가능)
        
        // === SINGLETON - BaseSingleton<T>에서 자동 관리됨 ===
        
        // === UNITY LIFECYCLE ===
        protected override void OnSingletonAwake()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.Register(this);
            }
            InitializeAudioManager();
        }
        
        private void Update()
        {
            UpdateSFXSourcePool();
        }
        
        // === INITIALIZATION ===
        private void InitializeAudioManager()
        {
            CreateAudioSources();
            // 저장 데이터가 등록 과정에서 로드되었으므로 현재 값으로 볼륨 적용
            ApplyDefaultsFromConfigIfUnset();
            // GameConfig 반영(가능한 항목)
            var gm = AFKS.Core.GameManager.Instance;
            if (gm != null && gm.Config != null)
            {
                MAX_AUDIO_CACHE_SIZE = gm.Config.MaxAudioCacheSize;
                // crossfadeDuration은 Config에 별도 필드가 없으므로 Constants로 보정
                if (Mathf.Approximately(crossfadeDuration, 1f))
                {
                    crossfadeDuration = AFKS.Shared.Utils.Constants.AUDIO_FADE_DURATION;
                }
            }
            UpdateAllVolumes();
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
            
            OnBGMChanged.Raise(clip);
            AFKS.Shared.Events.EventBus.BGMChanged.Raise(clip);
            
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
            
            OnBGMChanged.Raise(newClip);
            AFKS.Shared.Events.EventBus.BGMChanged.Raise(newClip);
            
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
            AFKS.Shared.Events.EventBus.SFXPlayed.Raise(clip.name);
            
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
            AFKS.Shared.Events.EventBus.SFXPlayed.Raise(clip.name);
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
        private float pendingSaveTimer = -1f;
        private const float SAVE_DEBOUNCE_SECONDS = 0.3f;

        private void UpdateAllVolumes()
        {
            UpdateBGMVolume();
            UpdateSFXVolume();
            UpdateAmbientVolume();
            
            OnVolumeChanged.Raise(masterVolume);
            AFKS.Shared.Events.EventBus.VolumeChanged.Raise(masterVolume);
            
            // 빈번한 저장을 디바운스하여 부하 감소
            pendingSaveTimer = SAVE_DEBOUNCE_SECONDS;
        }

        private void LateUpdate()
        {
            if (pendingSaveTimer >= 0f)
            {
                pendingSaveTimer -= Time.unscaledDeltaTime;
                if (pendingSaveTimer < 0f && AFKS.Shared.Core.SaveManager.HasInstance)
                {
                    AFKS.Shared.Core.SaveManager.Instance.SaveAll();
                }
            }
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
        /// 오디오 캐시에 클립 추가 (메모리 누수 방지)
        /// </summary>
        private void AddToAudioCache(string key, AudioClip clip)
        {
            if (audioCache.ContainsKey(key)) return;
            
            // 캐시 크기 제한 확인
            if (audioCache.Count >= MAX_AUDIO_CACHE_SIZE)
            {
                // 가장 오래된 캐시 항목 제거 (FIFO)
                if (audioCacheKeys.Count > 0)
                {
                    var oldestKey = audioCacheKeys.Dequeue();
                    audioCache.Remove(oldestKey);
                    Debug.Log($"[오디오매니저] 캐시 한계 도달, 제거: {oldestKey}");
                }
            }
            
            audioCache[key] = clip;
            audioCacheKeys.Enqueue(key);
            Debug.Log($"[오디오매니저] 캐시에 추가: {key}");
        }
        
        /// <summary>
        /// 오디오 캐시 정리
        /// </summary>
        public void ClearAudioCache()
        {
            audioCache.Clear();
            audioCacheKeys.Clear();
            Debug.Log("[오디오매니저] 오디오 캐시 청소 완료");
        }

        // === CLEANUP ===
        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 정적 이벤트 리스너 정리
            OnBGMChanged.RemoveAllListeners();
            OnSFXPlayed.RemoveAllListeners();
            OnVolumeChanged.RemoveAllListeners();
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

        // GameConfig 적용(선택): 초기 기본 볼륨 세팅을 GameConfig에서 가져오도록 확장 가능
        private void ApplyDefaultsFromConfigIfUnset()
        {
            // 저장 데이터가 없을 때만 기본값을 GameConfig에서 가져와 초기화
            if (!AFKS.Shared.Core.SaveManager.HasInstance) return;
            // 간단 기준: 볼륨들이 기본 초기값(1/0.7/0.8/0.5)과 동일할 때만 Config 덮어쓰기
            var gm = AFKS.Core.GameManager.Instance;
            if (gm == null || gm.Config == null) return;
            var cfg = gm.Config;
            // 마스터는 1 기본, 나머지는 Config 기본 적용
            if (Mathf.Approximately(bgmVolume, 0.7f)) bgmVolume = cfg.DefaultBGMVolume;
            if (Mathf.Approximately(sfxVolume, 0.8f)) sfxVolume = cfg.DefaultSFXVolume;
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