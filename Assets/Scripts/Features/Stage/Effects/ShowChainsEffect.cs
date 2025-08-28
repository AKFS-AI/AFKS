using UnityEngine;
using AFKS.Core.Utils;

namespace AFKS.Features.Stage.Effects
{
    /// <summary>
    /// 체인을 표시하고 활성화하는 효과입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "New Show Chains Effect", menuName = "AFKS/Stage/Effects/Show Chains Effect")]
    public class ShowChainsEffect : StageEffect
    {
        [Header("체인 설정")]
        [SerializeField] private string[] chainObjectIds = { "ChainTop", "ChainBottom" };
        [SerializeField] private bool showChains = true;
        [SerializeField] private bool enableClickable = true;
        
        public override void Apply(StageEventSystem eventSystem)
        {
            if (chainObjectIds == null || chainObjectIds.Length == 0)
            {
                Debug.LogWarning("ShowChainsEffect: chainObjectIds가 설정되지 않았습니다.");
                return;
            }
            
            foreach (var chainId in chainObjectIds)
            {
                if (string.IsNullOrEmpty(chainId)) continue;
                
                var chainObj = eventSystem.GetObject(chainId);
                if (chainObj == null)
                {
                    Debug.LogWarning($"ShowChainsEffect: {chainId} 오브젝트를 찾을 수 없습니다.");
                    return;
                }
                
                // 체인 표시
                if (showChains)
                {
                    chainObj.SetActive(true);
                }
                
                // 클릭 가능하게 설정
                if (enableClickable)
                {
                    var clickHandler = chainObj.GetComponent<AFKS.Features.Interaction.ClickHandler>();
                    if (clickHandler != null)
                    {
                        clickHandler.enabled = true;
                        clickHandler.SetClickable(true);
                    }
                    else
                    {
                        Debug.LogWarning($"ShowChainsEffect: {chainId}에 ClickHandler가 없습니다.");
                    }
                    
                    // StageInteractionSystem에도 반영
                    var interactionManager = AFKS.Features.Stage.StageInteractionSystem.Instance;
                    if (interactionManager != null)
                    {
                        interactionManager.SetObjectInteractable(chainId, true);
                    }
                    else
                    {
                        Debug.LogWarning("ShowChainsEffect: StageInteractionSystem를 찾을 수 없습니다.");
                    }
                }
            }
        }
    }
}
