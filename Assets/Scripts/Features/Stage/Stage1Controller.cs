using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.StageBackground;
using AFKS.Core.Events;
using AFKS.Features.Stage;

namespace AFKS.Features.Stage
{
    /// <summary>
    /// Stage1의 배경 전환과 체인 해금 상태를 관리합니다.
    /// 체인이 해금되면 배경을 1_2로 교체하고 문을 활성화합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Stage/Stage1 Controller")]
    public sealed class Stage1Controller : MonoBehaviour
    {
        [Header("배경 설정")]
        [SerializeField]
        [InspectorName("기본 배경 (1_1)")] private Sprite defaultBackground;
        
        [SerializeField]
        [InspectorName("해금 배경 (1_2)")] private Sprite unlockedBackground;
        
        [Header("상호작용 오브젝트")]
        [SerializeField]
        [InspectorName("체인 오브젝트")] private GameObject chainObject;
        
        [SerializeField]
        [InspectorName("문 핫스팟")] private GameObject doorHotspot;
        
        [Header("설정")]
        [SerializeField]
        [InspectorName("해금 시 배경 교체")] private bool changeBackgroundOnUnlock = true;
        
        [SerializeField]
        [InspectorName("체인 제거")] private bool removeChainOnUnlock = true;

        private SpriteRenderer backgroundRenderer;
        private const string SAVE_KEY_CHAIN_UNLOCKED = "Stage1.ChainUnlocked";

        #region Unity 수명주기

        private void Awake()
        {
            backgroundRenderer = GetComponent<SpriteRenderer>();
            if (backgroundRenderer == null)
            {
                Debug.LogError("Stage1Controller: SpriteRenderer가 없습니다.");
                return;
            }
        }

        private void Start()
        {
            Debug.Log("Stage1Controller Start 시작");
            InitializeStage();
            Debug.Log("Stage1Controller Start 완료");
        }
        
        // ESC 키는 이제 Core의 GlobalSettingsManager에서 전역으로 처리됩니다.
        // 설정창을 열고 닫거나 메뉴로 돌아가는 기능을 제공합니다.

        #endregion

        #region 공개 API

        /// <summary>
        /// 스테이지 초기화 - 저장된 상태에 따라 배경과 체인 상태를 설정합니다.
        /// </summary>
        public void InitializeStage()
        {
            Debug.Log("Stage1Controller InitializeStage 시작");
            
            bool isChainUnlocked = LoadChainUnlockedState();
            Debug.Log($"체인 잠금 해제 상태: {isChainUnlocked}");
            
            if (isChainUnlocked)
            {
                Debug.Log("체인이 해금된 상태 - UnlockStage 호출");
                UnlockStage();
            }
            else
            {
                Debug.Log("체인이 잠긴 상태 - LockStage 호출");
                LockStage();
            }
            
            // StageBackgroundService에 현재 배경 보고
            Debug.Log("현재 배경을 StageBackgroundService에 보고");
            ReportCurrentBackground();
            
            Debug.Log("Stage1Controller InitializeStage 완료");
        }

        /// <summary>
        /// 체인을 해금하고 스테이지를 열림 상태로 만듭니다.
        /// </summary>
        public void UnlockStage()
        {
            // 체인 제거
            if (removeChainOnUnlock && chainObject != null)
            {
                chainObject.SetActive(false);
            }
            
            // 배경 교체
            if (changeBackgroundOnUnlock && unlockedBackground != null)
            {
                backgroundRenderer.sprite = unlockedBackground;
            }
            
            // 문 활성화
            if (doorHotspot != null)
            {
                doorHotspot.SetActive(true);
            }
            
            // 상태 저장
            SaveChainUnlockedState(true);
            
            // StageBackgroundService에 변경된 배경 보고
            ReportCurrentBackground();
            
            Debug.Log("Stage1: 체인이 해금되었습니다.");
        }

        /// <summary>
        /// 스테이지를 잠금 상태로 만듭니다.
        /// </summary>
        public void LockStage()
        {
            // 기본 배경으로 복원
            if (defaultBackground != null)
            {
                backgroundRenderer.sprite = defaultBackground;
            }
            
            // 체인 표시
            if (chainObject != null)
            {
                chainObject.SetActive(true);
            }
            
            // 문 비활성화
            if (doorHotspot != null)
            {
                doorHotspot.SetActive(false);
            }
            
            // 상태 저장
            SaveChainUnlockedState(false);
            
            Debug.Log("Stage1: 스테이지가 잠금 상태입니다.");
        }

        #endregion

        #region 내부 메서드

        private bool LoadChainUnlockedState()
        {
            if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var saveService))
            {
                return saveService.GetBool(SAVE_KEY_CHAIN_UNLOCKED, false);
            }
            
            // SaveService가 없으면 PlayerPrefs 사용
            return PlayerPrefs.GetInt(SAVE_KEY_CHAIN_UNLOCKED, 0) == 1;
        }

        private void SaveChainUnlockedState(bool unlocked)
        {
            if (ServiceLocator.TryGet<AFKS.Core.Services.Save.ISaveService>(out var saveService))
            {
                saveService.SetBool(SAVE_KEY_CHAIN_UNLOCKED, unlocked);
            }
            else
            {
                // SaveService가 없으면 PlayerPrefs 사용
                PlayerPrefs.SetInt(SAVE_KEY_CHAIN_UNLOCKED, unlocked ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void ReportCurrentBackground()
        {
            Debug.Log("ReportCurrentBackground 시작");
            
            if (backgroundRenderer == null)
            {
                Debug.LogError("backgroundRenderer가 null입니다!");
                return;
            }
            
            if (backgroundRenderer.sprite == null)
            {
                Debug.LogWarning("backgroundRenderer.sprite가 null입니다!");
            }
            else
            {
                Debug.Log($"현재 배경 스프라이트: {backgroundRenderer.sprite.name}");
            }
            
            if (ServiceLocator.TryGet<AFKS.Core.Services.StageBackground.IStageBackgroundService>(out var bgService))
            {
                Debug.Log("StageBackgroundService 발견됨 - 배경 보고");
                bgService.ReportBackground("Stage1", backgroundRenderer.sprite);
            }
            else
            {
                Debug.LogWarning("StageBackgroundService를 찾을 수 없습니다");
            }
            
            Debug.Log("ReportCurrentBackground 완료");
        }

        #endregion

        #region 인스펙터 검증

        private void OnValidate()
        {
            // 기본 배경이 설정되지 않은 경우 자동으로 찾기
            if (defaultBackground == null)
            {
                var spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    defaultBackground = spriteRenderer.sprite;
                }
            }
        }

        #endregion
    }
}
