using UnityEngine;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 스테이지 조건 데이터 (잠금 해제, 활성화 등)
    /// </summary>
    [System.Serializable]
    public class StageCondition
    {
        [Header("🎯 조건 설정")]
        [SerializeField] public ConditionType conditionType;
        [SerializeField] public string targetId;
        [SerializeField] public string requiredValue;
        
        [Header("🔄 논리 설정")]
        [SerializeField] public bool invertCondition = false;
        
        /// <summary>
        /// 조건 확인 (실제 게임 상태와 연동)
        /// </summary>
        public bool IsConditionMet()
        {
            bool result = false;
            
            try
            {
                switch (conditionType)
                {
                    case ConditionType.HasItem:
                        result = CheckItemCondition();
                        break;
                        
                    case ConditionType.StageCompleted:
                        result = CheckStageCondition();
                        break;
                        
                    case ConditionType.VariableEquals:
                        result = CheckVariableCondition();
                        break;
                        
                    case ConditionType.Always:
                        result = true;
                        break;
                        
                    case ConditionType.Never:
                        result = false;
                        break;
                        
                    default:
                        Debug.LogWarning($"[StageCondition] 알 수 없는 조건 타입: {conditionType}");
                        result = false;
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StageCondition] 조건 확인 중 오류 발생: {e.Message}");
                result = false;
            }
            
            return invertCondition ? !result : result;
        }
        
        /// <summary>
        /// 아이템 보유 조건 확인
        /// </summary>
        private bool CheckItemCondition()
        {
            if (string.IsNullOrEmpty(targetId))
            {
                Debug.LogWarning("[StageCondition] 아이템 조건에서 대상 ID가 비어있습니다.");
                return false;
            }
            
            var itemManager = AFKS.ItemSystem.ItemManager.Instance;
            if (itemManager == null)
            {
                Debug.LogWarning("[StageCondition] ItemManager 인스턴스를 찾을 수 없습니다.");
                return false;
            }
            
            return itemManager.HasKeyItem(targetId);
        }
        
        /// <summary>
        /// 스테이지 완료 조건 확인
        /// </summary>
        private bool CheckStageCondition()
        {
            if (!int.TryParse(targetId, out int stageIndex))
            {
                Debug.LogWarning($"[StageCondition] 스테이지 인덱스 파싱 실패: {targetId}");
                return false;
            }
            
            var gameManager = AFKS.Core.GameManager.Instance;
            var stageManager = AFKS.StageSystem.StageManager.Instance;
            
            if (gameManager == null || stageManager == null)
            {
                Debug.LogWarning("[StageCondition] GameManager 또는 StageManager 인스턴스를 찾을 수 없습니다.");
                return false;
            }
            
            return stageManager.CurrentStageIndex >= stageIndex;
        }
        
        /// <summary>
        /// 변수 값 조건 확인
        /// </summary>
        private bool CheckVariableCondition()
        {
            if (string.IsNullOrEmpty(targetId))
            {
                Debug.LogWarning("[StageCondition] 변수 조건에서 대상 ID가 비어있습니다.");
                return false;
            }

            // SaveManager 변수 번들에서 조회하여 비교 (PlayerPrefs 직접 접근 제거)
            string value;
            if (AFKS.Shared.Core.SaveManager.HasInstance && AFKS.Shared.Core.SaveManager.Instance.TryGetVariable(targetId, out value))
            {
                return value == requiredValue;
            }
            return false;
        }
        
        /// <summary>
        /// 조건 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            switch (conditionType)
            {
                case ConditionType.HasItem:
                case ConditionType.StageCompleted:
                case ConditionType.VariableEquals:
                    if (string.IsNullOrEmpty(targetId))
                    {
                        Debug.LogWarning($"[StageCondition] 조건 타입 '{conditionType}'에서 대상 ID가 필요합니다.");
                        return false;
                    }
                    break;
                    
                case ConditionType.Always:
                case ConditionType.Never:
                    // 이 조건들은 추가 매개변수가 필요하지 않음
                    break;
                    
                default:
                    Debug.LogWarning($"[StageCondition] 알 수 없는 조건 타입: {conditionType}");
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 디버그 정보 반환
        /// </summary>
        public string GetDebugInfo()
        {
            string invertText = invertCondition ? " (역전)" : "";
            return $"Condition[{conditionType}] Target: {targetId}, Value: {requiredValue}{invertText}";
        }
        
        /// <summary>
        /// 조건 설명 텍스트 생성 (UI 표시용)
        /// </summary>
        public string GetDescription()
        {
            string description = conditionType switch
            {
                ConditionType.HasItem => $"아이템 '{targetId}' 보유",
                ConditionType.StageCompleted => $"스테이지 {targetId} 완료",
                ConditionType.VariableEquals => $"변수 '{targetId}'가 '{requiredValue}'",
                ConditionType.Always => "항상 참",
                ConditionType.Never => "항상 거짓",
                _ => "알 수 없는 조건"
            };
            
            if (invertCondition)
            {
                description = "NOT " + description;
            }
            
            return description;
        }
    }
    
    /// <summary>
    /// 조건 타입 열거형
    /// </summary>
    public enum ConditionType
    {
        Always,            // 항상 참
        Never,             // 항상 거짓
        HasItem,           // 아이템 보유
        StageCompleted,    // 스테이지 완료
        VariableEquals     // 변수 값 일치
    }
}