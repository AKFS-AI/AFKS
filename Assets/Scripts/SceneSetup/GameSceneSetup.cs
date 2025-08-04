using UnityEngine;
using UnityEngine.UI;
using AFKS.Core;
using AFKS.StageSystem;
using AFKS.InteractionSystem;
using AFKS.InventorySystem;
using AFKS.HorrorSystem;
using AFKS.AudioSystem;

namespace AFKS.SceneSetup
{
    /// <summary>
    /// Game 씬 자동 설정 도구
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        [Header("🎯 게임 매니저 프리팹들")]
        [SerializeField, Tooltip("StageManager 프리팹")] private GameObject stageManagerPrefab;
        [SerializeField, Tooltip("InteractionManager 프리팹")] private GameObject interactionManagerPrefab;
        [SerializeField, Tooltip("InventoryManager 프리팹")] private GameObject inventoryManagerPrefab;
        [SerializeField, Tooltip("HorrorEventManager 프리팹")] private GameObject horrorEventManagerPrefab;

        [Header("🎨 스테이지 데이터")]
        [SerializeField, Tooltip("6개 스테이지 데이터")] private StageData[] stageDataArray = new StageData[6];

        [Header("🔊 오디오 설정")]
        [SerializeField, Tooltip("게임 BGM")] private AudioClip gameBGM;
        [SerializeField, Tooltip("앰비언트 사운드")] private AudioClip ambientSound;

        [Header("🎒 인벤토리 설정")]
        [SerializeField, Tooltip("인벤토리 슬롯 프리팹")] private GameObject inventorySlotPrefab;
        [SerializeField, Tooltip("인벤토리 그리드")] private Transform inventoryGrid;

        private void Start()
        {
            SetupGameManagers();
            SetupStageSystem();
            SetupInventoryUI();
            SetupAudioSystem();
            StartGame();
        }

        /// <summary>
        /// 게임 매니저들 자동 생성
        /// </summary>
        private void SetupGameManagers()
        {
            // SystemManagers 부모 오브젝트 생성
            GameObject systemManagers = new GameObject("SystemManagers");
            
            // 각 매니저 생성
            if (stageManagerPrefab != null)
            {
                GameObject stageManager = Instantiate(stageManagerPrefab, systemManagers.transform);
                stageManager.name = "StageManager";
            }
            
            if (interactionManagerPrefab != null)
            {
                GameObject interactionManager = Instantiate(interactionManagerPrefab, systemManagers.transform);
                interactionManager.name = "InteractionManager";
            }
            
            if (inventoryManagerPrefab != null)
            {
                GameObject inventoryManager = Instantiate(inventoryManagerPrefab, systemManagers.transform);
                inventoryManager.name = "InventoryManager";
            }
            
            if (horrorEventManagerPrefab != null)
            {
                GameObject horrorEventManager = Instantiate(horrorEventManagerPrefab, systemManagers.transform);
                horrorEventManager.name = "HorrorEventManager";
            }

            Debug.Log("[GameSceneSetup] 게임 매니저들이 생성되었습니다.");
        }

        /// <summary>
        /// 스테이지 시스템 초기화
        /// </summary>
        private void SetupStageSystem()
        {
            if (StageManager.Instance != null)
            {
                // 스테이지 데이터 로드
                for (int i = 0; i < stageDataArray.Length; i++)
                {
                    if (stageDataArray[i] != null)
                    {
                        // StageManager에 스테이지 데이터 등록
                        // StageManager.Instance.RegisterStage(i, stageDataArray[i]);
                    }
                }

                // 첫 번째 스테이지로 시작
                // StageManager.Instance.LoadStage(0);
            }

            Debug.Log("[GameSceneSetup] 스테이지 시스템이 초기화되었습니다.");
        }

        /// <summary>
        /// 인벤토리 UI 자동 생성
        /// </summary>
        private void SetupInventoryUI()
        {
            if (inventorySlotPrefab != null && inventoryGrid != null)
            {
                // 10개의 인벤토리 슬롯 생성
                for (int i = 0; i < 10; i++)
                {
                    GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);
                    slot.name = $"InventorySlot_{i:00}";
                }
            }

            Debug.Log("[GameSceneSetup] 인벤토리 UI가 생성되었습니다.");
        }

        /// <summary>
        /// 오디오 시스템 초기화
        /// </summary>
        private void SetupAudioSystem()
        {
            if (AudioManager.Instance != null)
            {
                // BGM 재생
                if (gameBGM != null)
                {
                    AudioManager.Instance.PlayBGM(gameBGM);
                }

                // 앰비언트 사운드 재생
                if (ambientSound != null)
                {
                    AudioManager.Instance.PlayAmbient(ambientSound);
                }
            }

            Debug.Log("[GameSceneSetup] 오디오 시스템이 초기화되었습니다.");
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        private void StartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }

            Debug.Log("[GameSceneSetup] 게임이 시작되었습니다!");
        }

        /// <summary>
        /// 에디터에서 스테이지 데이터 자동 할당
        /// </summary>
        [ContextMenu("스테이지 데이터 자동 찾기")]
        private void AutoAssignStageData()
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:StageData");
            
            for (int i = 0; i < guids.Length && i < stageDataArray.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                stageDataArray[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<StageData>(path);
            }
            
            Debug.Log($"[GameSceneSetup] {guids.Length}개의 스테이지 데이터를 찾았습니다.");
#endif
        }
    }
}