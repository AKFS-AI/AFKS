using UnityEngine;
using System.Collections.Generic;
using AFKS.Features.Stage.Events;
using AFKS.Features.Stage.Animations;

namespace AFKS.Features.Stage
{
    /// <summary>
    /// Stage1의 이벤트 시퀀스를 정의하는 ScriptableObject입니다.
    /// 문 클릭 → 카메라 줌 → 체인 클릭 → 체인 해금 → 다음 스테이지 순서로 진행됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage1 Event Sequence", menuName = "AFKS/Stage/Stage1 Event Sequence")]
    public class Stage1EventSequence : ScriptableObject
    {
        [Header("Stage1 이벤트 시퀀스")]
        [SerializeField] private List<StageEvent> eventSequence = new List<StageEvent>();
        
        [Header("설정")]
        [SerializeField] private string stageId = "Stage1";
        [SerializeField] private string description = "Stage1: 문 클릭 → 카메라 줌 → 체인 클릭 → 체인 해금 → 다음 스테이지";
        
        #region 공개 프로퍼티
        
        /// <summary>
        /// 이벤트 시퀀스를 반환합니다.
        /// </summary>
        public List<StageEvent> EventSequence => eventSequence;
        
        /// <summary>
        /// 스테이지 ID를 반환합니다.
        /// </summary>
        public string StageId => stageId;
        
        /// <summary>
        /// 스테이지 설명을 반환합니다.
        /// </summary>
        public string Description => description;
        
        #endregion
        
        #region 이벤트 시퀀스 설정
        
        /// <summary>
        /// 기본 이벤트 시퀀스를 설정합니다.
        /// </summary>
        [ContextMenu("기본 이벤트 시퀀스 설정")]
        public void SetupDefaultEventSequence()
        {
            eventSequence.Clear();
            
            // 1. 문 클릭 이벤트 (카메라 줌 트리거)
            var doorClickEvent = CreateInstance<ClickEvent>();
            doorClickEvent.name = "Door Click Event";
            doorClickEvent.SetupDoorClickEvent();
            eventSequence.Add(doorClickEvent);
            
            // 2. 카메라 줌 이벤트 (체인 영역으로 줌)
            var zoomEvent = CreateInstance<ZoomEvent>();
            zoomEvent.name = "Camera Zoom Event";
            zoomEvent.SetupChainAreaZoom();
            eventSequence.Add(zoomEvent);
            
            // 3. 체인 클릭 이벤트 (5번 클릭으로 해금)
            var chainClickEvent = CreateInstance<ClickEvent>();
            chainClickEvent.name = "Chain Click Event";
            chainClickEvent.SetupChainClickEvent();
            eventSequence.Add(chainClickEvent);
            
            // 4. 체인 해금 이벤트 (체인 끊어짐 애니메이션)
            var chainUnlockEvent = CreateInstance<ClickEvent>();
            chainUnlockEvent.name = "Chain Unlock Event";
            chainUnlockEvent.SetupChainUnlockEvent();
            eventSequence.Add(chainUnlockEvent);
            
            // 5. 다음 스테이지 전환 이벤트 (문 클릭으로 전환)
            var stageTransitionEvent = CreateInstance<ClickEvent>();
            stageTransitionEvent.name = "Stage Transition Event";
            stageTransitionEvent.SetupStageTransitionEvent();
            eventSequence.Add(stageTransitionEvent);
            
            Debug.Log("Stage1EventSequence: 기본 이벤트 시퀀스 설정 완료");
        }
        
        #endregion
        
        #region 유틸리티
        
        /// <summary>
        /// 이벤트 시퀀스의 유효성을 검사합니다.
        /// </summary>
        [ContextMenu("이벤트 시퀀스 유효성 검사")]
        public void ValidateEventSequence()
        {
            if (eventSequence.Count == 0)
            {
                Debug.LogWarning("Stage1EventSequence: 이벤트 시퀀스가 비어있습니다.");
                return;
            }
            
            for (int i = 0; i < eventSequence.Count; i++)
            {
                var stageEvent = eventSequence[i];
                if (stageEvent == null)
                {
                    Debug.LogError($"Stage1EventSequence: 이벤트 {i}가 null입니다.");
                    continue;
                }
                
                Debug.Log($"Stage1EventSequence: 이벤트 {i + 1} - {stageEvent.EventName}");
                
                // 트리거 오브젝트 확인
                var triggerObjects = stageEvent.GetInteractableObjects();
                if (triggerObjects.Count == 0)
                {
                    Debug.LogWarning($"Stage1EventSequence: 이벤트 {i + 1}에 트리거 오브젝트가 없습니다.");
                }
            }
            
            Debug.Log($"Stage1EventSequence: 총 {eventSequence.Count}개 이벤트 검사 완료");
        }
        
        #endregion
    }
}
