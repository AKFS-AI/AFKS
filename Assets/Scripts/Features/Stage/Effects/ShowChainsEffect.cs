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
            GameDebug.Info(GameDebug.Category.Event, $"ShowChainsEffect: 실행 시작 - chainObjectIds: {string.Join(", ", chainObjectIds ?? new string[0])}");
            
            if (chainObjectIds == null || chainObjectIds.Length == 0)
            {
                GameDebug.Warn(GameDebug.Category.Event, "ShowChainsEffect: chainObjectIds가 설정되지 않았습니다.");
                return;
            }
            
            foreach (var chainId in chainObjectIds)
            {
                if (string.IsNullOrEmpty(chainId)) continue;
                
                GameDebug.Info(GameDebug.Category.Event, $"ShowChainsEffect: {chainId} 처리 시작");
                
                var chainObj = eventSystem.GetObject(chainId);
                if (chainObj == null)
                {
                    GameDebug.Warn(GameDebug.Category.Event, $"ShowChainsEffect: {chainId} 오브젝트를 찾을 수 없습니다.");
                    return;
                }
                
                GameDebug.Info(GameDebug.Category.Event, $"ShowChainsEffect: {chainId} 오브젝트 발견 - {chainObj.name}");
                
                // 체인 표시
                if (showChains)
                {
                    chainObj.SetActive(true);
                    GameDebug.Info(GameDebug.Category.Event, $"ShowChainsEffect: {chainId} 활성화됨");
                }
                
                // 클릭 가능하게 설정
                if (enableClickable)
                {
                    var clickHandler = chainObj.GetComponent<AFKS.Features.Interaction.ClickHandler>();
                    if (clickHandler != null)
                    {
                        clickHandler.enabled = true;
                        clickHandler.SetClickable(true);
                        GameDebug.Info(GameDebug.Category.Event, $"ShowChainsEffect: {chainId} ClickHandler 활성화됨");
                    }
                    else
                    {
                        GameDebug.Warn(GameDebug.Category.Event, $"ShowChainsEffect: {chainId}에 ClickHandler가 없습니다.");
                    }
                    
                    // StageInteractionManager에도 반영
                    var interactionManager = AFKS.Features.Stage.StageInteractionManager.Instance;
                    if (interactionManager != null)
                    {
                        interactionManager.SetObjectInteractable(chainId, true);
                        GameDebug.Info(GameDebug.Category.Event, $"ShowChainsEffect: {chainId} StageInteractionManager에 반영됨");
                    }
                    else
                    {
                        GameDebug.Warn(GameDebug.Category.Event, "ShowChainsEffect: StageInteractionManager를 찾을 수 없습니다.");
                    }
                }
            }
            
            GameDebug.Info(GameDebug.Category.Event, $"ShowChainsEffect: {chainObjectIds.Length}개 체인 표시 및 활성화 완료");
        }
    }
}
