using UnityEngine;
using System.Collections;

namespace AFKS.Features.Stage.Effects
{
    /// <summary>
    /// 체인이 5번 클릭되었을 때 상체와 하체가 분리되어 떨어지는 효과를 제공하는 컴포넌트입니다.
    /// </summary>
    public class ChainBreakEffect : MonoBehaviour
    {
        [Header("분리 설정")]
        [SerializeField] private float separationDistance = 0.5f;
        [SerializeField] private float fallDuration = 1.5f;
        [SerializeField] private float fallDistance = 2f;
        [SerializeField] private float bounceHeight = 0.3f;
        [SerializeField] private int bounceCount = 2;
        
        [Header("물리 설정")]
        [SerializeField] private float gravity = 15f; // Unity 2D에 적합한 중력 값
        [SerializeField] private float bounceDecay = 0.7f;
        
        private bool isBreaking = false;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        
        /// <summary>
        /// 현재 분리 애니메이션이 진행 중인지 확인합니다.
        /// </summary>
        public bool IsBreaking => isBreaking;
        
        private void Awake()
        {
            originalPosition = transform.localPosition;
            originalScale = transform.localScale;
        }
        
        /// <summary>
        /// 체인 분리 효과를 시작합니다.
        /// </summary>
        public void StartBreak()
        {
            if (isBreaking) return;
            
            StartCoroutine(BreakCoroutine());
        }
        
        private IEnumerator BreakCoroutine()
        {
            isBreaking = true;
            
            // 1단계: 체인 흔들림 (기존 ChainShakeEffect와 연동)
            var shakeEffect = GetComponent<ChainShakeEffect>();
            if (shakeEffect != null)
            {
                shakeEffect.StartShake(0.3f);
                yield return new WaitForSeconds(0.3f);
            }
            
            // 2단계: 체인 분리
            yield return StartCoroutine(SeparateChain());
            
            // 3단계: 분리된 체인들이 떨어짐
            yield return StartCoroutine(FallChainParts());
            
            isBreaking = false;
        }
        
        private IEnumerator SeparateChain()
        {
            float elapsed = 0f;
            float duration = 0.5f;
            
            // 실제 체인 파트들을 생성하고 원본에 자식으로 추가
            GameObject topPart = CreateChainPart("ChainTop_Part", Vector3.up * separationDistance);
            GameObject bottomPart = CreateChainPart("ChainBottom_Part", Vector3.down * separationDistance);
            
            // 원본 체인을 투명하게 만들어서 분리 효과 표현
            var originalRenderer = GetComponent<SpriteRenderer>();
            if (originalRenderer != null)
            {
                originalRenderer.color = new Color(1f, 1f, 1f, 0.3f); // 반투명하게
            }
            
            // 분리 애니메이션
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float easeProgress = 1f - Mathf.Pow(1f - progress, 3f); // Ease Out
                
                // 각 파트를 서로 다른 방향으로 분리
                Vector3 topOffset = Vector3.up * separationDistance * easeProgress;
                Vector3 bottomOffset = Vector3.down * separationDistance * easeProgress;
                
                topPart.transform.localPosition = originalPosition + topOffset;
                bottomPart.transform.localPosition = originalPosition + bottomOffset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 최종 분리 위치 설정
            topPart.transform.localPosition = originalPosition + Vector3.up * separationDistance;
            bottomPart.transform.localPosition = originalPosition + Vector3.down * separationDistance;
            
            Debug.Log("[ChainBreakEffect] 체인 분리 완료 - 상체와 하체가 실제로 분리됨");
        }
        
        private IEnumerator FallChainParts()
        {
            // 이미 분리된 체인 파트들을 찾기
            GameObject topPart = transform.Find("ChainTop_Part")?.gameObject;
            GameObject bottomPart = transform.Find("ChainBottom_Part")?.gameObject;
            
            if (topPart == null || bottomPart == null)
            {
                Debug.LogError("[ChainBreakEffect] 분리된 체인 파트를 찾을 수 없습니다!");
                yield break;
            }
            
            // 두 부분이 동시에 떨어짐
            var topFall = StartCoroutine(FallChainPart(topPart, topPart.transform.localPosition));
            var bottomFall = StartCoroutine(FallChainPart(bottomPart, bottomPart.transform.localPosition));
            
            // 원본 체인을 완전히 투명하게 만들어서 사라진 것처럼 보이게
            var originalRenderer = GetComponent<SpriteRenderer>();
            if (originalRenderer != null)
            {
                originalRenderer.color = new Color(1f, 1f, 1f, 0f); // 완전 투명
            }
            
            yield return topFall;
            yield return bottomFall;
            
            // 모든 효과 완료 후 원본 체인 제거
            Destroy(gameObject);
        }
        
        private GameObject CreateChainPart(string partName, Vector3 offset)
        {
            GameObject part = new GameObject(partName);
            part.transform.SetParent(transform); // 원본 체인의 자식으로 설정
            part.transform.localPosition = offset; // 상대 위치로 설정
            part.transform.localScale = originalScale * 0.8f; // 약간 작게
            
            // 스프라이트 렌더러 복사
            var originalRenderer = GetComponent<SpriteRenderer>();
            if (originalRenderer != null)
            {
                var partRenderer = part.AddComponent<SpriteRenderer>();
                partRenderer.sprite = originalRenderer.sprite;
                partRenderer.color = originalRenderer.color;
                partRenderer.sortingOrder = originalRenderer.sortingOrder + 1; // 원본보다 앞에 표시
            }
            
            // 콜라이더 추가 (클릭 이벤트 방지)
            var partCollider = part.AddComponent<BoxCollider2D>();
            var originalCollider = GetComponent<BoxCollider2D>();
            if (originalCollider != null)
            {
                partCollider.size = originalCollider.size * 0.8f;
                partCollider.offset = originalCollider.offset;
            }
            
            // 클릭 이벤트 방지를 위한 태그 설정
            part.tag = "Untagged";
            
            Debug.Log($"[ChainBreakEffect] 체인 파트 생성 완료: {partName}");
            return part;
        }
        
        private IEnumerator FallChainPart(GameObject chainPart, Vector3 startPos)
        {
            // GameObject가 유효한지 확인
            if (chainPart == null)
            {
                Debug.LogWarning("[ChainBreakEffect] 체인 파트가 null입니다!");
                yield break;
            }
            
            Vector3 endPos = startPos + Vector3.down * fallDistance;
            
            float elapsed = 0f;
            float duration = fallDuration;
            float initialVelocity = 0f; // 초기 속도
            
            while (elapsed < duration && chainPart != null)
            {
                float progress = elapsed / duration;
                
                // 중력에 의한 자연스러운 낙하 곡선 (물리 공식 사용)
                float time = elapsed;
                float yPos = startPos.y + initialVelocity * time - 0.5f * gravity * time * time;
                
                // 바닥에 닿지 않도록 제한
                yPos = Mathf.Max(yPos, endPos.y);
                
                // 바운스 효과
                float bounce = 0f;
                if (progress > 0.5f)
                {
                    float bounceProgress = (progress - 0.5f) * 2f; // 0.5~1.0을 0~1로 변환
                    bounce = Mathf.Sin(bounceProgress * Mathf.PI * bounceCount) * bounceHeight * bounceDecay;
                }
                
                Vector3 newPos = new Vector3(startPos.x, yPos + bounce, startPos.z);
                
                // GameObject가 여전히 유효한지 확인 후 위치 설정
                if (chainPart != null)
                {
                    chainPart.transform.localPosition = newPos;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 최종 위치 설정 (안전하게)
            if (chainPart != null)
            {
                chainPart.transform.localPosition = endPos;
                
                // 잠시 후 제거
                yield return new WaitForSeconds(2f);
                Destroy(chainPart);
            }
        }
        
        private void OnDisable()
        {
            // 비활성화 시 원래 상태로 복원
            if (isBreaking)
            {
                transform.localPosition = originalPosition;
                transform.localScale = originalScale;
                isBreaking = false;
            }
        }
    }
}
