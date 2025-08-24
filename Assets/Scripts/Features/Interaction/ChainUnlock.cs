using UnityEngine;
using UnityEngine.Events;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;

namespace AFKS.Features.Interaction
{
    /// <summary>
    /// 클릭 누적 횟수가 임계값에 도달하면 해제(OnUnlocked)하고 타깃 오브젝트를 활성화합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Interaction/Chain Unlock")]
    public sealed class ChainUnlock : MonoBehaviour
    {
        #region 필드
        [SerializeField]
        [InspectorName("필요 클릭 횟수")]
        [Tooltip("해제되기까지 필요한 클릭 횟수")] 
        private int requiredClicks = 3;

        [SerializeField]
        [InspectorName("해제 시 활성화할 타깃(옵션)")]
        [Tooltip("Unlocked 시 활성화할 오브젝트(예: DoorMove)")]
        private GameObject targetToEnable;

        [SerializeField]
        [InspectorName("해제 시 이벤트")]
        private UnityEvent OnUnlocked;

        private IInputService inputService;
        private int clickCount;
        private bool unlocked;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            ServiceLocator.TryGet<IInputService>(out inputService);
        }

        private void OnEnable()
        {
            if (inputService != null)
            {
                inputService.ObjectClicked += OnObjectClicked;
            }
        }

        private void OnDisable()
        {
            if (inputService != null)
            {
                inputService.ObjectClicked -= OnObjectClicked;
            }
        }
        #endregion

        #region 이벤트 핸들러
        private void OnObjectClicked(GameObject clicked)
        {
            if (unlocked) return;
            if (clicked != gameObject) return;

            clickCount++;
            if (clickCount >= Mathf.Max(1, requiredClicks))
            {
                Unlock();
            }
        }
        #endregion

        #region 내부 메서드
        private void Unlock()
        {
            if (unlocked) return;
            unlocked = true;

            if (targetToEnable != null)
            {
                targetToEnable.SetActive(true);
            }

            OnUnlocked?.Invoke();
        }
        #endregion
    }
}


