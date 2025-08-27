using UnityEngine;
using System.Collections.Generic;
using AFKS.Features.Interaction;

namespace AFKS.Features.Stage
{
    /// <summary>
    /// 스테이지의 상호작용 가능한 오브젝트들을 제어하는 매니저입니다.
    /// 각 이벤트에 따라 클릭 가능한 오브젝트를 동적으로 제어합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Stage/Stage Interaction Manager")]
    public class StageInteractionManager : MonoBehaviour
    {
        // ID → 상호작용 객체 맵(빠른 조회/토글용)
        private readonly System.Collections.Generic.Dictionary<string, InteractableObject> idToInteractable = new System.Collections.Generic.Dictionary<string, InteractableObject>();

        public static StageInteractionManager Instance { get; private set; }

        [Header("상호작용 오브젝트")]
        [SerializeField] private List<InteractableObject> interactableObjects = new List<InteractableObject>();
        
        [Header("상태")]
        [SerializeField] private bool isInitialized = false;
        
        #region Unity 수명주기
        
        private void Awake()
        {
            Instance = this;
            InitializeInteractableObjects();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        
        #endregion
        
        #region 초기화
        
        private void InitializeInteractableObjects()
        {
            if (isInitialized) return;
            // 맵 초기화 및 목록 동기화
            idToInteractable.Clear();
            for (int i = 0; i < interactableObjects.Count; i++)
            {
                var io = interactableObjects[i];
                if (io == null || string.IsNullOrEmpty(io.objectId) || io.gameObject == null) continue;
                if (!idToInteractable.ContainsKey(io.objectId))
                {
                    idToInteractable.Add(io.objectId, io);
                }
            }
            // 자동 등록/연결은 사용하지 않습니다. 에디터에서 명시적으로 설정합니다.
            isInitialized = true;
        }
        
        #endregion
        
        #region 공개 API
        
        /// <summary>
        /// 상호작용 가능한 오브젝트들을 설정합니다.
        /// </summary>
        /// <param name="objectIds">상호작용 가능한 오브젝트 ID 목록</param>
        public void SetInteractableObjects(List<string> objectIds)
        {
            // 모든 오브젝트를 비활성화
            foreach (var obj in interactableObjects)
            {
                SetObjectInteractable(obj.objectId, false);
            }

            // 지정된 오브젝트들만 활성화
            if (objectIds != null)
            {
                foreach (var objectId in objectIds)
                {
                    SetObjectInteractable(objectId, true);
                }

                // Door가 포함되면 Collider 활성 보장(체인 해금 전 줌을 위해)
                if (objectIds.Contains("Door"))
                {
                    if (TryGet("Door", out var io) && io?.gameObject != null)
                    {
                        var col = io.gameObject.GetComponent<Collider2D>();
                        if (col != null) col.enabled = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// 특정 오브젝트의 상호작용 가능 여부를 설정합니다.
        /// </summary>
        /// <param name="objectId">오브젝트 ID</param>
        /// <param name="interactable">상호작용 가능 여부</param>
        public void SetObjectInteractable(string objectId, bool interactable)
        {
            var obj = FindInteractableObject(objectId);
            if (obj != null)
            {
                obj.isInteractable = interactable;
                obj.clickHandler.enabled = interactable;
                
                // 시각적 피드백 (선택사항)
                var spriteRenderer = obj.gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    var color = spriteRenderer.color;
                    color.a = interactable ? 1f : 0.5f;
                    spriteRenderer.color = color;
                }
                
                
            }
        }
        
        /// <summary>
        /// 특정 오브젝트를 숨깁니다.
        /// </summary>
        /// <param name="objectId">숨길 오브젝트 ID</param>
        public void HideObject(string objectId)
        {
            var obj = FindInteractableObject(objectId);
            if (obj != null)
            {
                obj.gameObject.SetActive(false);
                
            }
        }
        
        /// <summary>
        /// 특정 오브젝트를 표시합니다.
        /// </summary>
        /// <param name="objectId">표시할 오브젝트 ID</param>
        public void ShowObject(string objectId)
        {
            var obj = FindInteractableObject(objectId);
            if (obj != null)
            {
                obj.gameObject.SetActive(true);
                
            }
        }
        
        /// <summary>
        /// 모든 상호작용 오브젝트를 숨깁니다.
        /// </summary>
        public void HideAllObjects()
        {
            foreach (var obj in interactableObjects)
            {
                obj.gameObject.SetActive(false);
            }
            
        }
        
        /// <summary>
        /// 모든 상호작용 오브젝트를 표시합니다.
        /// </summary>
        public void ShowAllObjects()
        {
            foreach (var obj in interactableObjects)
            {
                obj.gameObject.SetActive(true);
            }
            
        }
        
        #endregion
        
        #region 내부 메서드
        
        private InteractableObject FindInteractableObject(string objectId)
        {
            if (string.IsNullOrEmpty(objectId)) return null;
            if (idToInteractable.TryGetValue(objectId, out var found)) return found;
            return interactableObjects.Find(obj => obj.objectId == objectId);
        }

        /// <summary>
        /// ID로 상호작용 대상 조회를 시도합니다.
        /// </summary>
        public bool TryGet(string objectId, out InteractableObject interactable)
        {
            return idToInteractable.TryGetValue(objectId, out interactable);
        }

        /// <summary>
        /// ID로 GameObject를 가져옵니다(없으면 null).
        /// </summary>
        public GameObject GetObject(string objectId)
        {
            return TryGet(objectId, out var io) ? io.gameObject : null;
        }
        
        // 클릭 이벤트 라우팅은 ClickHandler + ClickToEventRouter에서 전담합니다.
        
        #endregion
        
        #region 유틸리티
        
        /// <summary>
        /// 현재 상호작용 가능한 오브젝트 목록을 반환합니다.
        /// </summary>
        /// <returns>상호작용 가능한 오브젝트 목록</returns>
        public List<string> GetCurrentInteractableObjects()
        {
            var result = new List<string>();
            foreach (var obj in interactableObjects)
            {
                if (obj.isInteractable && obj.gameObject.activeInHierarchy)
                {
                    result.Add(obj.objectId);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 특정 오브젝트가 상호작용 가능한지 확인합니다.
        /// </summary>
        /// <param name="objectId">확인할 오브젝트 ID</param>
        /// <returns>상호작용 가능 여부</returns>
        public bool IsObjectInteractable(string objectId)
        {
            var obj = FindInteractableObject(objectId);
            return obj != null && obj.isInteractable && obj.gameObject.activeInHierarchy;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 상호작용 가능한 오브젝트의 정보를 담는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class InteractableObject
    {
        public string objectId;
        public GameObject gameObject;
        public ClickHandler clickHandler;
        public bool isInteractable;
    }
}
