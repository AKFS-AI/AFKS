using UnityEngine;
using System.Collections.Generic;
using AFKS.Features.Stage.Events;
using AFKS.Core.Utils;
using AFKS.Core.Services.Save;
using AFKS.Core.Services;

namespace AFKS.Features.Stage
{
    /// <summary>
    /// 모든 스테이지에서 공통으로 사용하는 이벤트 시스템의 메인 컨트롤러입니다.
    /// 클릭 순서에 따른 스토리 진행, 애니메이션, 줌 시스템을 통합 관리합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Stage/Stage Event System")]
    public class StageEventSystem : MonoBehaviour
    {
        [Header("스테이지 설정")]
        [SerializeField] private string stageId = "Stage1";
        [SerializeField] private StageType stageType = StageType.Story; // 참조 유지용: 인스펙터에서 스테이지 성격 확인
        
        [Header("이벤트 시퀀스")]
        [SerializeField] private List<StageEvent> eventSequence = new List<StageEvent>();
        [SerializeField] private int currentEventIndex = 0;
        
        [Header("애니메이션 컨트롤러")]
        [SerializeField] private MonoBehaviour animationController;
        
        [Header("상호작용 관리")]
        [SerializeField] private StageInteractionSystem interactionManager;
        [Header("실행기")]
        [SerializeField] private IStageEventRunner eventRunner;
        
        [Header("스토리 진행")]
        [SerializeField] private StoryProgress storyProgress;
        
        /// <summary>
        /// 스토리 진행 상태에 접근할 수 있는 public 속성입니다.
        /// </summary>
        public StoryProgress StoryProgress => storyProgress;
        
        [Header("상태")]
        [SerializeField] private StageState currentState = StageState.Initial;
        [SerializeField] private bool isEventPlaying = false;
        [SerializeField] private bool enableCheckpoint = true;

        // 클릭/상태 추적
        private readonly Dictionary<string, int> objectIdToClickCount = new Dictionary<string, int>();
        private readonly List<string> clickHistory = new List<string>(32);
        
        #region Unity 수명주기
        
        private void Awake()
        {
            InitializeComponents();
            ValidateEventSequence();
            if (eventRunner == null) eventRunner = new DefaultStageEventRunner();
        }
        
        private void Start()
        {
            InitializeStage();
            
            // 초기 배경 보고 (Stage1Controller.ReportCurrentBackground() 대체)
            ReportInitialBackground();
        }
        
        #endregion
        
        #region 초기화
        
        private void InitializeComponents()
        {
            // 자동 생성 제거: 에디터에서 명시적으로 연결해야 합니다.
        }
        
        private void ValidateEventSequence()
        {
            if (eventSequence.Count == 0)
            {
                Debug.LogWarning($"StageEventSystem: {stageId}에 이벤트 시퀀스가 설정되지 않았습니다.");
            }
        }
        
        private void InitializeStage()
        {
            currentState = StageState.Initial;
            currentEventIndex = 0;
            
            // 이벤트 체크포인트 로드(EVT:<index>)
            if (enableCheckpoint && ServiceLocator.TryGet<ISaveService>(out var save))
            {
                var cp = save.GetStageCheckpoint(stageId);
                if (!string.IsNullOrEmpty(cp) && cp.StartsWith("EVT:"))
                {
                    if (int.TryParse(cp.Substring(4), out var idx))
                    {
                        currentEventIndex = Mathf.Clamp(idx, 0, Mathf.Max(0, eventSequence.Count - 1));
                    }
                }
            }
            
            // 첫 번째 이벤트 준비
            if (eventSequence.Count > 0)
            {
                PrepareNextEvent();
            }
            
            Debug.Log($"StageEventSystem: {stageId} 초기화 완료 - 총 {eventSequence.Count}개 이벤트, StageType={stageType}");
        }
        
        #endregion
        
        #region 배경 관리
        
        /// <summary>
        /// 초기 배경을 StageBackgroundService에 보고합니다.
        /// </summary>
        private void ReportInitialBackground()
        {
            if (ServiceLocator.TryGet<AFKS.Core.Services.StageBackground.IStageBackgroundService>(out var bgService))
            {
                // 배경 오브젝트 찾기 (Background 또는 Stage1_Background 등)
                var backgroundObj = GameObject.Find("Background") ?? GameObject.Find($"{stageId}_Background");
                if (backgroundObj != null)
                {
                    var spriteRenderer = backgroundObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        bgService.ReportBackground(stageId, spriteRenderer.sprite);
                        Debug.Log($"StageEventSystem: {stageId} 초기 배경 보고 완료");
                        return;
                    }
                }
                
                // 배경을 찾지 못한 경우 기본 배경 사용
                Debug.LogWarning($"StageEventSystem: {stageId} 배경을 찾을 수 없습니다. 기본 배경 사용");
            }
        }
        
        #endregion
        
        #region 공개 API
        
        /// <summary>
        /// 스테이지 ID를 반환합니다.
        /// </summary>
        public string StageId => stageId;
        
        /// <summary>
        /// 지정된 ID의 오브젝트를 찾아 반환합니다.
        /// </summary>
        /// <param name="objectId">찾을 오브젝트의 ID</param>
        /// <returns>찾은 오브젝트 또는 null</returns>
        public GameObject GetObject(string objectId)
        {
            if (string.IsNullOrEmpty(objectId)) return null;
            
            Debug.Log($"StageEventSystem.GetObject: {objectId} 검색 시작");
            
            // StageInteractionSystem에서 먼저 찾기
            var interactionManager = StageInteractionSystem.Instance;
            if (interactionManager != null)
            {
                var obj = interactionManager.GetObject(objectId);
                if (obj != null)
                {
                    Debug.Log($"StageEventSystem.GetObject: {objectId}를 StageInteractionSystem에서 발견 - {obj.name}");
                    return obj;
                }
            }
            else
            {
                Debug.LogWarning("StageEventSystem.GetObject: StageInteractionSystem를 찾을 수 없습니다.");
            }
            
            // 씬에서 직접 찾기
            var foundObj = GameObject.Find(objectId);
            if (foundObj != null)
            {
                Debug.Log($"StageEventSystem.GetObject: {objectId}를 씬에서 직접 발견 - {foundObj.name}");
                return foundObj;
            }
            
            // 스테이지 ID를 접두사로 사용해서 찾기
            var stagePrefixedObj = GameObject.Find($"{stageId}_{objectId}");
            if (stagePrefixedObj != null)
            {
                Debug.Log($"StageEventSystem.GetObject: {objectId}를 스테이지 접두사로 발견 - {stagePrefixedObj.name}");
                return stagePrefixedObj;
            }
            
            Debug.LogWarning($"StageEventSystem.GetObject: {objectId}를 찾을 수 없습니다.");
            return null;
        }
        
        /// <summary>
        /// 특정 오브젝트 클릭 시 호출되는 메서드입니다.
        /// </summary>
        /// <param name="objectId">클릭된 오브젝트의 ID</param>
        public void OnObjectClicked(string objectId)
        {
            if (isEventPlaying) return;
            
            // 클릭 카운트/히스토리 갱신
            if (!string.IsNullOrEmpty(objectId))
            {
                if (!objectIdToClickCount.ContainsKey(objectId)) objectIdToClickCount[objectId] = 0;
                objectIdToClickCount[objectId]++;
                clickHistory.Add(objectId);
                if (clickHistory.Count > 128) clickHistory.RemoveAt(0);
            }

            // 현재 이벤트에서 해당 오브젝트 클릭 처리 및 조건 평가
            if (currentEventIndex < eventSequence.Count)
            {
                var currentEvent = eventSequence[currentEventIndex];
                bool canTrigger = currentEvent.CanTriggerWith(objectId);
                bool conditionsSatisfied = currentEvent.AreConditionsSatisfied(this);
                
                // 클릭 피드백 실행 (조건 만족 여부와 관계없이)
                if (canTrigger && currentEvent is ClickEvent clickEvent)
                {
                    clickEvent.ExecuteClickFeedback(objectId, this);
                }
                
                if (canTrigger && conditionsSatisfied)
                {
                    ExecuteEvent(currentEvent);
                }
            }
        }
        
        /// <summary>
        /// 다음 이벤트로 진행합니다.
        /// </summary>
        public void ProceedToNextEvent()
        {
            if (isEventPlaying) return;
            
            currentEventIndex++;
            
            if (currentEventIndex < eventSequence.Count)
            {
                PrepareNextEvent();
                Debug.Log($"StageEventSystem: 다음 이벤트로 진행 - {currentEventIndex + 1}/{eventSequence.Count}");
                SaveCheckpoint();
            }
            else
            {
                OnStageComplete();
                SaveCheckpoint();
            }
        }
        
        /// <summary>
        /// 특정 이벤트 인덱스로 점프합니다.
        /// </summary>
        /// <param name="eventIndex">점프할 이벤트 인덱스</param>
        public void JumpToEvent(int eventIndex)
        {
            if (eventIndex < 0 || eventIndex >= eventSequence.Count) return;
            
            currentEventIndex = eventIndex;
            PrepareNextEvent();
            Debug.Log($"StageEventSystem: 이벤트 {eventIndex}로 점프");
            SaveCheckpoint();
        }
        
        /// <summary>
        /// 현재 이벤트를 다시 실행합니다.
        /// </summary>
        public void ReplayCurrentEvent()
        {
            if (currentEventIndex < eventSequence.Count)
            {
                var currentEvent = eventSequence[currentEventIndex];
                ExecuteEvent(currentEvent);
            }
        }
        
        #endregion
        
        #region 내부 메서드
        
        private void PrepareNextEvent()
        {
            if (currentEventIndex >= eventSequence.Count) return;
            EnsureEventRunner();
            EnsureInteractionManager();

            var nextEvent = eventSequence[currentEventIndex];
            if (nextEvent == null)
            {
                Debug.LogWarning($"StageEventSystem: 이벤트 인스턴스가 null입니다. index={currentEventIndex}");
                return;
            }
            Debug.Log($"PrepareNextEvent: index={currentEventIndex}, name={nextEvent.EventName}");
            eventRunner.Prepare(nextEvent, interactionManager);
            Debug.Log($"이벤트 준비됨 - {nextEvent.EventName}");

            // 클릭 트리거가 아니거나, 클릭 트리거지만 트리거 오브젝트가 없는 경우 즉시 실행
            if (nextEvent.TriggerType != Events.EventTriggerType.Click || !nextEvent.HasTriggerObjects)
            {
                ExecuteEvent(nextEvent);
            }
        }
        
        private void ExecuteEvent(StageEvent stageEvent)
        {
            isEventPlaying = true;
            currentState = StageState.EventPlaying;
            
            // Debug 축소: 핵심 진행만 유지
            Debug.Log($"이벤트 실행 시작 - {stageEvent.EventName}");
            
            // 이벤트 실행
            EnsureEventRunner();
            eventRunner.Execute(stageEvent, () => { OnEventComplete(); });
        }
        
        private void OnEventComplete()
        {
            isEventPlaying = false;
            currentState = StageState.EventComplete;
            
            Debug.Log($"이벤트 완료 - {eventSequence[currentEventIndex].EventName}");
            // 효과 적용
            var ev = eventSequence[currentEventIndex];
            if (ev != null) ev.ApplyEffects(this);
            SaveCheckpoint();
            
            // 자동 진행 여부 확인
            var shouldAutoProceed = eventRunner.ShouldAutoProceed(eventSequence[currentEventIndex]);
            Debug.Log($"자동 진행 확인: {shouldAutoProceed}, 이벤트 AutoProceed: {ev?.AutoProceed}");
            
            if (shouldAutoProceed)
            {
                Debug.Log("자동으로 다음 이벤트로 진행합니다.");
                ProceedToNextEvent();
            }
            else
            {
                Debug.Log("자동 진행이 비활성화되어 있습니다. 사용자 입력을 기다립니다.");
            }
        }
        
        private void OnStageComplete()
        {
            currentState = StageState.Completed;
            Debug.Log($"StageEventSystem: {stageId} 완료!");
            
            // 스테이지 완료 이벤트 발생
            storyProgress.OnStageComplete(stageId);
        }
        
        #endregion
        
        #region 상태 및 정보
        
        /// <summary>
        /// 현재 스테이지 상태를 반환합니다.
        /// </summary>
        public StageState CurrentState => currentState;
        
        /// <summary>
        /// 현재 이벤트 진행률을 반환합니다 (0.0 ~ 1.0).
        /// </summary>
        public float Progress => eventSequence.Count > 0 ? (float)currentEventIndex / eventSequence.Count : 0f;
        
        /// <summary>
        /// 현재 이벤트 인덱스를 반환합니다.
        /// </summary>
        public int CurrentEventIndex => currentEventIndex;
        
        /// <summary>
        /// 총 이벤트 개수를 반환합니다.
        /// </summary>
        public int TotalEventCount => eventSequence.Count;
        
        /// <summary>
        /// 현재 이벤트가 진행 중인지 확인합니다.
        /// </summary>
        public bool IsEventPlaying => isEventPlaying;
        
        /// <summary>
        /// 이벤트 시퀀스를 설정합니다.
        /// </summary>
        /// <param name="newEventSequence">설정할 이벤트 시퀀스</param>
        public void SetEventSequence(List<StageEvent> newEventSequence)
        {
            eventSequence.Clear();
            if (newEventSequence != null)
            {
                for (int i = 0; i < newEventSequence.Count; i++)
                {
                    var ev = newEventSequence[i];
                    if (ev != null) eventSequence.Add(ev);
                }
            }
            currentEventIndex = 0;
            
            Debug.Log($"이벤트 시퀀스 설정됨 - {eventSequence.Count}개 이벤트");
            
            // 첫 번째 이벤트 준비
            if (eventSequence.Count > 0)
            {
                PrepareNextEvent();
            }
        }

        public void LoadFromDefinition(StageDefinition def)
        {
            if (def == null)
            {
                Debug.LogError("StageEventSystem.LoadFromDefinition: null definition");
                return;
            }
            stageId = def.StageId;
            SetEventSequence(def.Events);
        }
        
        private void SaveCheckpoint()
        {
            if (!enableCheckpoint) return;
            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                save.SetStageCheckpoint(stageId, $"EVT:{currentEventIndex}");
            }
        }

        private void EnsureInteractionManager()
        {
            if (interactionManager == null)
            {
                interactionManager = gameObject.GetComponent<StageInteractionSystem>();
                if (interactionManager == null)
                {
                    interactionManager = gameObject.AddComponent<StageInteractionSystem>();
                }
            }
        }

        private void EnsureEventRunner()
        {
            if (eventRunner == null)
            {
                eventRunner = new DefaultStageEventRunner();
            }
        }

        #region 조건/효과 지원 유틸
        public int GetClickCount(string objectId)
        {
            return objectIdToClickCount.TryGetValue(objectId, out var c) ? c : 0;
        }

        public bool DoesClickHistoryMatch(List<string> sequence, bool allowExtra)
        {
            if (sequence == null || sequence.Count == 0) return false;
            if (!allowExtra && clickHistory.Count != sequence.Count) return false;
            if (allowExtra && clickHistory.Count < sequence.Count) return false;
            int offset = allowExtra ? (clickHistory.Count - sequence.Count) : 0;
            for (int i = 0; i < sequence.Count; i++)
            {
                if (!string.Equals(clickHistory[i + offset], sequence[i])) return false;
            }
            return true;
        }

        public void SetObjectInteractable(string objectId, bool interactable)
        {
            EnsureInteractionManager();
            if (interactionManager != null)
            {
                interactionManager.SetObjectInteractable(objectId, interactable);
            }
        }
        #endregion
        
        #endregion
    }
    
    /// <summary>
    /// 스테이지 타입을 정의합니다.
    /// </summary>
    public enum StageType
    {
        Story,      // 스토리 진행
        Puzzle,     // 퍼즐 해결
        Action,     // 액션/전투
        Exploration // 탐험/자유
    }
    
    /// <summary>
    /// 스테이지의 현재 상태를 정의합니다.
    /// </summary>
    public enum StageState
    {
        Initial,        // 초기화
        EventPlaying,   // 이벤트 진행 중
        EventComplete,  // 이벤트 완료
        Completed       // 스테이지 완료
    }
}
