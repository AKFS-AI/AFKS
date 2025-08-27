using UnityEngine;
using AFKS.Core.Utils;
using AFKS.Core.Services;
using AFKS.Core.Services.StageBackground;

namespace AFKS.Features.Stage.Effects
{
    /// <summary>
    /// 현재 스테이지의 배경을 StageBackgroundService에 보고하는 효과입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "New Report Background Effect", menuName = "AFKS/Stage/Effects/Report Background Effect")]
    public class ReportBackgroundEffect : StageEffect
    {
        [Header("배경 설정")]
        [SerializeField] private string stageId = "";
        [SerializeField] private string backgroundObjectId = "";
        [SerializeField] private Sprite fallbackBackground;
        
        public override void Apply(StageEventSystem eventSystem)
        {
            if (string.IsNullOrEmpty(stageId))
            {
                stageId = eventSystem.StageId;
            }
            
            Sprite backgroundSprite = null;
            
            // 지정된 오브젝트에서 배경 찾기
            if (!string.IsNullOrEmpty(backgroundObjectId))
            {
                var bgObj = eventSystem.GetObject(backgroundObjectId);
                if (bgObj != null)
                {
                    var spriteRenderer = bgObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        backgroundSprite = spriteRenderer.sprite;
                    }
                }
            }
            
            // 배경을 찾지 못한 경우 fallback 사용
            if (backgroundSprite == null)
            {
                backgroundSprite = fallbackBackground;
            }
            
            // StageBackgroundService에 배경 보고
            if (ServiceLocator.TryGet<IStageBackgroundService>(out var bgService))
            {
                bgService.ReportBackground(stageId, backgroundSprite);
                GameDebug.Info(GameDebug.Category.Event, $"ReportBackgroundEffect: {stageId} 배경 보고 완료");
            }
            else
            {
                GameDebug.Warn(GameDebug.Category.Event, "ReportBackgroundEffect: IStageBackgroundService를 찾을 수 없습니다.");
            }
        }
    }
}
