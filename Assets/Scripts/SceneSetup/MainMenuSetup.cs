using UnityEngine;
using UnityEngine.UI;
using AFKS.Core;

namespace AFKS.SceneSetup
{
    /// <summary>
    /// MainMenu 씬 자동 설정 도구
    /// </summary>
    public class MainMenuSetup : MonoBehaviour
    {
        [Header("🎯 Core Manager 프리팹들")]
        [SerializeField, Tooltip("GameManager 프리팹")] private GameObject gameManagerPrefab;
        [SerializeField, Tooltip("SceneController 프리팹")] private GameObject sceneControllerPrefab;
        [SerializeField, Tooltip("AudioManager 프리팹")] private GameObject audioManagerPrefab;
        [SerializeField, Tooltip("UIManager 프리팹")] private GameObject uiManagerPrefab;

        [Header("🎨 UI 설정")]
        [SerializeField, Tooltip("메인 메뉴 배경 이미지")] private Sprite backgroundSprite;
        [SerializeField, Tooltip("메인 메뉴 BGM")] private AudioClip menuBGM;

        [Header("🔘 버튼 이벤트")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            SetupCoreManagers();
            SetupUI();
            SetupButtonEvents();
            PlayMenuBGM();
        }

        /// <summary>
        /// 핵심 매니저들 자동 생성
        /// </summary>
        private void SetupCoreManagers()
        {
            // CoreManagers 부모 오브젝트 생성
            GameObject coreManagers = new GameObject("CoreManagers");
            
            // 각 매니저 인스턴스 생성
            if (gameManagerPrefab != null)
            {
                GameObject gameManager = Instantiate(gameManagerPrefab, coreManagers.transform);
                gameManager.name = "GameManager";
            }
            
            if (sceneControllerPrefab != null)
            {
                GameObject sceneController = Instantiate(sceneControllerPrefab, coreManagers.transform);
                sceneController.name = "SceneController";
            }
            
            if (audioManagerPrefab != null)
            {
                GameObject audioManager = Instantiate(audioManagerPrefab, coreManagers.transform);
                audioManager.name = "AudioManager";
            }
            
            if (uiManagerPrefab != null)
            {
                GameObject uiManager = Instantiate(uiManagerPrefab, coreManagers.transform);
                uiManager.name = "UIManager";
            }

            Debug.Log("[MainMenuSetup] 핵심 매니저들이 생성되었습니다.");
        }

        /// <summary>
        /// UI 자동 설정
        /// </summary>
        private void SetupUI()
        {
            // 배경 이미지 설정
            if (backgroundSprite != null)
            {
                Image backgroundImage = GameObject.Find("BackgroundImage")?.GetComponent<Image>();
                if (backgroundImage != null)
                {
                    backgroundImage.sprite = backgroundSprite;
                }
            }
        }

        /// <summary>
        /// 버튼 이벤트 자동 연결
        /// </summary>
        private void SetupButtonEvents()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(() => {
                    SceneController.Instance?.LoadGameScene();
                });
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() => {
                    SceneController.Instance?.QuitGame();
                });
            }

            Debug.Log("[MainMenuSetup] 버튼 이벤트가 연결되었습니다.");
        }

        /// <summary>
        /// 메뉴 BGM 재생
        /// </summary>
        private void PlayMenuBGM()
        {
            if (menuBGM != null)
            {
                AudioSource audioSource = GameObject.Find("MenuAudioSource")?.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.clip = menuBGM;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
    }
}