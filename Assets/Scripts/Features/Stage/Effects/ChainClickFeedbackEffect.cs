using UnityEngine;
using AFKS.Features.Stage;

namespace AFKS.Features.Stage.Effects
{
    /// <summary>
    /// 오브젝트 클릭 시 즉시 실행되는 피드백 효과입니다.
    /// 작은 흔들림과 색상 변화를 제공합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectClickFeedbackEffect", menuName = "AFKS/Stage/Effects/Object Click Feedback")]
    public class ObjectClickFeedbackEffect : StageEffect
    {
        [Header("피드백 설정")]
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private float verticalShakeIntensity = 0.0125f; // 위아래 흔들림 (1/4 - 진짜 조금만)
        [SerializeField] private float horizontalShakeIntensity = 0.025f; // 좌우 흔들림 (절반)
        
        [Header("대상 오브젝트")]
        [SerializeField] private string targetObjectId = ""; // 비어있으면 클릭된 오브젝트에 적용
        
        public override void Apply(StageEventSystem system)
        {
            if (system == null) return;
            
            Debug.Log($"ObjectClickFeedbackEffect: 오브젝트 클릭 피드백 시작 - 흔들림: {shakeDuration}초");
            
            // 대상 오브젝트에 피드백 적용
            if (!string.IsNullOrEmpty(targetObjectId))
            {
                ApplyFeedbackToObject(targetObjectId, system);
            }
        }
        
        /// <summary>
        /// 체인 세트에 피드백을 적용합니다. 체인 하나를 클릭하면 두 체인 모두 반응합니다.
        /// </summary>
        /// <param name="clickedChainId">클릭된 체인 ID (ChainTop 또는 ChainBottom)</param>
        /// <param name="system">스테이지 이벤트 시스템</param>
        public void ApplyFeedbackToChainSet(string clickedChainId, StageEventSystem system)
        {
            Debug.Log($"ObjectClickFeedbackEffect: 체인 세트 피드백 - {clickedChainId} 클릭됨, 두 체인 모두 반응");
            
            // 두 체인 모두에 피드백 적용
            ApplyFeedbackToObject("ChainTop", system);
            ApplyFeedbackToObject("ChainBottom", system);
        }
        
        /// <summary>
        /// 특정 오브젝트에 피드백을 적용합니다.
        /// </summary>
        /// <param name="objectId">피드백을 적용할 오브젝트 ID</param>
        /// <param name="system">스테이지 이벤트 시스템</param>
        public void ApplyFeedbackToObject(string objectId, StageEventSystem system)
        {
            var targetObject = system.GetObject(objectId);
            if (targetObject == null) return;
            
            // 흔들림 효과만 시작 (색상 변화 제거)
            StartShakeEffect(targetObject);
        }
        
        private void StartShakeEffect(GameObject chainObject)
        {
            if (chainObject == null) return;
            
            // 코루틴으로 흔들림 효과 실행
            AFKS.Core.Utils.CoroutineRunner.Start(ShakeCoroutine(chainObject));
        }
        
        private System.Collections.IEnumerator ShakeCoroutine(GameObject chainObject)
        {
            if (chainObject == null) yield break;
            
            Vector3 originalPosition = chainObject.transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                // X(좌우)는 절반, Y(위아래)는 진짜 조금만 흔들림
                float x = originalPosition.x + Random.Range(-horizontalShakeIntensity, horizontalShakeIntensity);
                float y = originalPosition.y + Random.Range(-verticalShakeIntensity, verticalShakeIntensity);
                chainObject.transform.localPosition = new Vector3(x, y, originalPosition.z);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 원래 위치로 복원
            chainObject.transform.localPosition = originalPosition;
        }
        

    }
}
