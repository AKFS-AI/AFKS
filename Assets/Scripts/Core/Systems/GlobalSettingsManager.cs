using UnityEngine;
using UnityEngine.UI;
using AFKS.Core.Services;
using AFKS.Core.Services.Scene;
using AFKS.Core.Services.Save;
using AFKS.Core.Events;

namespace AFKS.Core.Systems
{
    /// <summary>
    /// 전역 설정을 관리합니다. 모든 씬에서 ESC 키로 설정창을 열고 닫을 수 있습니다.
    /// </summary>
    [AddComponentMenu("AFKS/Core/Global Settings Manager")]
    public sealed class GlobalSettingsManager : MonoBehaviour
    {
        [Header("설정창 UI")]
        [SerializeField]
        [InspectorName("설정 패널")]
        private GameObject settingsPanel;
        
        [SerializeField]
        [InspectorName("설정창 닫기 버튼")]
        private Button closeSettingsButton;
        
        [SerializeField]
        [InspectorName("메뉴로 돌아가기 버튼")]
        private Button returnToMenuButton;
        
        [SerializeField]
        [InspectorName("세이브 초기화 버튼")]
        private Button resetSaveButton;

        private void Awake()
        {
            Debug.Log("GlobalSettingsManager 초기화");
            
            // 설정 패널 초기 상태
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            
            // 버튼 이벤트 연결
            if (closeSettingsButton != null)
            {
                closeSettingsButton.onClick.AddListener(CloseSettings);
            }
            
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.AddListener(ReturnToMenu);
            }
            
            if (resetSaveButton != null)
            {
                resetSaveButton.onClick.AddListener(ResetSave);
            }
        }

        private void Update()
        {
            // ESC 키로 설정창 열기/닫기 (모든 씬에서 작동)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("GlobalSettingsManager: ESC 키가 눌렸습니다!");
                
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    Debug.Log("설정창을 닫습니다.");
                    CloseSettings();
                }
                else
                {
                    Debug.Log("설정창을 엽니다.");
                    OpenSettings();
                }
            }
        }

        #region 공개 API
        
        /// <summary>
        /// 설정창을 엽니다.
        /// </summary>
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                Debug.Log("전역 설정창이 열렸습니다");
            }
        }

        /// <summary>
        /// 설정창을 닫습니다.
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                Debug.Log("전역 설정창이 닫혔습니다");
            }
        }

        /// <summary>
        /// 메뉴로 돌아갑니다.
        /// </summary>
        public void ReturnToMenu()
        {
            Debug.Log("메뉴로 돌아가기 요청됨");
            CloseSettings();
            
            // GameEvents를 통해 메뉴로 이동 요청
            GameEvents.RaiseStageChangeRequested("Menu");
        }

        /// <summary>
        /// 세이브를 초기화합니다.
        /// </summary>
        public void ResetSave()
        {
            Debug.Log("세이브 초기화 요청됨");
            
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
                if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var saveService))
                {
                    saveService.DeleteAll();
                    Debug.Log("모든 저장 데이터가 삭제되었습니다.");
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
        
        #endregion
    }
}
