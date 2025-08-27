using UnityEngine;
using AFKS.Core.Utils;

namespace AFKS.Features.Stage.Effects
{
    /// <summary>
    /// 특정 오브젝트의 상호작용을 비활성화하는 효과입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "New Disable Interactable Effect", menuName = "AFKS/Stage/Effects/Disable Interactable Effect")]
    public class DisableInteractableEffect : StageEffect
    {
        [Header("비활성화할 오브젝트")]
        [SerializeField] private string objectId = "";
        [SerializeField] private bool disableCollider = true;
        [SerializeField] private bool disableClickHandler = true;
        
        public override void Apply(StageEventSystem eventSystem)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                GameDebug.Warn(GameDebug.Category.Event, "DisableInteractableEffect: objectId가 설정되지 않았습니다.");
                return;
            }
            
            var obj = eventSystem.GetObject(objectId);
            if (obj == null)
            {
                GameDebug.Warn(GameDebug.Category.Event, $"DisableInteractableEffect: {objectId} 오브젝트를 찾을 수 없습니다.");
                return;
            }
            
            // Collider 비활성화
            if (disableCollider)
            {
                var collider = obj.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                    GameDebug.Info(GameDebug.Category.Event, $"DisableInteractableEffect: {objectId} Collider 비활성화됨");
                }
            }
            
            // ClickHandler 비활성화
            if (disableClickHandler)
            {
                var clickHandler = obj.GetComponent<AFKS.Features.Interaction.ClickHandler>();
                if (clickHandler != null)
                {
                    clickHandler.SetClickable(false);
                    GameDebug.Info(GameDebug.Category.Event, $"DisableInteractableEffect: {objectId} ClickHandler 비활성화됨");
                }
            }
            
            // StageInteractionManager에도 반영
            var interactionManager = AFKS.Features.Stage.StageInteractionManager.Instance;
            if (interactionManager != null)
            {
                interactionManager.SetObjectInteractable(objectId, false);
            }
            
            GameDebug.Info(GameDebug.Category.Event, $"DisableInteractableEffect: {objectId} 상호작용 비활성화 완료");
        }
    }
}
