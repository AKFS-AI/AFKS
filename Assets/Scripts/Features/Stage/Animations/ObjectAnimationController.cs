using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AFKS.Features.Stage.Animations
{
    /// <summary>
    /// 오브젝트의 다양한 애니메이션을 제어하는 컨트롤러입니다.
    /// 페이드, 스케일, 이동, 회전 등을 처리합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Stage/Animations/Object Animation Controller")]
    public class ObjectAnimationController : MonoBehaviour
    {
        [Header("애니메이션 설정")]
        [SerializeField] private float defaultDuration = 1.0f;
        [SerializeField] private AnimationCurve defaultCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("상태")]
        [SerializeField] private bool isAnimationPlaying = false;
        [SerializeField] private List<ObjectAnimationData> activeAnimations = new List<ObjectAnimationData>();
        
        private Dictionary<string, GameObject> trackedObjects = new Dictionary<string, GameObject>();
        
        #region Unity 수명주기
        
        private void Awake()
        {
            InitializeTrackedObjects();
        }
        
        #endregion
        
        #region 초기화
        
        private void InitializeTrackedObjects()
        {
            // 씬에서 애니메이션 가능한 오브젝트들을 자동으로 찾기
            var animatableObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in animatableObjects)
            {
                // 특정 태그나 컴포넌트를 가진 오브젝트만 추적
                if (obj.GetComponent<SpriteRenderer>() != null || obj.GetComponent<Renderer>() != null)
                {
                    trackedObjects[obj.name] = obj;
                }
            }
            
            Debug.Log($"ObjectAnimationController: {trackedObjects.Count}개의 오브젝트 추적 시작");
        }
        
        #endregion
        
        #region 공개 API
        
        /// <summary>
        /// 오브젝트 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="objectId">애니메이션할 오브젝트 ID</param>
        /// <param name="animationType">애니메이션 타입</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayAnimation(string objectId, ObjectAnimationType animationType, System.Action onComplete = null)
        {
            if (!trackedObjects.ContainsKey(objectId))
            {
                Debug.LogWarning($"ObjectAnimationController: {objectId}를 찾을 수 없습니다.");
                onComplete?.Invoke();
                return;
            }
            
            var targetObject = trackedObjects[objectId];
            if (targetObject == null)
            {
                Debug.LogWarning($"ObjectAnimationController: {objectId}가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            
            switch (animationType)
            {
                case ObjectAnimationType.FadeIn:
                    StartCoroutine(FadeInCoroutine(targetObject, onComplete));
                    break;
                case ObjectAnimationType.FadeOut:
                    StartCoroutine(FadeOutCoroutine(targetObject, onComplete));
                    break;
                case ObjectAnimationType.Scale:
                    StartCoroutine(ScaleCoroutine(targetObject, Vector3.one, onComplete));
                    break;
                case ObjectAnimationType.Move:
                    StartCoroutine(MoveCoroutine(targetObject, Vector3.zero, onComplete));
                    break;
                case ObjectAnimationType.Rotate:
                    StartCoroutine(RotateCoroutine(targetObject, Vector3.zero, onComplete));
                    break;
                default:
                    Debug.LogWarning($"ObjectAnimationController: 지원하지 않는 애니메이션 타입 - {animationType}");
                    onComplete?.Invoke();
                    break;
            }
        }
        
        /// <summary>
        /// 페이드 인 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayFadeIn(string objectId, float duration = -1f, System.Action onComplete = null)
        {
            if (!trackedObjects.ContainsKey(objectId)) return;
            
            var targetObject = trackedObjects[objectId];
            StartCoroutine(FadeInCoroutine(targetObject, onComplete, duration));
        }
        
        /// <summary>
        /// 페이드 아웃 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayFadeOut(string objectId, float duration = -1f, System.Action onComplete = null)
        {
            if (!trackedObjects.ContainsKey(objectId)) return;
            
            var targetObject = trackedObjects[objectId];
            StartCoroutine(FadeOutCoroutine(targetObject, onComplete, duration));
        }
        
        /// <summary>
        /// 스케일 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        /// <param name="targetScale">타겟 스케일</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayScale(string objectId, Vector3 targetScale, float duration = -1f, System.Action onComplete = null)
        {
            if (!trackedObjects.ContainsKey(objectId)) return;
            
            var targetObject = trackedObjects[objectId];
            StartCoroutine(ScaleCoroutine(targetObject, targetScale, onComplete, duration));
        }
        
        /// <summary>
        /// 이동 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        /// <param name="targetPosition">타겟 위치</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayMove(string objectId, Vector3 targetPosition, float duration = -1f, System.Action onComplete = null)
        {
            if (!trackedObjects.ContainsKey(objectId)) return;
            
            var targetObject = trackedObjects[objectId];
            StartCoroutine(MoveCoroutine(targetObject, targetPosition, onComplete, duration));
        }
        
        /// <summary>
        /// 회전 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        /// <param name="targetRotation">타겟 회전</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="onComplete">완료 시 콜백</param>
        public void PlayRotate(string objectId, Vector3 targetRotation, float duration = -1f, System.Action onComplete = null)
        {
            if (!trackedObjects.ContainsKey(objectId)) return;
            
            var targetObject = trackedObjects[objectId];
            StartCoroutine(RotateCoroutine(targetObject, targetRotation, onComplete, duration));
        }
        
        #endregion
        
        #region 애니메이션 코루틴
        
        private IEnumerator FadeInCoroutine(GameObject targetObject, System.Action onComplete, float duration = -1f)
        {
            if (duration < 0f) duration = defaultDuration;
            
            var renderer = targetObject.GetComponent<Renderer>();
            if (renderer == null) yield break;
            
            var material = renderer.material;
            var color = material.color;
            var startAlpha = 0f;
            var endAlpha = 1f;
            
            // 초기 상태 설정
            color.a = startAlpha;
            material.color = color;
            targetObject.SetActive(true);
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float curveValue = defaultCurve.Evaluate(progress);
                
                color.a = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                material.color = color;
                
                yield return null;
            }
            
            // 최종 상태 설정
            color.a = endAlpha;
            material.color = color;
            
            onComplete?.Invoke();
        }
        
        private IEnumerator FadeOutCoroutine(GameObject targetObject, System.Action onComplete, float duration = -1f)
        {
            if (duration < 0f) duration = defaultDuration;
            
            var renderer = targetObject.GetComponent<Renderer>();
            if (renderer == null) yield break;
            
            var material = renderer.material;
            var color = material.color;
            var startAlpha = color.a;
            var endAlpha = 0f;
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float curveValue = defaultCurve.Evaluate(progress);
                
                color.a = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                material.color = color;
                
                yield return null;
            }
            
            // 최종 상태 설정
            color.a = endAlpha;
            material.color = color;
            targetObject.SetActive(false);
            
            onComplete?.Invoke();
        }
        
        private IEnumerator ScaleCoroutine(GameObject targetObject, Vector3 targetScale, System.Action onComplete, float duration = -1f)
        {
            if (duration < 0f) duration = defaultDuration;
            
            var startScale = targetObject.transform.localScale;
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float curveValue = defaultCurve.Evaluate(progress);
                
                targetObject.transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
                
                yield return null;
            }
            
            // 최종 상태 설정
            targetObject.transform.localScale = targetScale;
            
            onComplete?.Invoke();
        }
        
        private IEnumerator MoveCoroutine(GameObject targetObject, Vector3 targetPosition, System.Action onComplete, float duration = -1f)
        {
            if (duration < 0f) duration = defaultDuration;
            
            var startPosition = targetObject.transform.localPosition;
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float curveValue = defaultCurve.Evaluate(progress);
                
                targetObject.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
                
                yield return null;
            }
            
            // 최종 상태 설정
            targetObject.transform.localPosition = targetPosition;
            
            onComplete?.Invoke();
        }
        
        private IEnumerator RotateCoroutine(GameObject targetObject, Vector3 targetRotation, System.Action onComplete, float duration = -1f)
        {
            if (duration < 0f) duration = defaultDuration;
            
            var startRotation = targetObject.transform.localEulerAngles;
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float curveValue = defaultCurve.Evaluate(progress);
                
                targetObject.transform.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, curveValue);
                
                yield return null;
            }
            
            // 최종 상태 설정
            targetObject.transform.localEulerAngles = targetRotation;
            
            onComplete?.Invoke();
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
        
        /// <summary>
        /// 특정 오브젝트를 추적 목록에 추가합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        /// <param name="gameObject">게임 오브젝트</param>
        public void AddTrackedObject(string objectId, GameObject gameObject)
        {
            if (!trackedObjects.ContainsKey(objectId))
            {
                trackedObjects[objectId] = gameObject;
                Debug.Log($"ObjectAnimationController: {objectId} 추적 목록에 추가됨");
            }
        }
        
        /// <summary>
        /// 특정 오브젝트를 추적 목록에서 제거합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        public void RemoveTrackedObject(string objectId)
        {
            if (trackedObjects.ContainsKey(objectId))
            {
                trackedObjects.Remove(objectId);
                Debug.Log($"ObjectAnimationController: {objectId} 추적 목록에서 제거됨");
            }
        }
        
        /// <summary>
        /// 현재 애니메이션이 진행 중인지 확인합니다.
        /// </summary>
        public bool IsAnimationPlaying => isAnimationPlaying;
        
        /// <summary>
        /// 추적 중인 오브젝트 목록을 반환합니다.
        /// </summary>
        public List<string> GetTrackedObjectIds() => new List<string>(trackedObjects.Keys);
        
        #endregion
    }
    
    /// <summary>
    /// 오브젝트 애니메이션 데이터를 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ObjectAnimationData
    {
        public string objectId;
        public ObjectAnimationType animationType;
        public bool isPlaying;
        public float startTime;
        public float duration;
    }
}
