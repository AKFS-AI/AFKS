using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AFKS.Shared.Events;
using AFKS.Shared.Interfaces;
using AFKS.Shared.Utils;
using AFKS.StageSystem;

namespace AFKS.HorrorSystem
{
    /// <summary>
    /// 공포 이벤트를 관리하는 매니저
    /// </summary>
    public class HorrorEventManager : MonoBehaviour
    {
        [Header("😱 공포 설정")]
        [SerializeField, Tooltip("공포 이벤트 시스템 활성화 여부")] private bool enableHorrorEvents = true;
        [SerializeField, Range(1f, 30f), Tooltip("전역 공포 이벤트 쿨다운 시간 (초)")] private float globalCooldown = 5f;
        [SerializeField, Range(1, 5), Tooltip("동시 실행 가능한 최대 공포 이벤트 개수")] private int maxConcurrentEvents = 1;
        
        [Header("🎬 컴포넌트")]
        [SerializeField, Tooltip("공포 효과 전용 Canvas (최상위 레이어)")] private Canvas horrorCanvas;
        [SerializeField, Tooltip("공포 효과 전용 Camera (필요시)")] private Camera horrorCamera;
        
        [Header("📡 이벤트")]
        [SerializeField, Tooltip("공포 이벤트 시작 시 발생하는 게임 이벤트")] private GameEvent onHorrorEventTriggered;
        [SerializeField, Tooltip("공포 이벤트 완료 시 발생하는 게임 이벤트")] private GameEvent onHorrorEventCompleted;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<string> OnHorrorEventTriggered = new GameEvent<string>();
        public static readonly GameEvent<string> OnHorrorEventCompleted = new GameEvent<string>();
        public static readonly GameEvent<float> OnHorrorIntensityChanged = new GameEvent<float>();
        
        // === PROPERTIES ===
        public bool EnableHorrorEvents 
        { 
            get => enableHorrorEvents; 
            set => enableHorrorEvents = value; 
        }
        
        public float LastEventTime { get; private set; }
        public bool CanTriggerEvent => Time.time - LastEventTime >= globalCooldown;
        public int ActiveEventsCount => activeEvents.Count;
        public float CurrentIntensity { get; private set; }
        
        // === PRIVATE FIELDS ===
        private List<HorrorEventController> activeEvents = new List<HorrorEventController>();
        private Dictionary<string, HorrorEventData> registeredEvents = new Dictionary<string, HorrorEventData>();
        private Dictionary<string, float> eventCooldowns = new Dictionary<string, float>();
        
        // === SINGLETON ACCESS ===
        private static HorrorEventManager instance;
        public static HorrorEventManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<HorrorEventManager>();
                return instance;
            }
        }
        
        // === UNITY LIFECYCLE ===
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 스테이지 이벤트 리스너 등록
            StageManager.OnStageDataLoaded.AddListener(OnStageLoaded);
        }
        
        private void Update()
        {
            UpdateEventCooldowns();
            UpdateHorrorIntensity();
        }
        
        private void OnDestroy()
        {
            StageManager.OnStageDataLoaded.RemoveListener(OnStageLoaded);
            ForceCleanupAllEvents();
        }
        
        // === INITIALIZATION ===
        private void InitializeManager()
        {
            if (horrorCanvas == null)
            {
                CreateHorrorCanvas();
            }
            
            Debug.Log("[공포이벤트매니저] 초기화 완료");
        }
        
        /// <summary>
        /// 공포 이벤트용 캔버스 생성
        /// </summary>
        private void CreateHorrorCanvas()
        {
            GameObject canvasObject = new GameObject("HorrorCanvas");
            canvasObject.transform.SetParent(transform);
            
            horrorCanvas = canvasObject.AddComponent<Canvas>();
            horrorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            horrorCanvas.sortingOrder = 1000; // 최상위 레이어
            
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 초기에는 비활성화
            horrorCanvas.gameObject.SetActive(false);
        }
        
        // === EVENT MANAGEMENT ===
        
        /// <summary>
        /// 공포 이벤트 트리거
        /// </summary>
        /// <param name="eventId">이벤트 ID</param>
        /// <param name="forceExecute">강제 실행 여부</param>
        /// <returns>성공적으로 트리거되었으면 true</returns>
        public bool TriggerEvent(string eventId, bool forceExecute = false)
        {
            if (!enableHorrorEvents && !forceExecute)
            {
                Debug.Log("[공포이벤트매니저] 공포 이벤트 비활성화");
                return false;
            }
            
            if (!CanTriggerEvent && !forceExecute)
            {
                Debug.Log("[공포이벤트매니저] 전역 쿨다운 활성화");
                return false;
            }
            
            if (ActiveEventsCount >= maxConcurrentEvents && !forceExecute)
            {
                Debug.Log("[공포이벤트매니저] 최대 동시 이벤트 수에 도달");
                return false;
            }
            
            if (!registeredEvents.TryGetValue(eventId, out HorrorEventData eventData))
            {
                Debug.LogError($"[공포이벤트매니저] 이벤트를 찾을 수 없습니다: {eventId}");
                return false;
            }
            
            if (IsEventOnCooldown(eventId) && !forceExecute)
            {
                Debug.Log($"[공포이벤트매니저] 이벤트 쿨다운 중: {eventId}");
                return false;
            }
            
            StartCoroutine(ExecuteHorrorEvent(eventData));
            return true;
        }
        
        /// <summary>
        /// 공포 이벤트 실행
        /// </summary>
        private IEnumerator ExecuteHorrorEvent(HorrorEventData eventData)
        {
            LastEventTime = Time.time;
            
            // 쿨다운 설정
            if (!eventData.canRepeat)
            {
                eventCooldowns[eventData.eventId] = Time.time + eventData.cooldownTime;
            }
            
            // 이벤트 컨트롤러 생성
            HorrorEventController controller = CreateEventController(eventData);
            activeEvents.Add(controller);
            
            // 이벤트 알림
            onHorrorEventTriggered?.Raise();
            OnHorrorEventTriggered.Raise(eventData.eventId);
            
            // 트리거 딜레이
            if (eventData.triggerDelay > 0)
            {
                yield return new WaitForSeconds(eventData.triggerDelay);
            }
            
            // 이벤트 실행
            yield return StartCoroutine(controller.ExecuteEvent());
            
            // 정리 (안전한 리소스 해제)
            if (controller != null)
            {
                activeEvents.Remove(controller);
                
                // 컨트롤러 정리 후 제거
                controller.Reset();
                Destroy(controller.gameObject);
                controller = null;
            }
            
            // 완료 알림
            onHorrorEventCompleted?.Raise();
            OnHorrorEventCompleted.Raise(eventData.eventId);
            
            Debug.Log($"[공포이벤트매니저] 공포 이벤트 완료: {eventData.eventName}");
        }
        
        /// <summary>
        /// 이벤트 컨트롤러 생성
        /// </summary>
        private HorrorEventController CreateEventController(HorrorEventData eventData)
        {
            GameObject controllerObject = new GameObject($"HorrorEvent_{eventData.eventId}");
            controllerObject.transform.SetParent(horrorCanvas.transform, false);
            
            HorrorEventController controller = controllerObject.AddComponent<HorrorEventController>();
            controller.Initialize(eventData);
            
            return controller;
        }
        
        /// <summary>
        /// 랜덤 공포 이벤트 트리거
        /// </summary>
        public bool TriggerRandomEvent()
        {
            var availableEvents = registeredEvents.Values
                .Where(e => !IsEventOnCooldown(e.eventId))
                .ToList();
            
            if (availableEvents.Count == 0)
            {
                Debug.Log("[공포이벤트매니저] 랜덤 트리거에 사용가능한 이벤트가 없습니다");
                return false;
            }
            
            var randomEvent = availableEvents.GetRandomElement();
            return TriggerEvent(randomEvent.eventId);
        }
        
        /// <summary>
        /// 모든 활성 이벤트 중단
        /// </summary>
        public void StopAllEvents()
        {
            foreach (var controller in activeEvents.ToList())
            {
                if (controller != null)
                {
                    controller.StopEvent();
                    Destroy(controller.gameObject);
                }
            }
            
            activeEvents.Clear();
            Debug.Log("[공포이벤트매니저] 모든 공포 이벤트 중지");
        }
        
        // === EVENT REGISTRATION ===
        
        /// <summary>
        /// 공포 이벤트 등록
        /// </summary>
        public void RegisterEvent(HorrorEventData eventData)
        {
            if (eventData == null || string.IsNullOrEmpty(eventData.eventId))
            {
                Debug.LogWarning("[공포이벤트매니저] 잘못된 이벤트 데이터");
                return;
            }
            
            registeredEvents[eventData.eventId] = eventData;
            Debug.Log($"[공포이벤트매니저] 이벤트 등록: {eventData.eventName}");
        }
        
        /// <summary>
        /// 공포 이벤트 등록 해제
        /// </summary>
        public void UnregisterEvent(string eventId)
        {
            if (registeredEvents.ContainsKey(eventId))
            {
                registeredEvents.Remove(eventId);
                Debug.Log($"[공포이벤트매니저] 이벤트 등록 해제: {eventId}");
            }
        }
        
        /// <summary>
        /// 스테이지의 모든 이벤트 등록
        /// </summary>
        private void RegisterStageEvents(StageData stageData)
        {
            foreach (var horrorEvent in stageData.HorrorEvents)
            {
                RegisterEvent(horrorEvent);
            }
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 이벤트 쿨다운 상태 확인
        /// </summary>
        private bool IsEventOnCooldown(string eventId)
        {
            if (eventCooldowns.TryGetValue(eventId, out float cooldownTime))
            {
                return Time.time < cooldownTime;
            }
            return false;
        }
        
        /// <summary>
        /// 이벤트 쿨다운 업데이트
        /// </summary>
        private void UpdateEventCooldowns()
        {
            var expiredCooldowns = eventCooldowns
                .Where(kvp => Time.time >= kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (string eventId in expiredCooldowns)
            {
                eventCooldowns.Remove(eventId);
            }
        }
        
        /// <summary>
        /// 공포 강도 업데이트
        /// </summary>
        private void UpdateHorrorIntensity()
        {
            float targetIntensity = ActiveEventsCount > 0 ? 1f : 0f;
            CurrentIntensity = Mathf.Lerp(CurrentIntensity, targetIntensity, Time.deltaTime * 2f);
            
            OnHorrorIntensityChanged.Raise(CurrentIntensity);
        }
        
        // === EVENT HANDLERS ===
        
        /// <summary>
        /// 스테이지 로드 시 호출
        /// </summary>
        private void OnStageLoaded(StageData stageData)
        {
            // 이전 이벤트들 정리
            StopAllEvents();
            registeredEvents.Clear();
            
            // 새 스테이지 이벤트 등록
            RegisterStageEvents(stageData);
            
            // 자동 트리거 이벤트 체크
            CheckAutoTriggerEvents(stageData);
        }
        
        /// <summary>
        /// 자동 트리거 이벤트 확인
        /// </summary>
        private void CheckAutoTriggerEvents(StageData stageData)
        {
            foreach (var horrorEvent in stageData.HorrorEvents)
            {
                if (horrorEvent.triggerType == HorrorTriggerType.OnStageEnter)
                {
                    StartCoroutine(DelayedTrigger(horrorEvent.eventId, horrorEvent.triggerDelay));
                }
            }
        }
        
        /// <summary>
        /// 지연된 트리거
        /// </summary>
        private IEnumerator DelayedTrigger(string eventId, float delay)
        {
            yield return new WaitForSeconds(delay);
            TriggerEvent(eventId);
        }
        
        // === DEBUG METHODS ===
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보 출력")]
        public void PrintDebugInfo()
        {
            Debug.Log($"[공포이벤트매니저] === 디버그 정보 ===");
            Debug.Log($"Horror Events Enabled: {enableHorrorEvents}");
            Debug.Log($"Active Events: {ActiveEventsCount}/{maxConcurrentEvents}");
            Debug.Log($"Registered Events: {registeredEvents.Count}");
            Debug.Log($"Can Trigger Event: {CanTriggerEvent}");
            Debug.Log($"Current Intensity: {CurrentIntensity:F2}");
            
            foreach (var kvp in registeredEvents)
            {
                bool onCooldown = IsEventOnCooldown(kvp.Key);
                Debug.Log($"- {kvp.Value.eventName} (ID: {kvp.Key}) - Cooldown: {onCooldown}");
            }
        }
        
        /// <summary>
        /// 모든 활성 이벤트 강제 정리 (메모리 누수 방지)
        /// </summary>
        [ContextMenu("모든 이벤트 강제 정리")]
        public void ForceCleanupAllEvents()
        {
            Debug.Log("[공포이벤트매니저] 모든 활성 이벤트 강제 정리");
            
            // 활성 이벤트들 안전하게 정리
            var eventsToClean = new List<HorrorEventController>(activeEvents);
            foreach (var controller in eventsToClean)
            {
                if (controller != null)
                {
                    controller.Reset();
                    Destroy(controller.gameObject);
                }
            }
            
            activeEvents.Clear();
            Debug.Log($"[공포이벤트매니저] {eventsToClean.Count}개 활성 이벤트 정리 완료");
        }
        

    }
}