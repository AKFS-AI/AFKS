using UnityEngine;
using System.Collections.Generic;
using AFKS.Core.Services;

namespace AFKS.Features.Stage
{
    /// <summary>
    /// 스토리 진행 상태와 완료를 관리하는 컴포넌트입니다.
    /// 각 스테이지의 완료 상태를 추적하고 저장합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Stage/Story Progress")]
    public class StoryProgress : MonoBehaviour
    {
        [Header("스토리 진행")]
        [SerializeField] private List<string> completedStages = new List<string>();
        [SerializeField] private string currentStage = "";
        [SerializeField] private int totalStages = 6;
        
        [Header("아이템 수집")]
        [SerializeField] private List<string> collectedItems = new List<string>();
        
        [Header("저장 설정")]
        [SerializeField] private const string SAVE_KEY_COMPLETED_STAGES = "Story.CompletedStages";
        [SerializeField] private const string SAVE_KEY_CURRENT_STAGE = "Story.CurrentStage";
        [SerializeField] private const string SAVE_KEY_COLLECTED_ITEMS = "Story.CollectedItems";
        
        #region Unity 수명주기
        
        private void Awake()
        {
            LoadProgress();
        }
        
        #endregion
        
        #region 공개 API
        
        /// <summary>
        /// 특정 스테이지가 완료되었음을 기록합니다.
        /// </summary>
        /// <param name="stageId">완료된 스테이지 ID</param>
        public void OnStageComplete(string stageId)
        {
            if (!completedStages.Contains(stageId))
            {
                completedStages.Add(stageId);
                SaveProgress();
                
                Debug.Log($"StoryProgress: {stageId} 완료! 총 {completedStages.Count}/{totalStages} 스테이지 완료");
                
                // 모든 스테이지 완료 체크
                if (completedStages.Count >= totalStages)
                {
                    OnAllStagesComplete();
                }
            }
        }
        
        /// <summary>
        /// 현재 스테이지를 설정합니다.
        /// </summary>
        /// <param name="stageId">현재 스테이지 ID</param>
        public void SetCurrentStage(string stageId)
        {
            currentStage = stageId;
            SaveProgress();
            Debug.Log($"StoryProgress: 현재 스테이지 설정 - {stageId}");
        }
        
        /// <summary>
        /// 특정 스테이지가 완료되었는지 확인합니다.
        /// </summary>
        /// <param name="stageId">확인할 스테이지 ID</param>
        /// <returns>완료 여부</returns>
        public bool IsStageCompleted(string stageId)
        {
            return completedStages.Contains(stageId);
        }
        
        /// <summary>
        /// 모든 스테이지가 완료되었는지 확인합니다.
        /// </summary>
        /// <returns>모든 스테이지 완료 여부</returns>
        public bool AreAllStagesCompleted()
        {
            return completedStages.Count >= totalStages;
        }
        
        /// <summary>
        /// 스토리 진행률을 반환합니다 (0.0 ~ 1.0).
        /// </summary>
        /// <returns>진행률</returns>
        public float GetProgress()
        {
            return totalStages > 0 ? (float)completedStages.Count / totalStages : 0f;
        }
        
        /// <summary>
        /// 완료된 스테이지 목록을 반환합니다.
        /// </summary>
        /// <returns>완료된 스테이지 ID 목록</returns>
        public List<string> GetCompletedStages()
        {
            return new List<string>(completedStages);
        }
        
        /// <summary>
        /// 아이템을 수집합니다.
        /// </summary>
        /// <param name="itemId">수집할 아이템 ID</param>
        public void CollectItem(string itemId)
        {
            if (!collectedItems.Contains(itemId))
            {
                collectedItems.Add(itemId);
                SaveProgress();
                Debug.Log($"StoryProgress: 아이템 수집됨 - {itemId}");
            }
        }
        
        /// <summary>
        /// 특정 아이템이 수집되었는지 확인합니다.
        /// </summary>
        /// <param name="itemId">확인할 아이템 ID</param>
        /// <returns>수집 여부</returns>
        public bool IsItemCollected(string itemId)
        {
            return collectedItems.Contains(itemId);
        }
        
        /// <summary>
        /// 수집된 아이템 목록을 반환합니다.
        /// </summary>
        /// <returns>수집된 아이템 ID 목록</returns>
        public List<string> GetCollectedItems()
        {
            return new List<string>(collectedItems);
        }
        
        /// <summary>
        /// 현재 스테이지 ID를 반환합니다.
        /// </summary>
        /// <returns>현재 스테이지 ID</returns>
        public string GetCurrentStage()
        {
            return currentStage;
        }
        
        #endregion
        
        #region 내부 메서드
        
        private void OnAllStagesComplete()
        {
            Debug.Log("🎉 축하합니다! 모든 스테이지를 완료했습니다!");
            
            // 게임 완료 이벤트 발생
            // 여기에 크레딧, 엔딩 씬 전환 등의 로직 추가
        }
        
        #endregion
        
        #region 저장/불러오기
        
        private void SaveProgress()
        {
            // SaveService 사용 시도
            if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var saveService))
            {
                // 완료된 스테이지 목록을 JSON으로 직렬화
                var completedStagesJson = JsonUtility.ToJson(new StageList { stages = completedStages });
                var collectedItemsJson = JsonUtility.ToJson(new ItemList { items = collectedItems });
                saveService.SetString(SAVE_KEY_COMPLETED_STAGES, completedStagesJson);
                saveService.SetString(SAVE_KEY_CURRENT_STAGE, currentStage);
                saveService.SetString(SAVE_KEY_COLLECTED_ITEMS, collectedItemsJson);
            }
            else
            {
                // PlayerPrefs 사용
                var completedStagesJson = JsonUtility.ToJson(new StageList { stages = completedStages });
                var collectedItemsJson = JsonUtility.ToJson(new ItemList { items = collectedItems });
                PlayerPrefs.SetString(SAVE_KEY_COMPLETED_STAGES, completedStagesJson);
                PlayerPrefs.SetString(SAVE_KEY_CURRENT_STAGE, currentStage);
                PlayerPrefs.SetString(SAVE_KEY_COLLECTED_ITEMS, collectedItemsJson);
                PlayerPrefs.Save();
            }
        }
        
        private void LoadProgress()
        {
            // SaveService 사용 시도
            if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var saveService))
            {
                var completedStagesJson = saveService.GetString(SAVE_KEY_COMPLETED_STAGES, "");
                var currentStageSaved = saveService.GetString(SAVE_KEY_CURRENT_STAGE, "");
                var collectedItemsJson = saveService.GetString(SAVE_KEY_COLLECTED_ITEMS, "");
                
                if (!string.IsNullOrEmpty(completedStagesJson))
                {
                    var stageList = JsonUtility.FromJson<StageList>(completedStagesJson);
                    completedStages = stageList.stages;
                }
                
                if (!string.IsNullOrEmpty(currentStageSaved))
                {
                    currentStage = currentStageSaved;
                }
                
                if (!string.IsNullOrEmpty(collectedItemsJson))
                {
                    var itemList = JsonUtility.FromJson<ItemList>(collectedItemsJson);
                    collectedItems = itemList.items;
                }
            }
            else
            {
                // PlayerPrefs 사용
                var completedStagesJson = PlayerPrefs.GetString(SAVE_KEY_COMPLETED_STAGES, "");
                var currentStageSaved = PlayerPrefs.GetString(SAVE_KEY_CURRENT_STAGE, "");
                var collectedItemsJson = PlayerPrefs.GetString(SAVE_KEY_COLLECTED_ITEMS, "");
                
                if (!string.IsNullOrEmpty(completedStagesJson))
                {
                    var stageList = JsonUtility.FromJson<StageList>(completedStagesJson);
                    completedStages = stageList.stages;
                }
                
                if (!string.IsNullOrEmpty(currentStageSaved))
                {
                    currentStage = currentStageSaved;
                }
                
                if (!string.IsNullOrEmpty(collectedItemsJson))
                {
                    var itemList = JsonUtility.FromJson<ItemList>(collectedItemsJson);
                    collectedItems = itemList.items;
                }
            }
            
            Debug.Log($"StoryProgress: 진행 상태 불러오기 완료 - 완료된 스테이지: {completedStages.Count}, 현재 스테이지: {currentStage}, 수집된 아이템: {collectedItems.Count}");
        }
        
        #endregion
        
        #region 디버그
        
        [ContextMenu("진행 상태 리셋")]
        private void ResetProgress()
        {
            completedStages.Clear();
            currentStage = "";
            collectedItems.Clear();
            SaveProgress();
            Debug.Log("StoryProgress: 진행 상태 리셋 완료");
        }
        
        [ContextMenu("모든 스테이지 완료")]
        private void CompleteAllStages()
        {
            for (int i = 1; i <= totalStages; i++)
            {
                var stageId = $"Stage{i}";
                if (!completedStages.Contains(stageId))
                {
                    completedStages.Add(stageId);
                }
            }
            SaveProgress();
            Debug.Log("StoryProgress: 모든 스테이지 완료 처리 완료");
        }
        
        #endregion
    }
    
    /// <summary>
    /// 스테이지 목록을 JSON 직렬화하기 위한 헬퍼 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class StageList
    {
        public List<string> stages = new List<string>();
    }
    
    /// <summary>
    /// 아이템 목록을 JSON 직렬화하기 위한 헬퍼 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ItemList
    {
        public List<string> items = new List<string>();
    }
}
