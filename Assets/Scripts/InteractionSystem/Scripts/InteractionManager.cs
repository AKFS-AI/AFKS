using UnityEngine;
using System.Collections.Generic;
using AFKS.Shared.Events;
using AFKS.Shared.Interfaces;
using AFKS.StageSystem;

namespace AFKS.InteractionSystem
{
    /// <summary>
    /// 게임 전체 상호작용을 관리하는 매니저
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        [Header("⚙️ 상호작용 설정")]
        [SerializeField, Tooltip("전역 상호작용 시스템 활성화 여부")] private bool enableGlobalInteractions = true;
        [SerializeField, Range(0.1f, 2f), Tooltip("상호작용 간 최소 대기 시간 (초)")] private float interactionCooldown = 0.2f;
        [SerializeField, Tooltip("상호작용 가능한 레이어 마스크")] private LayerMask interactionLayer = -1;
        
        [Header("🐛 디버그")]
        [SerializeField, Tooltip("상호작용 디버그 정보 표시 여부")] private bool showDebugInfo = false;
        
        // === RUNTIME EVENTS ===
        public static readonly GameEvent<string> OnGlobalInteraction = new GameEvent<string>();
        public static readonly GameEvent<InteractionType> OnInteractionTypeChanged = new GameEvent<InteractionType>();
        
        // === PROPERTIES ===
        public bool EnableGlobalInteractions 
        { 
            get => enableGlobalInteractions; 
            set => enableGlobalInteractions = value; 
        }
        
        public float LastInteractionTime { get; private set; }
        public bool CanInteract => Time.time - LastInteractionTime >= interactionCooldown;
        
        // === PRIVATE FIELDS ===
        private Dictionary<string, IInteractable> registeredInteractables = new Dictionary<string, IInteractable>();
        private List<string> disabledInteractions = new List<string>();
        
        // === SINGLETON ACCESS ===
        private static InteractionManager instance;
        public static InteractionManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<InteractionManager>();
                return instance;
            }
        }
        
        // === UNITY LIFECYCLE ===
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                
                // 루트 GameObject로 설정하여 DontDestroyOnLoad 경고 방지
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }
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
            // 이벤트 리스너 등록
            StageInteractionController.OnInteractionTriggered.AddListener(OnInteractionTriggered);
            StageInteractionController.OnInteractionHover.AddListener(OnInteractionHover);
        }
        
        private void OnDestroy()
        {
            // 이벤트 리스너 해제
            StageInteractionController.OnInteractionTriggered.RemoveListener(OnInteractionTriggered);
            StageInteractionController.OnInteractionHover.RemoveListener(OnInteractionHover);
        }
        
        // === INITIALIZATION ===
        private void InitializeManager()
        {
            Debug.Log("[상호작용매니저] 초기화 완료");
        }
        
        // === INTERACTION REGISTRATION ===
        
        /// <summary>
        /// 상호작용 가능한 오브젝트 등록
        /// </summary>
        /// <param name="id">고유 ID</param>
        /// <param name="interactable">상호작용 인터페이스</param>
        public void RegisterInteractable(string id, IInteractable interactable)
        {
            if (string.IsNullOrEmpty(id) || interactable == null)
            {
                Debug.LogWarning("[상호작용매니저] 잘못된 등록 매개변수");
                return;
            }
            
            if (registeredInteractables.ContainsKey(id))
            {
                Debug.LogWarning($"[상호작용매니저] 이미 등록된 상호작용 객체: {id}");
                return;
            }
            
            registeredInteractables[id] = interactable;
            
            if (showDebugInfo)
            {
                Debug.Log($"[상호작용매니저] 상호작용 객체 등록: {id}");
            }
        }
        
        /// <summary>
        /// 상호작용 가능한 오브젝트 등록 해제
        /// </summary>
        /// <param name="id">고유 ID</param>
        public void UnregisterInteractable(string id)
        {
            if (registeredInteractables.ContainsKey(id))
            {
                registeredInteractables.Remove(id);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[상호작용매니저] 상호작용 객체 등록 해제: {id}");
                }
            }
        }
        
        /// <summary>
        /// 모든 상호작용 등록 해제
        /// </summary>
        public void ClearAllInteractables()
        {
            registeredInteractables.Clear();
            Debug.Log("[상호작용매니저] 모든 상호작용 객체 제거 완료");
        }
        
        // === INTERACTION CONTROL ===
        
        /// <summary>
        /// 특정 상호작용 비활성화
        /// </summary>
        /// <param name="id">상호작용 ID</param>
        public void DisableInteraction(string id)
        {
            if (!disabledInteractions.Contains(id))
            {
                disabledInteractions.Add(id);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[상호작용매니저] 상호작용 비활성화: {id}");
                }
            }
        }
        
        /// <summary>
        /// 특정 상호작용 활성화
        /// </summary>
        /// <param name="id">상호작용 ID</param>
        public void EnableInteraction(string id)
        {
            if (disabledInteractions.Contains(id))
            {
                disabledInteractions.Remove(id);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[상호작용매니저] 상호작용 활성화: {id}");
                }
            }
        }
        
        /// <summary>
        /// 상호작용 활성화 상태 확인
        /// </summary>
        /// <param name="id">상호작용 ID</param>
        /// <returns>활성화되어 있으면 true</returns>
        public bool IsInteractionEnabled(string id)
        {
            return !disabledInteractions.Contains(id);
        }
        
        /// <summary>
        /// 모든 상호작용 비활성화
        /// </summary>
        public void DisableAllInteractions()
        {
            enableGlobalInteractions = false;
            Debug.Log("[상호작용매니저] 모든 상호작용 비활성화 완료");
        }
        
        /// <summary>
        /// 모든 상호작용 활성화
        /// </summary>
        public void EnableAllInteractions()
        {
            enableGlobalInteractions = true;
            Debug.Log("[상호작용매니저] 모든 상호작용 활성화 완료");
        }
        
        // === INTERACTION EXECUTION ===
        
        /// <summary>
        /// 특정 ID의 상호작용 실행
        /// </summary>
        /// <param name="id">상호작용 ID</param>
        public void TriggerInteraction(string id)
        {
            if (!CanInteractWithId(id)) return;
            
            if (registeredInteractables.TryGetValue(id, out IInteractable interactable))
            {
                if (interactable.CanInteract())
                {
                    LastInteractionTime = Time.time;
                    interactable.OnInteract();
                    
                    OnGlobalInteraction.Raise(id);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[상호작용매니저] 상호작용 실행: {id}");
                    }
                }
                else
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[상호작용매니저] 상호작용을 사용할 수 없습니다: {id}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[상호작용매니저] 상호작용을 찾을 수 없습니다: {id}");
            }
        }
        
        /// <summary>
        /// 특정 ID와 상호작용 가능한지 확인
        /// </summary>
        /// <param name="id">상호작용 ID</param>
        /// <returns>상호작용 가능하면 true</returns>
        public bool CanInteractWithId(string id)
        {
            if (!enableGlobalInteractions) return false;
            if (!CanInteract) return false;
            if (!IsInteractionEnabled(id)) return false;
            
            return registeredInteractables.ContainsKey(id);
        }
        
        // === EVENT HANDLERS ===
        
        /// <summary>
        /// 상호작용 트리거 이벤트 처리
        /// </summary>
        private void OnInteractionTriggered(string interactionId)
        {
            LastInteractionTime = Time.time;
            
            if (showDebugInfo)
            {
                Debug.Log($"[상호작용매니저] 상호작용 트리거: {interactionId}");
            }
            
            // 상호작용별 특수 처리
            ProcessSpecialInteractions(interactionId);
        }
        
        /// <summary>
        /// 상호작용 호버 이벤트 처리
        /// </summary>
        private void OnInteractionHover(string interactionId)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[상호작용매니저] 상호작용 호버: {interactionId}");
            }
        }
        
        /// <summary>
        /// 특수 상호작용 처리
        /// </summary>
        private void ProcessSpecialInteractions(string interactionId)
        {
            switch (interactionId)
            {
                case "hospital_door":
                    // 병원 문 상호작용 특수 처리
                    ProcessHospitalDoorInteraction();
                    break;
                    
                case "cross_pickup":
                    // 십자가 수집 특수 처리
                    ProcessCrossPickupInteraction();
                    break;
                    
                case "cctv_monitor":
                    // CCTV 모니터 특수 처리
                    ProcessCCTVInteraction();
                    break;
                    
                default:
                    // 기본 처리
                    break;
            }
        }
        
        /// <summary>
        /// 병원 문 상호작용 처리
        /// </summary>
        private void ProcessHospitalDoorInteraction()
        {
            // 병원 문 클릭 시 다음 스테이지로 이동
            if (StageManager.Instance != null)
            {
                StageManager.Instance.NextStage();
            }
        }
        
        /// <summary>
        /// 십자가 수집 상호작용 처리
        /// </summary>
        private void ProcessCrossPickupInteraction()
        {
            // 십자가를 인벤토리에 추가
            // InventoryManager.Instance.AddItem("cross");
            Debug.Log("[상호작용매니저] 십자가 수집됨");
        }
        
        /// <summary>
        /// CCTV 상호작용 처리
        /// </summary>
        private void ProcessCCTVInteraction()
        {
            // CCTV 화면 켜기 및 공포 이벤트 트리거
            // HorrorEventManager.Instance.TriggerEvent("cctv_ghost");
            Debug.Log("[상호작용매니저] CCTV 활성화");
        }
        
        // === UTILITY METHODS ===
        
        /// <summary>
        /// 등록된 상호작용 목록 반환
        /// </summary>
        /// <returns>상호작용 ID 배열</returns>
        public string[] GetRegisteredInteractionIds()
        {
            string[] ids = new string[registeredInteractables.Count];
            registeredInteractables.Keys.CopyTo(ids, 0);
            return ids;
        }
        
        /// <summary>
        /// 비활성화된 상호작용 목록 반환
        /// </summary>
        /// <returns>비활성화된 상호작용 ID 배열</returns>
        public string[] GetDisabledInteractionIds()
        {
            return disabledInteractions.ToArray();
        }
        
        /// <summary>
        /// 상호작용 통계 반환
        /// </summary>
        /// <returns>상호작용 통계 정보</returns>
        public InteractionStats GetInteractionStats()
        {
            return new InteractionStats
            {
                totalRegistered = registeredInteractables.Count,
                totalDisabled = disabledInteractions.Count,
                globalEnabled = enableGlobalInteractions,
                lastInteractionTime = LastInteractionTime
            };
        }
        
        // === DEBUG METHODS ===
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보 출력")]
        public void PrintDebugInfo()
        {
            Debug.Log($"[상호작용매니저] === 디버그 정보 ===");
            Debug.Log($"Global Interactions Enabled: {enableGlobalInteractions}");
            Debug.Log($"Registered Interactions: {registeredInteractables.Count}");
            Debug.Log($"Disabled Interactions: {disabledInteractions.Count}");
            Debug.Log($"Last Interaction Time: {LastInteractionTime}");
            Debug.Log($"Can Interact: {CanInteract}");
            
            foreach (var kvp in registeredInteractables)
            {
                Debug.Log($"- {kvp.Key}: {kvp.Value.GetType().Name}");
            }
        }
    }
    
    // === DATA STRUCTURES ===
    
    [System.Serializable]
    public struct InteractionStats
    {
        public int totalRegistered;
        public int totalDisabled;
        public bool globalEnabled;
        public float lastInteractionTime;
    }
}