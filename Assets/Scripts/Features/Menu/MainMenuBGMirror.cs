using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AFKS.Core.Services;
using AFKS.Core.Services.GameState;
using AFKS.Core.Services.StageBackground;

namespace AFKS.Features.Menu
{
    /// <summary>
    /// 메뉴 배경을 마지막 플레이 스테이지의 배경 스프라이트로 미러링합니다.
    /// SaveService/게임 상태가 없을 때는 기본 메뉴 배경을 사용합니다.
    /// </summary>
    [AddComponentMenu("AFKS/Menu/MainMenu BG Mirror")]
    public sealed class MainMenuBGMirror : MonoBehaviour
    {
        [SerializeField]
        [InspectorName("배경 이미지")]
        private Image targetImage;

        [SerializeField]
        [InspectorName("기본 메뉴 배경")]
        private Sprite defaultMenuBackground;

        private void Awake()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
        }

        private void Start()
        {
            TryApplyBackground();
        }

        private void TryApplyBackground()
        {
            Debug.Log("MainMenuBGMirror: 배경 적용 시작");
            
            Sprite sprite = null;

            // 우선 최근 보고된 배경 사용
            if (ServiceLocator.TryGet<IStageBackgroundService>(out var bgSvc))
            {
                Debug.Log("StageBackgroundService 발견됨");
                if (bgSvc.LastSprite != null) 
                {
                    sprite = bgSvc.LastSprite;
                    Debug.Log($"최근 배경 스프라이트 사용: {sprite.name}");
                }
                else
                {
                    Debug.Log("LastSprite가 null입니다");
                }
            }
            else
            {
                Debug.Log("StageBackgroundService를 찾을 수 없습니다");
            }

            // 없으면 기본 메뉴 배경 사용
            if (sprite == null && defaultMenuBackground != null)
            {
                sprite = defaultMenuBackground;
                Debug.Log($"기본 메뉴 배경 사용: {sprite.name}");
            }
            else if (sprite == null)
            {
                Debug.Log("기본 메뉴 배경이 null입니다");
            }

            // 기본 메뉴 배경도 없으면 Placeholder 사용
            if (sprite == null)
            {
                Debug.Log("Placeholder 배경 로드 시도");
                var tex = Resources.Load<Sprite>("Placeholders/BG_Default");
                sprite = tex;
                if (sprite != null)
                {
                    Debug.Log($"Placeholder 배경 로드됨: {sprite.name}");
                }
                else
                {
                    Debug.LogWarning("Placeholder 배경을 찾을 수 없습니다");
                }
            }

            if (targetImage != null)
            {
                targetImage.sprite = sprite;
                targetImage.color = sprite != null ? Color.white : new Color(0,0,0,0.65f);
                targetImage.preserveAspect = true;
                var rt = targetImage.rectTransform; 
                rt.anchorMin = Vector2.zero; 
                rt.anchorMax = Vector2.one; 
                rt.offsetMin = Vector2.zero; 
                rt.offsetMax = Vector2.zero;
                
                Debug.Log($"배경 이미지 설정 완료: Sprite={sprite?.name ?? "null"}, Color={targetImage.color}");
            }
            else
            {
                Debug.LogError("targetImage가 null입니다!");
            }
            
            Debug.Log("MainMenuBGMirror: 배경 적용 완료");
        }
    }
}


