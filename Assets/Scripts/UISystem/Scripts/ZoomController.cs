using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using AFKS.Shared.Events;

namespace AFKS.UISystem
{
    /// <summary>
    /// 이미지 확대/축소 기능을 제공하는 컨트롤러
    /// 병원 철문 → 쇠사슬 상호작용을 위한 줌 시스템
    /// </summary>
    public class ZoomController : MonoBehaviour
    {
        [Header("🔍 줌 설정")]
        [SerializeField, Range(1f, 5f), Tooltip("최대 확대 배율")] private float maxZoomScale = 3f;
        [SerializeField, Range(0.1f, 2f), Tooltip("줌 애니메이션 시간")] private float zoomDuration = 0.8f;
        [SerializeField, Tooltip("줌 대상 이미지")] private Image targetImage;
        [SerializeField, Tooltip("줌 시 배경 어둡게 처리")] private Image backgroundOverlay;
        
        [Header("🎨 시각적 설정")]
        [SerializeField, Tooltip("배경 어둡게 처리 색상")] private Color overlayColor = new Color(0, 0, 0, 0.7f);
        [SerializeField, Tooltip("줌 애니메이션 곡선")] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("🔊 오디오")]
        [SerializeField, Tooltip("줌 인 사운드")] private AudioClip zoomInSound;
        [SerializeField, Tooltip("줌 아웃 사운드")] private AudioClip zoomOutSound;
        
        // === EVENTS ===
        public static readonly GameEvent<bool> OnZoomStateChanged = new GameEvent<bool>();
        public static readonly GameEvent<string> OnZoomCompleted = new GameEvent<string>();
        
        // === PROPERTIES ===
        public bool IsZoomed { get; private set; }
        public bool IsZooming { get; private set; }
        public Image TargetImage => targetImage;
        
        // === PRIVATE FIELDS ===
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private Vector3 targetZoomPosition;
        private Canvas parentCanvas;
        private RectTransform imageRectTransform;
        private Coroutine zoomCoroutine;
        
        // === SINGLETON ACCESS ===
        private static ZoomController instance;
        public static ZoomController Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<ZoomController>();
                return instance;
            }
        }
        
        // === UNITY LIFECYCLE ===
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            SetupComponents();
        }
        
        private void Start()
        {
            InitializeZoomController();
        }
        
        private void Update()
        {
            // ESC 키로 줌 해제
            if (IsZoomed && Input.GetKeyDown(KeyCode.Escape))
            {
                ZoomOut();
            }
        }
        
        // === INITIALIZATION ===
        
        /// <summary>
        /// 컴포넌트 설정
        /// </summary>
        private void SetupComponents()
        {
            // Canvas 찾기
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                parentCanvas = FindFirstObjectByType<Canvas>();
            }
            
            // 배경 오버레이 설정
            if (backgroundOverlay == null)
            {
                // 자동으로 배경 오버레이 생성
                GameObject overlayGO = new GameObject("ZoomBackgroundOverlay");
                overlayGO.transform.SetParent(transform, false);
                
                backgroundOverlay = overlayGO.AddComponent<Image>();
                backgroundOverlay.color = Color.clear;
                backgroundOverlay.raycastTarget = true;
                
                // 전체 화면 덮도록 설정
                RectTransform overlayRect = backgroundOverlay.rectTransform;
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.sizeDelta = Vector2.zero;
                overlayRect.anchoredPosition = Vector2.zero;
                
                // 클릭 시 줌 아웃
                Button overlayButton = overlayGO.AddComponent<Button>();
                overlayButton.onClick.AddListener(ZoomOut);
                
                overlayGO.SetActive(false);
            }
        }
        
        /// <summary>
        /// 줌 컨트롤러 초기화
        /// </summary>
        private void InitializeZoomController()
        {
            if (targetImage != null)
            {
                imageRectTransform = targetImage.rectTransform;
                originalScale = imageRectTransform.localScale;
                originalPosition = imageRectTransform.anchoredPosition;
            }
            
            Debug.Log("[ZoomController] 초기화 완료");
        }
        
        // === PUBLIC METHODS ===
        
        /// <summary>
        /// 특정 이미지를 확대
        /// </summary>
        /// <param name="image">확대할 이미지</param>
        /// <param name="zoomScale">확대 배율 (기본값: maxZoomScale)</param>
        /// <param name="focusPoint">확대 중심점 (기본값: 이미지 중앙)</param>
        public void ZoomToImage(Image image, float zoomScale = 0f, Vector2? focusPoint = null)
        {
            if (IsZooming || IsZoomed) return;
            
            targetImage = image;
            imageRectTransform = image.rectTransform;
            originalScale = imageRectTransform.localScale;
            originalPosition = imageRectTransform.anchoredPosition;
            
            if (zoomScale <= 0f) zoomScale = maxZoomScale;
            
            // 중심점 설정
            Vector2 center = focusPoint ?? Vector2.zero;
            targetZoomPosition = originalPosition - (Vector3)center * (zoomScale - 1f);
            
            StartZoom(zoomScale, true);
        }
        
        /// <summary>
        /// 현재 이미지 확대
        /// </summary>
        public void ZoomIn(Vector2? focusPoint = null)
        {
            if (targetImage == null || IsZooming || IsZoomed) return;
            
            Vector2 center = focusPoint ?? Vector2.zero;
            targetZoomPosition = originalPosition - (Vector3)center * (maxZoomScale - 1f);
            
            StartZoom(maxZoomScale, true);
        }
        
        /// <summary>
        /// 줌 해제
        /// </summary>
        public void ZoomOut()
        {
            if (!IsZoomed || IsZooming) return;
            
            StartZoom(1f, false);
        }
        
        /// <summary>
        /// 줌 토글
        /// </summary>
        public void ToggleZoom()
        {
            if (IsZoomed)
                ZoomOut();
            else
                ZoomIn();
        }
        
        // === PRIVATE METHODS ===
        
        /// <summary>
        /// 줌 애니메이션 시작
        /// </summary>
        private void StartZoom(float targetScale, bool isZoomIn)
        {
            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);
                
            zoomCoroutine = StartCoroutine(ZoomCoroutine(targetScale, isZoomIn));
        }
        
        /// <summary>
        /// 줌 애니메이션 코루틴
        /// </summary>
        private IEnumerator ZoomCoroutine(float targetScale, bool isZoomIn)
        {
            IsZooming = true;
            
            // 줌 시작 시 배경 오버레이 활성화
            if (isZoomIn && backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(true);
                backgroundOverlay.color = Color.clear;
            }
            
            // 사운드 재생
            if (isZoomIn && zoomInSound != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(zoomInSound);
            }
            else if (!isZoomIn && zoomOutSound != null && AFKS.AudioSystem.AudioManager.Instance != null)
            {
                AFKS.AudioSystem.AudioManager.Instance.PlaySFX(zoomOutSound);
            }
            
            // 애니메이션 시작값
            Vector3 startScale = imageRectTransform.localScale;
            Vector3 startPosition = imageRectTransform.anchoredPosition;
            
            Vector3 endScale = originalScale * targetScale;
            Vector3 endPosition = isZoomIn ? targetZoomPosition : originalPosition;
            
            Color startOverlayColor = backgroundOverlay != null ? backgroundOverlay.color : Color.clear;
            Color endOverlayColor = isZoomIn ? overlayColor : Color.clear;
            
            // 애니메이션 실행
            float elapsed = 0f;
            while (elapsed < zoomDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / zoomDuration;
                float curveValue = zoomCurve.Evaluate(t);
                
                // 스케일과 위치 애니메이션
                imageRectTransform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
                imageRectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, curveValue);
                
                // 배경 오버레이 애니메이션
                if (backgroundOverlay != null)
                {
                    backgroundOverlay.color = Color.Lerp(startOverlayColor, endOverlayColor, t);
                }
                
                yield return null;
            }
            
            // 최종값 설정
            imageRectTransform.localScale = endScale;
            imageRectTransform.anchoredPosition = endPosition;
            
            if (backgroundOverlay != null)
            {
                backgroundOverlay.color = endOverlayColor;
                
                // 줌 아웃 완료 시 배경 오버레이 비활성화
                if (!isZoomIn)
                {
                    backgroundOverlay.gameObject.SetActive(false);
                }
            }
            
            // 상태 업데이트
            IsZoomed = isZoomIn;
            IsZooming = false;
            
            // 이벤트 발생
            OnZoomStateChanged.Raise(IsZoomed);
            OnZoomCompleted.Raise(targetImage?.name ?? "Unknown");
            
            Debug.Log($"[ZoomController] 줌 {(isZoomIn ? "확대" : "축소")} 완료: {targetImage?.name}");
        }
        
        /// <summary>
        /// 현재 줌 상태 리셋
        /// </summary>
        public void ResetZoom()
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }
            
            if (imageRectTransform != null)
            {
                imageRectTransform.localScale = originalScale;
                imageRectTransform.anchoredPosition = originalPosition;
            }
            
            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(false);
                backgroundOverlay.color = Color.clear;
            }
            
            IsZoomed = false;
            IsZooming = false;
            
            OnZoomStateChanged.Raise(false);
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보 출력")]
        public void PrintDebugInfo()
        {
            Debug.Log($"=== ZoomController Debug Info ===");
            Debug.Log($"Is Zoomed: {IsZoomed}");
            Debug.Log($"Is Zooming: {IsZooming}");
            Debug.Log($"Target Image: {targetImage?.name ?? "null"}");
            Debug.Log($"Max Zoom Scale: {maxZoomScale}");
            Debug.Log($"Current Scale: {imageRectTransform?.localScale ?? Vector3.zero}");
        }
        
        // === CLEANUP ===
        
        private void OnDestroy()
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
            
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}