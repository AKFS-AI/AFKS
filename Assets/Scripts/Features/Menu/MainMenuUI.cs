using UnityEngine;
using UnityEngine.UI;
using AFKS.Core.Events;
using AFKS.Features.Stage;

namespace AFKS.Features.Menu
{
    /// <summary>
    /// 메인 메뉴 씬에서 사용합니다. 인스펙터에서 버튼들을 연결하십시오.
    /// </summary>
    [AddComponentMenu("AFKS/메뉴/메인 메뉴 UI")]
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField]
        [InspectorName("새 게임 버튼")]
        private Button newGameButton;

        [SerializeField]
        [InspectorName("계속하기 버튼")]
        private Button continueButton;

        [SerializeField]
        [InspectorName("종료 버튼")]
        private Button quitButton;

        private void Awake()
        {
            if (newGameButton != null) newGameButton.onClick.AddListener(OnClickNewGame);
            if (continueButton != null) continueButton.onClick.AddListener(OnClickContinue);
            if (quitButton != null) quitButton.onClick.AddListener(OnClickQuit);
        }

        private void OnDestroy()
        {
            if (newGameButton != null) newGameButton.onClick.RemoveListener(OnClickNewGame);
            if (continueButton != null) continueButton.onClick.RemoveListener(OnClickContinue);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnClickQuit);
        }

        private void OnClickNewGame()
        {
            // MVP 단계에서는 바로 Stage_Front로 이동
            GameEvents.RaiseStageChangeRequested(StageIds.Front);
        }

        private void OnClickContinue()
        {
            // SaveService 구현 전까지 New와 동일하게 처리
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
    }
}


