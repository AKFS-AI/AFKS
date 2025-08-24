using UnityEngine;
using UnityEngine.UI;
using AFKS.Core.Events;
using AFKS.Core.Services;
using AFKS.Core.Services.Save;
using AFKS.Features.Stage;

namespace AFKS.Features.Menu
{
    /// <summary>
    /// 메인 메뉴 씬에서 사용합니다. 인스펙터에서 버튼들을 연결하십시오.
    /// </summary>
    [AddComponentMenu("AFKS/Menu/Main Menu UI")]
    public sealed class MainMenuUI : MonoBehaviour
    {
        #region 필드
        [SerializeField]
        [InspectorName("새 게임 버튼")]
        private Button newGameButton;

        [SerializeField]
        [InspectorName("계속하기 버튼")]
        private Button continueButton;

        [SerializeField]
        [InspectorName("종료 버튼")]
        private Button quitButton;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            if (newGameButton != null) newGameButton.onClick.AddListener(OnClickNewGame);
            if (continueButton != null) continueButton.onClick.AddListener(OnClickContinue);
            if (quitButton != null) quitButton.onClick.AddListener(OnClickQuit);

            // 저장 유무에 따라 '계속하기' 표시/활성 제어
            TrySetupContinueVisibility();
        }

        private void OnDestroy()
        {
            if (newGameButton != null) newGameButton.onClick.RemoveListener(OnClickNewGame);
            if (continueButton != null) continueButton.onClick.RemoveListener(OnClickContinue);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnClickQuit);
        }
        #endregion

        #region 이벤트 핸들러
        private void OnClickNewGame()
        {
            // MVP 단계에서는 바로 Stage_Front로 이동
            GameEvents.RaiseStageChangeRequested(StageIds.Front);
        }

        private void OnClickContinue()
        {
            // SaveService가 있다면 로드 후 저장된 진행으로 이동하도록 확장 여지
            GameEvents.RaiseStageChangeRequested(StageIds.Front);
        }

        private void OnClickQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region 내부 메서드
        private void TrySetupContinueVisibility()
        {
            if (continueButton == null) return;

            // 저장 서비스가 등록되어 있으면 이를 사용, 없으면 보수적으로 숨김
            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                bool hasSave = false;
                try { hasSave = save.HasAnySave(); }
                catch { hasSave = false; }

                continueButton.gameObject.SetActive(hasSave);
                continueButton.interactable = hasSave;
            }
            else
            {
                // 저장 시스템 미구현: 계속하기 숨김
                continueButton.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}


