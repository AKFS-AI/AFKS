using UnityEngine;
using System.Collections.Generic;
using AFKS.Shared.Interfaces;

namespace AFKS.StageSystem
{
    /// <summary>
    /// 스테이지 내 상호작용 포인트 데이터
    /// </summary>
    [System.Serializable]
    public class InteractionPoint
    {
        [Header("📋 기본 정보")]
        [SerializeField] public string id;
        [SerializeField] public string displayName;
        [SerializeField, TextArea(2, 3)] public string description;
        
        [Header("📍 위치 및 크기")]
        [SerializeField] public Vector2 position;
        [SerializeField] public Vector2 size = Vector2.one;
        
        [Header("🖱️ 상호작용 설정")]
        [SerializeField] public InteractionType interactionType;
        [SerializeField] public bool isEnabled = true;
        [SerializeField] public bool isVisible = true;
        
        [Header("🎨 시각적 피드백")]
        [SerializeField] public Sprite hoverSprite;
        [SerializeField] public Color hoverColor = Color.white;
        [SerializeField, Range(0.8f, 2f)] public float hoverScale = 1.1f;
        
        [Header("🔊 오디오")]
        [SerializeField] public AudioClip interactionSound;
        [SerializeField] public AudioClip hoverSound;
        
        [Header("🔒 조건")]
        [SerializeField] public List<string> requiredItems = new List<string>();
        [SerializeField] public List<StageCondition> enableConditions = new List<StageCondition>();
        
        [Header("📤 결과")]
        [SerializeField] public InteractionResult result;
        
        /// <summary>
        /// 상호작용 가능 여부 확인
        /// </summary>
        public bool CanInteract()
        {
            if (!isEnabled) return false;
            
            // 필요 아이템 체크 (ItemManager와 연동)
            foreach (string itemId in requiredItems)
            {
                if (AFKS.ItemSystem.ItemManager.Instance != null && 
                    !AFKS.ItemSystem.ItemManager.Instance.HasKeyItem(itemId))
                {
                    return false;
                }
            }
            
            // 활성화 조건 체크
            foreach (var condition in enableConditions)
            {
                if (!condition.IsConditionMet())
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 디버그 정보 반환
        /// </summary>
        public string GetDebugInfo()
        {
            return $"InteractionPoint[{id}] - {displayName} (Enabled: {isEnabled}, Visible: {isVisible})";
        }
    }
}