using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AFKS.Core.Services;
using AFKS.Core.Services.GameState;
using AFKS.Core.Services.StageBackground;
using System;

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
            Sprite sprite = null;

            // 우선 최근 보고된 배경 사용
            if (ServiceLocator.TryGet<IStageBackgroundService>(out var bgSvc))
            {
                if (bgSvc.LastSprite != null) 
                {
                    sprite = bgSvc.LastSprite;
                }
            }

            // 없으면 기본 메뉴 배경 사용
            if (sprite == null && defaultMenuBackground != null)
            {
                sprite = defaultMenuBackground;
            }

            // 기본 메뉴 배경도 없으면 Placeholder 사용(런타임 Resources.Load 제거: 인스펙터로 주입)
            // Addressables 또는 ScriptableObject 레지스트리를 사용하는 방식으로 전환할 수 있습니다.

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
            }
            else
            {
                Debug.LogError("targetImage가 null입니다!");
            }
        }
    }
}


