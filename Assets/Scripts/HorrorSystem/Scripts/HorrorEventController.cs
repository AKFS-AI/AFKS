using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using AFKS.Shared.Interfaces;
using AFKS.Shared.Utils;
using AFKS.StageSystem;

namespace AFKS.HorrorSystem
{
    /// <summary>
    /// 개별 공포 이벤트를 실행하는 컨트롤러
    /// </summary>
    public class HorrorEventController : MonoBehaviour, IHorrorEvent
    {
        [Header("😱 이벤트 데이터")]
        [SerializeField, Tooltip("실행할 공포 이벤트 데이터")] private HorrorEventData eventData;
        
        [Header("⚡ 실행 상태")]
        [SerializeField, Tooltip("현재 이벤트가 실행 중인지")] private bool isExecuting = false;
        [SerializeField, Tooltip("이벤트가 완료되었는지")] private bool isCompleted = false;
        
        // === COMPONENTS ===
        private Image jumpscareImage;
        private AudioSource audioSource;
        private RectTransform rectTransform;
        
        // === PROPERTIES ===
        public HorrorEventData EventData => eventData;
        public bool IsExecuting => isExecuting;
        
        // === INITIALIZATION ===
        
        /// <summary>
        /// 이벤트 컨트롤러 초기화
        /// </summary>
        /// <param name="data">공포 이벤트 데이터</param>
        public void Initialize(HorrorEventData data)
        {
            eventData = data;
            SetupComponents();
            SetupVisual();
            SetupAudio();
        }
        
        /// <summary>
        /// 컴포넌트 설정
        /// </summary>
        private void SetupComponents()
        {
            // RectTransform 설정
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = gameObject.AddComponent<RectTransform>();
            
            // Image 컴포넌트 추가
            jumpscareImage = gameObject.AddComponent<Image>();
            
            // AudioSource 컴포넌트 추가
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        /// <summary>
        /// 시각적 요소 설정
        /// </summary>
        private void SetupVisual()
        {
            if (eventData?.jumpscareImage == null) return;
            
            // 이미지 설정
            jumpscareImage.sprite = eventData.jumpscareImage;
            jumpscareImage.color = Color.clear; // 초기에는 투명
            
            // 위치 및 크기 설정
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = eventData.imagePosition;
            rectTransform.sizeDelta = eventData.imageSize;
            
            // 초기에는 비활성화
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 오디오 설정
        /// </summary>
        private void SetupAudio()
        {
            if (eventData?.horrorSound == null) return;
            
            audioSource.clip = eventData.horrorSound;
            audioSource.volume = eventData.volume;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
        
        // === IHOHOR_EVENT IMPLEMENTATION ===
        
        public void Trigger()
        {
            if (!IsReady()) return;
            
            StartCoroutine(ExecuteEvent());
        }
        
        public bool IsReady()
        {
            return !isExecuting && !isCompleted && eventData != null;
        }
        
        public bool IsCompleted()
        {
            return isCompleted;
        }
        
        public void Reset()
        {
            isExecuting = false;
            isCompleted = false;
            
            if (jumpscareImage != null)
                jumpscareImage.color = Color.clear;
            
            gameObject.SetActive(false);
        }
        
        // === EVENT EXECUTION ===
        
        /// <summary>
        /// 공포 이벤트 실행
        /// </summary>
        public IEnumerator ExecuteEvent()
        {
            if (!IsReady()) yield break;
            
            isExecuting = true;
            
            Debug.Log($"[공포이벤트컨트롤러] 이벤트 실행: {eventData.eventName}");
            
            // 게임 오브젝트 활성화
            gameObject.SetActive(true);
            
            // 화면 흔들림 시작
            if (eventData.enableScreenShake)
            {
                StartCoroutine(ScreenShakeEffect());
            }
            
            // 오디오 재생
            if (audioSource != null && eventData.horrorSound != null)
            {
                audioSource.Play();
            }
            
            // 점프스케어 이미지 표시
            yield return StartCoroutine(ShowJumpscareImage());
            
            // 표시 시간만큼 대기
            yield return new WaitForSeconds(eventData.displayDuration);
            
            // 페이드 아웃
            yield return StartCoroutine(FadeOutJumpscare());
            
            // 이벤트 완료
            isExecuting = false;
            isCompleted = true;
            
            Debug.Log($"[공포이벤트컨트롤러] 이벤트 완료: {eventData.eventName}");
        }
        
        /// <summary>
        /// 이벤트 중단
        /// </summary>
        public void StopEvent()
        {
            if (!isExecuting) return;
            
            StopAllCoroutines();
            
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            isExecuting = false;
            gameObject.SetActive(false);
            
            Debug.Log($"[공포이벤트컨트롤러] 이벤트 중지: {eventData.eventName}");
        }
        
        // === VISUAL EFFECTS ===
        
        /// <summary>
        /// 점프스케어 이미지 표시
        /// </summary>
        private IEnumerator ShowJumpscareImage()
        {
            if (jumpscareImage == null) yield break;
            
            float fadeInDuration = 0.1f; // 빠른 페이드 인
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = elapsedTime / fadeInDuration;
                
                Color color = jumpscareImage.color;
                color.a = alpha;
                jumpscareImage.color = color;
                
                yield return null;
            }
            
            // 완전히 불투명하게
            Color finalColor = jumpscareImage.color;
            finalColor.a = 1f;
            jumpscareImage.color = finalColor;
        }
        
        /// <summary>
        /// 점프스케어 페이드 아웃
        /// </summary>
        private IEnumerator FadeOutJumpscare()
        {
            if (jumpscareImage == null) yield break;
            
            float fadeOutDuration = 0.5f;
            float elapsedTime = 0f;
            Color startColor = jumpscareImage.color;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = 1f - (elapsedTime / fadeOutDuration);
                
                Color color = startColor;
                color.a = alpha;
                jumpscareImage.color = color;
                
                yield return null;
            }
            
            // 완전히 투명하게
            Color finalColor = jumpscareImage.color;
            finalColor.a = 0f;
            jumpscareImage.color = finalColor;
            
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 화면 흔들림 효과
        /// </summary>
        private IEnumerator ScreenShakeEffect()
        {
            float duration = eventData.shakeDuration;
            float intensity = eventData.shakeIntensity;
            float elapsedTime = 0f;
            
            Vector3 originalPosition = rectTransform.anchoredPosition;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                
                // 감쇠 효과
                float damping = 1f - (elapsedTime / duration);
                float currentIntensity = intensity * damping;
                
                // 랜덤 오프셋 계산
                Vector2 randomOffset = new Vector2(
                    Random.Range(-currentIntensity, currentIntensity),
                    Random.Range(-currentIntensity, currentIntensity)
                ) * 100f; // UI 단위로 변환
                
                rectTransform.anchoredPosition = originalPosition + randomOffset.ToVector3();
                
                yield return null;
            }
            
            // 원래 위치로 복원
            rectTransform.anchoredPosition = originalPosition;
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 이벤트 데이터 업데이트
        /// </summary>
        public void UpdateEventData(HorrorEventData newData)
        {
            eventData = newData;
            SetupVisual();
            SetupAudio();
        }
        
        /// <summary>
        /// 진행률 반환
        /// </summary>
        /// <returns>0~1 사이의 진행률</returns>
        public float GetProgress()
        {
            if (!isExecuting) return isCompleted ? 1f : 0f;
            
            // 실행 중인 경우 대략적인 진행률 계산
            return 0.5f; // 임시값
        }
        
        // === DEBUG ===
        
        private void OnValidate()
        {
            if (eventData != null && gameObject.name != $"HorrorEvent_{eventData.eventId}")
            {
                gameObject.name = $"HorrorEvent_{eventData.eventId}";
            }
        }
    }
}