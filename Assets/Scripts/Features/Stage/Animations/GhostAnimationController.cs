using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AFKS.Features.Stage.Animations
{
    /// <summary>
    /// 귀신 등장 애니메이션을 제어하는 컨트롤러입니다.
    /// 귀신의 페이드 인, 움직임, 사라짐 등을 처리합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Stage/Animations/Ghost Animation Controller")]
    public class GhostAnimationController : MonoBehaviour
    {
        [Header("귀신 설정")]
        [SerializeField] private List<GhostData> ghosts = new List<GhostData>();
        [SerializeField] private float defaultAppearanceDuration = 2.0f;
        [SerializeField] private float defaultDisappearanceDuration = 1.5f;
        
        [Header("애니메이션 설정")]
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool enableShakeEffect = true;
        [SerializeField] private float shakeIntensity = 0.1f;
        
        [Header("상태")]
        [SerializeField] private bool isAnimationPlaying = false;
        [SerializeField] private List<GhostInstance> activeGhosts = new List<GhostInstance>();
        
        #region Unity 수명주기
        
        private void Awake()
        {
            InitializeGhosts();
        }
        
        #endregion
        
        #region 초기화
        
        private void InitializeGhosts()
        {
            // 씬에서 귀신 오브젝트들을 자동으로 찾기 (태그가 없을 때도 안전)
            GameObject[] ghostObjects = null;
            try
            {
                ghostObjects = GameObject.FindGameObjectsWithTag("Ghost");
            }
            catch (UnityException)
            {
                // 태그가 정의되지 않은 경우 조용히 스킵
                Debug.Log("GhostAnimationController: 'Ghost' 태그가 정의되지 않아 초기화 스킵");
                return;
            }

            foreach (var ghostObj in ghostObjects)
            {
                var ghostData = new GhostData
                {
                    ghostId = ghostObj.name,
                    gameObject = ghostObj,
                    spriteRenderer = ghostObj.GetComponent<SpriteRenderer>(),
                    originalPosition = ghostObj.transform.localPosition,
                    originalScale = ghostObj.transform.localScale
                };

                ghosts.Add(ghostData);

                // 초기에는 모든 귀신을 숨김
                ghostObj.SetActive(false);
            }

            Debug.Log($"GhostAnimationController: {ghosts.Count}개의 귀신 초기화 완료");
        }
        
        #endregion
        
        #region 공개 API
        
        /// <summary>
        /// 귀신 등장 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="ghostId">귀신 ID</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayGhostAppearance(string ghostId, System.Action onComplete = null)
        {
            var ghostData = FindGhostData(ghostId);
            if (ghostData == null)
            {
                Debug.LogWarning($"GhostAnimationController: {ghostId}를 찾을 수 없습니다.");
                onComplete?.Invoke();
                return;
            }
            
            if (isAnimationPlaying)
            {
                Debug.LogWarning("GhostAnimationController: 이미 애니메이션이 진행 중입니다.");
                return;
            }
            
            StartCoroutine(GhostAppearanceCoroutine(ghostData, onComplete));
        }
        
        /// <summary>
        /// 귀신 사라짐 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="ghostId">귀신 ID</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayGhostDisappearance(string ghostId, System.Action onComplete = null)
        {
            var ghostData = FindGhostData(ghostId);
            if (ghostData == null)
            {
                Debug.LogWarning($"GhostAnimationController: {ghostId}를 찾을 수 없습니다.");
                onComplete?.Invoke();
                return;
            }
            
            StartCoroutine(GhostDisappearanceCoroutine(ghostData, onComplete));
        }
        
        /// <summary>
        /// 모든 귀신을 즉시 숨깁니다.
        /// </summary>
        public void HideAllGhosts()
        {
            foreach (var ghost in ghosts)
            {
                if (ghost.gameObject != null)
                {
                    ghost.gameObject.SetActive(false);
                }
            }
            
            activeGhosts.Clear();
            Debug.Log("GhostAnimationController: 모든 귀신 숨김");
        }
        
        /// <summary>
        /// 특정 귀신을 즉시 숨깁니다.
        /// </summary>
        /// <param name="ghostId">귀신 ID</param>
        public void HideGhostImmediate(string ghostId)
        {
            var ghostData = FindGhostData(ghostId);
            if (ghostData != null)
            {
                ghostData.gameObject.SetActive(false);
                Debug.Log($"GhostAnimationController: {ghostId} 즉시 숨김");
            }
        }
        
        #endregion
        
        #region 애니메이션 코루틴
        
        private IEnumerator GhostAppearanceCoroutine(GhostData ghostData, System.Action onComplete)
        {
            isAnimationPlaying = true;
            
            var ghost = ghostData.gameObject;
            var spriteRenderer = ghostData.spriteRenderer;
            
            // 귀신 활성화
            ghost.SetActive(true);
            
            // 초기 상태 설정
            var color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
            
            // 시작 위치 설정 (약간 위에서)
            var startPos = ghostData.originalPosition + Vector3.up * 2f;
            var endPos = ghostData.originalPosition;
            ghost.transform.localPosition = startPos;
            
            Debug.Log($"GhostAnimationController: {ghostData.ghostId} 등장 시작");
            
            // 페이드 인 + 이동 애니메이션
            float elapsedTime = 0f;
            while (elapsedTime < defaultAppearanceDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / defaultAppearanceDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                
                // 알파값 보간
                color.a = curveValue;
                spriteRenderer.color = color;
                
                // 위치 보간
                ghost.transform.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
                
                // 흔들림 효과
                if (enableShakeEffect)
                {
                    var shakeOffset = Random.insideUnitSphere * shakeIntensity * (1f - curveValue);
                    ghost.transform.localPosition += shakeOffset;
                }
                
                yield return null;
            }
            
            // 최종 상태 설정
            color.a = 1f;
            spriteRenderer.color = color;
            ghost.transform.localPosition = endPos;
            
            // 활성 귀신 목록에 추가
            var ghostInstance = new GhostInstance
            {
                ghostData = ghostData,
                isVisible = true
            };
            activeGhosts.Add(ghostInstance);
            
            isAnimationPlaying = false;
            
            Debug.Log($"GhostAnimationController: {ghostData.ghostId} 등장 완료");
            onComplete?.Invoke();
        }
        
        private IEnumerator GhostDisappearanceCoroutine(GhostData ghostData, System.Action onComplete)
        {
            var ghost = ghostData.gameObject;
            var spriteRenderer = ghostData.spriteRenderer;
            
            Debug.Log($"GhostAnimationController: {ghostData.ghostId} 사라짐 시작");
            
            // 페이드 아웃 애니메이션
            float elapsedTime = 0f;
            var startColor = spriteRenderer.color;
            var endColor = startColor;
            endColor.a = 0f;
            
            while (elapsedTime < defaultDisappearanceDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / defaultDisappearanceDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                
                // 알파값 보간
                spriteRenderer.color = Color.Lerp(startColor, endColor, curveValue);
                
                // 흔들림 효과
                if (enableShakeEffect)
                {
                    var shakeOffset = Random.insideUnitSphere * shakeIntensity * curveValue;
                    ghost.transform.localPosition = ghostData.originalPosition + shakeOffset;
                }
                
                yield return null;
            }
            
            // 귀신 비활성화
            ghost.SetActive(false);
            ghost.transform.localPosition = ghostData.originalPosition;
            
            // 활성 귀신 목록에서 제거
            activeGhosts.RemoveAll(g => g.ghostData.ghostId == ghostData.ghostId);
            
            Debug.Log($"GhostAnimationController: {ghostData.ghostId} 사라짐 완료");
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region 유틸리티
        
        private GhostData FindGhostData(string ghostId)
        {
            return ghosts.Find(g => g.ghostId == ghostId);
        }
        
        /// <summary>
        /// 현재 애니메이션이 진행 중인지 확인합니다.
        /// </summary>
        public bool IsAnimationPlaying => isAnimationPlaying;
        
        /// <summary>
        /// 활성 귀신 목록을 반환합니다.
        /// </summary>
        public List<GhostInstance> GetActiveGhosts() => new List<GhostInstance>(activeGhosts);
        
        /// <summary>
        /// 특정 귀신이 활성 상태인지 확인합니다.
        /// </summary>
        /// <param name="ghostId">귀신 ID</param>
        public bool IsGhostActive(string ghostId)
        {
            return activeGhosts.Exists(g => g.ghostData.ghostId == ghostId);
        }
        
        #endregion
        
        #region 유틸리티
        
        /// <summary>
        /// 모든 애니메이션을 중지합니다.
        /// </summary>
        public void StopAnimation()
        {
            StopAllCoroutines();
            isAnimationPlaying = false;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 귀신의 기본 데이터를 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class GhostData
    {
        public string ghostId;
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public Vector3 originalPosition;
        public Vector3 originalScale;
    }
    
    /// <summary>
    /// 현재 활성 상태인 귀신 인스턴스를 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class GhostInstance
    {
        public GhostData ghostData;
        public bool isVisible;
        public float appearanceTime;
    }
}
