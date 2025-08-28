using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace AFKS.Features.Menu
{
    /// <summary>
    /// 메뉴에서 스테이지 진행도에 따라 배경을 동적으로 전환하고 카메라 무빙을 구현합니다.
    /// </summary>
    public class DynamicMenuBackground : MonoBehaviour
    {
        [Header("배경 설정")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private AFKS.Features.Stage.StoryProgress storyProgress;
        
        [Header("전환 설정")]
        [SerializeField] private float transitionDuration = 2.0f;
        [SerializeField] private float backgroundChangeInterval = 15.0f;
        
        [Header("카메라 무빙")]
        [SerializeField] private bool enableCameraMovement = true;
        [SerializeField] private float cameraMovementSpeed = 0.5f;
        [SerializeField] private float cameraMovementRange = 0.1f;
        
        [Header("배경 이미지 경로")]
        [SerializeField] private string[] stageBackgroundPaths = {
            "Images/Backgrounds/Stage1/Stage1_1",
            "Images/Backgrounds/Stage2/Stage2",
            "Images/Backgrounds/Stage3/Stage3",
            "Images/Backgrounds/Stage4/Stage4",
            "Images/Backgrounds/Stage5/Stage5_1",
            "Images/Backgrounds/Stage6/Stage6"
        };
        
        private List<Sprite> availableBackgrounds = new List<Sprite>();
        private int currentBackgroundIndex = 0;
        private Camera menuCamera;
        private Vector3 originalCameraPosition;
        private Coroutine backgroundChangeCoroutine;
        private Coroutine cameraMovementCoroutine;
        
        private void Start()
        {
            // 즉시 초기화 (프레임 대기 없이)
            InitializeBackgrounds();
            StartBackgroundRotation();
            if (enableCameraMovement)
            {
                StartCameraMovement();
            }
            
            Debug.Log($"[DynamicMenuBackground] 초기화 완료 - {availableBackgrounds.Count}개 배경 로드됨");
        }
        
        private void InitializeBackgrounds()
        {
            availableBackgrounds.Clear();
            
            // Stage1은 항상 포함
            var stage1Bg = Resources.Load<Sprite>(stageBackgroundPaths[0]);
            if (stage1Bg != null)
            {
                availableBackgrounds.Add(stage1Bg);
                Debug.Log($"[DynamicMenuBackground] Stage1 배경 로드됨: {stage1Bg.name}");
                
                // 즉시 첫 번째 배경 적용
                if (backgroundImage != null)
                {
                    backgroundImage.sprite = stage1Bg;
                    backgroundImage.color = Color.white;
                    Debug.Log($"[DynamicMenuBackground] 초기 배경 즉시 적용: {stage1Bg.name}");
                }
                else
                {
                    Debug.LogError($"[DynamicMenuBackground] backgroundImage이 null입니다!");
                }
            }
            
            // 스토리 진행도에 따라 추가 배경 로드
            if (storyProgress != null)
            {
                var completedStages = storyProgress.GetCompletedStages();
                
                for (int i = 1; i < stageBackgroundPaths.Length; i++)
                {
                    if (completedStages.Contains($"Stage{i + 1}"))
                    {
                        var bg = Resources.Load<Sprite>(stageBackgroundPaths[i]);
                        if (bg != null)
                        {
                            availableBackgrounds.Add(bg);
                            Debug.Log($"[DynamicMenuBackground] Stage{i + 1} 배경 로드됨");
                        }
                    }
                }
            }
            
            // 최소 1개는 있어야 함
            if (availableBackgrounds.Count == 0)
            {
                var fallbackBg = Resources.Load<Sprite>(stageBackgroundPaths[0]);
                if (fallbackBg != null)
                {
                    availableBackgrounds.Add(fallbackBg);
                    Debug.Log($"[DynamicMenuBackground] 폴백 배경 로드됨");
                }
            }
            
            Debug.Log($"[DynamicMenuBackground] 총 {availableBackgrounds.Count}개 배경 로드 완료");
        }
        
        private void StartBackgroundRotation()
        {
            if (availableBackgrounds.Count > 1)
            {
                backgroundChangeCoroutine = StartCoroutine(BackgroundRotationCoroutine());
            }
        }
        
        private IEnumerator BackgroundRotationCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(backgroundChangeInterval);
                
                if (availableBackgrounds.Count > 1)
                {
                    // 다음 배경으로 전환
                    currentBackgroundIndex = (currentBackgroundIndex + 1) % availableBackgrounds.Count;
                    StartCoroutine(TransitionToBackground(availableBackgrounds[currentBackgroundIndex]));
                }
            }
        }
        
        private IEnumerator TransitionToBackground(Sprite newBackground)
        {
            if (backgroundImage == null) yield break;
            
            float elapsed = 0f;
            Color startColor = backgroundImage.color;
            Color targetColor = new Color(1f, 1f, 1f, 1f);
            
            // 페이드 아웃
            while (elapsed < transitionDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (transitionDuration / 2f);
                backgroundImage.color = Color.Lerp(startColor, new Color(1f, 1f, 1f, 0f), progress);
                yield return null;
            }
            
            // 배경 변경
            backgroundImage.sprite = newBackground;
            
            // 페이드 인
            elapsed = 0f;
            startColor = backgroundImage.color;
            while (elapsed < transitionDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (transitionDuration / 2f);
                backgroundImage.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), targetColor, progress);
                yield return null;
            }
            
            backgroundImage.color = targetColor;
            Debug.Log($"[DynamicMenuBackground] 배경 전환 완료: {newBackground.name}");
        }
        
        private void StartCameraMovement()
        {
            menuCamera = Camera.main;
            if (menuCamera != null)
            {
                originalCameraPosition = menuCamera.transform.position;
                cameraMovementCoroutine = StartCoroutine(CameraMovementCoroutine());
            }
        }
        
        private IEnumerator CameraMovementCoroutine()
        {
            while (true)
            {
                float time = 0f;
                Vector3 startPos = originalCameraPosition;
                
                // 부드러운 카메라 움직임
                while (time < 1f)
                {
                    time += Time.deltaTime * cameraMovementSpeed;
                    float progress = Mathf.Sin(time * Mathf.PI * 2f) * 0.5f + 0.5f;
                    
                    Vector3 offset = new Vector3(
                        Mathf.Sin(time * 2f) * cameraMovementRange,
                        Mathf.Cos(time * 1.5f) * cameraMovementRange * 0.5f,
                        0f
                    );
                    
                    menuCamera.transform.position = startPos + offset;
                    yield return null;
                }
            }
        }
        
        private void OnDestroy()
        {
            if (backgroundChangeCoroutine != null)
            {
                StopCoroutine(backgroundChangeCoroutine);
            }
            if (cameraMovementCoroutine != null)
            {
                StopCoroutine(cameraMovementCoroutine);
            }
        }
        
        /// <summary>
        /// 수동으로 배경을 변경합니다.
        /// </summary>
        public void ChangeBackground(int stageIndex)
        {
            if (stageIndex >= 0 && stageIndex < availableBackgrounds.Count)
            {
                currentBackgroundIndex = stageIndex;
                StartCoroutine(TransitionToBackground(availableBackgrounds[currentBackgroundIndex]));
            }
        }
        
        /// <summary>
        /// 카메라 무빙을 토글합니다.
        /// </summary>
        public void ToggleCameraMovement()
        {
            enableCameraMovement = !enableCameraMovement;
            
            if (enableCameraMovement)
            {
                StartCameraMovement();
            }
            else if (cameraMovementCoroutine != null)
            {
                StopCoroutine(cameraMovementCoroutine);
                if (menuCamera != null)
                {
                    menuCamera.transform.position = originalCameraPosition;
                }
            }
        }
    }
}
