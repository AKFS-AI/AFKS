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
        [InspectorName("설정 버튼")]
        private Button settingsButton;

        [SerializeField]
        [InspectorName("종료 버튼")]
        private Button quitButton;

        [SerializeField]
        [InspectorName("설정 패널")]
        private GameObject settingsPanel;

        [SerializeField]
        [InspectorName("설정 패널 배경")]
        private Image settingsPanelBackground;
        #endregion

        #region 유니티 수명주기
        private void Awake()
        {
            Debug.Log("MainMenuUI Awake 시작");
            
            if (newGameButton != null) 
            {
                newGameButton.onClick.AddListener(OnClickNewGame);
                Debug.Log("새 게임 버튼 이벤트 연결됨");
            }
            else Debug.LogWarning("새 게임 버튼이 null입니다!");
            
            if (continueButton != null) 
            {
                continueButton.onClick.AddListener(OnClickContinue);
                Debug.Log("계속하기 버튼 이벤트 연결됨");
            }
            else Debug.LogWarning("계속하기 버튼이 null입니다!");
            
            if (settingsButton != null) 
            {
                settingsButton.onClick.AddListener(OnClickSettings);
                Debug.Log("설정 버튼 이벤트 연결됨");
            }
            else Debug.LogWarning("설정 버튼이 null입니다!");
            
            if (quitButton != null) 
            {
                quitButton.onClick.AddListener(OnClickQuit);
                Debug.Log("종료 버튼 이벤트 연결됨");
            }
            else Debug.LogWarning("종료 버튼이 null입니다!");

            // 설정 패널 초기 상태 설정
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                Debug.Log("설정 패널이 비활성화됨");
            }
            else Debug.LogWarning("설정 패널이 null입니다!");

            // 저장 유무에 따라 '계속하기' 표시/활성 제어
            TrySetupContinueVisibility();
            
            Debug.Log("MainMenuUI Awake 완료");
        }

        private void Update()
        {
            // ESC 키는 이제 Core의 GlobalSettingsManager에서 처리됩니다.
            // 디버그: 다른 키 입력도 확인
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("스페이스바가 눌렸습니다 - 입력 시스템 작동 확인");
            }
        }

        private void OnDestroy()
        {
            if (newGameButton != null) newGameButton.onClick.RemoveListener(OnClickNewGame);
            if (continueButton != null) continueButton.onClick.RemoveListener(OnClickContinue);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnClickSettings);
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

        private void OnClickSettings()
        {
            // 전역 설정창 열기
            if (ServiceLocator.TryGet<AFKS.Core.Systems.GlobalSettingsManager>(out var settingsManager))
            {
                settingsManager.OpenSettings();
            }
            else
            {
                // GlobalSettingsManager가 없으면 Core 씬으로 이동하여 설정창 열기
                Debug.Log("GlobalSettingsManager를 찾을 수 없습니다. Core 씬으로 이동합니다.");
                
                // 사용자에게 안내 메시지 표시
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("설정", "설정창을 열기 위해 Core 씬으로 이동합니다.", "확인");
#else
                // 런타임에서는 간단한 로그로 대체
                Debug.Log("설정창을 열기 위해 Core 씬으로 이동합니다.");
#endif
                
                GameEvents.RaiseStageChangeRequested("Core");
            }
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

        #region 설정 관련
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void OnClickBackgroundMusic()
        {
            // 배경음 설정 (AudioService 연동 예정)
            Debug.Log("배경음 설정");
        }

        public void OnClickEnvironmentSound()
        {
            // 환경음 설정 (AudioService 연동 예정)
            Debug.Log("환경음 설정");
        }

        public void OnClickReturnToCurrent()
        {
            // 현재 화면으로 돌아가기 (설정창 닫기와 동일)
            CloseSettings();
        }

        public void OnClickReturnToMenu()
        {
            // 메뉴로 돌아가기 (설정창 닫기와 동일)
            CloseSettings();
        }

        public void OnClickResetSave()
        {
            // 세이브 초기화 확인 다이얼로그
            bool shouldReset = false;
            
#if UNITY_EDITOR
            shouldReset = UnityEditor.EditorUtility.DisplayDialog("세이브 초기화", 
                "모든 저장 데이터를 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.", 
                "삭제", "취소");
#else
            // 런타임에서는 간단한 로그로 대체 (실제로는 UI 다이얼로그 구현 필요)
            Debug.Log("세이브 초기화 요청됨 - 런타임에서는 UI 다이얼로그 구현 필요");
            shouldReset = true; // 임시로 true로 설정
#endif

            if (shouldReset)
            {
                if (ServiceLocator.TryGet<ISaveService>(out var saveService))
                {
                    saveService.DeleteAll();
                    Debug.Log("모든 저장 데이터가 삭제되었습니다.");
                    
                    // 계속하기 버튼 상태 업데이트
                    TrySetupContinueVisibility();
                }
                else
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    Debug.Log("PlayerPrefs가 초기화되었습니다.");
                }
                
                CloseSettings();
            }
        }

        public void OnClickCloseSettings()
        {
            CloseSettings();
        }
        #endregion

        #region 내부 메서드
        private void TrySetupContinueVisibility()
        {
            Debug.Log("TrySetupContinueVisibility 시작");
            
            if (continueButton == null) 
            {
                Debug.LogWarning("continueButton이 null입니다!");
                return;
            }

            Debug.Log("저장 서비스 확인 중...");
            
            // 저장 서비스가 등록되어 있으면 이를 사용, 없으면 보수적으로 숨김
            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                Debug.Log("저장 서비스 발견됨");
                bool hasSave = false;
                try 
                { 
                    hasSave = save.HasAnySave(); 
                    Debug.Log($"저장 데이터 존재 여부: {hasSave}");
                }
                catch (System.Exception e) 
                { 
                    hasSave = false; 
                    Debug.LogError($"저장 데이터 확인 중 오류: {e.Message}");
                }

                continueButton.gameObject.SetActive(hasSave);
                continueButton.interactable = hasSave;
                Debug.Log($"계속하기 버튼 상태: Active={hasSave}, Interactable={hasSave}");
            }
            else
            {
                Debug.Log("저장 서비스를 찾을 수 없음 - 계속하기 버튼 비활성화");
                // 저장 시스템 미구현: 계속하기 숨김
                continueButton.gameObject.SetActive(false);
            }
            
            Debug.Log("TrySetupContinueVisibility 완료");
        }
        #endregion
    }
}


