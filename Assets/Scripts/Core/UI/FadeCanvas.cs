using System.Collections;
using UnityEngine;

namespace AFKS.Core.UI
{
    /// <summary>
    /// 전환 시 CanvasGroup 알파를 변경하여 페이드인을/아웃을 수행하고,
    /// 페이드 중에는 Raycast 차단으로 입력을 막습니다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("AFKS/UI/Fade Canvas")]
    public sealed class FadeCanvas : MonoBehaviour
    {
        #region 필드
        [SerializeField]
        [InspectorName("캔버스 그룹")]
        [Tooltip("페이드에 사용할 CanvasGroup 컴포넌트입니다.")]
        private CanvasGroup canvasGroup;

        private Coroutine currentRoutine;
        #endregion

        #region 유니티 수명주기
        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        #endregion

        #region 코루틴
        public IEnumerator FadeOut(float seconds)
        {
            yield return FadeTo(1f, seconds);
        }

        public IEnumerator FadeIn(float seconds)
        {
            yield return FadeTo(0f, seconds);
        }

        public void StopFade()
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                currentRoutine = null;
            }
        }

        private IEnumerator FadeTo(float target, float seconds)
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = StartCoroutine(FadeRoutine(target, seconds));
            yield return currentRoutine;
        }

        private IEnumerator FadeRoutine(float target, float seconds)
        {
            float start = canvasGroup.alpha;
            float t = 0f;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float p = seconds > 0f ? Mathf.Clamp01(t / seconds) : 1f;
                canvasGroup.alpha = Mathf.Lerp(start, target, p);
                yield return null;
            }

            canvasGroup.alpha = target;
            bool block = target > 0.99f;
            canvasGroup.blocksRaycasts = block;
            canvasGroup.interactable = block;
            currentRoutine = null;
        }
        #endregion
    }
}


