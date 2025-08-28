using UnityEngine;
using System.Collections.Generic;

namespace AFKS.Features.Stage.Conditions
{
    /// <summary>
    /// 특정 아이템들이 수집되었는지 확인하는 조건입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemCollectionCondition", menuName = "AFKS/Stage/Conditions/ItemCollectionCondition")]
    public class ItemCollectionCondition : StageCondition
    {
        [Header("아이템 수집 조건")]
        [SerializeField] private List<string> requiredItems = new List<string>();
        
        /// <summary>
        /// 필요한 아이템 목록
        /// </summary>
        public List<string> RequiredItems => requiredItems;
        
        /// <summary>
        /// StageEventSystem에서 조건이 충족되었는지 확인합니다.
        /// </summary>
        /// <param name="system">StageEventSystem</param>
        /// <returns>모든 필요한 아이템이 수집되었으면 true</returns>
        public override bool Evaluate(AFKS.Features.Stage.StageEventSystem system)
        {
            if (requiredItems == null || requiredItems.Count == 0)
                return true;
                
            // StoryProgress에서 수집된 아이템 확인
            var storyProgress = system?.StoryProgress;
            if (storyProgress == null)
                return false;
                
            // 모든 필요한 아이템이 수집되었는지 확인
            foreach (var itemId in requiredItems)
            {
                if (!storyProgress.IsItemCollected(itemId))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 조건 설명을 반환합니다.
        /// </summary>
        public string GetConditionDescription()
        {
            if (requiredItems == null || requiredItems.Count == 0)
                return "아이템 수집 조건 없음";
                
            return $"다음 아이템들을 수집해야 합니다: {string.Join(", ", requiredItems)}";
        }
    }
}
