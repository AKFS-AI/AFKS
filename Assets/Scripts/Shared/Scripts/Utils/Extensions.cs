using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AFKS.Shared.Utils
{
    /// <summary>
    /// 유용한 확장 메서드들
    /// </summary>
    public static class Extensions
    {
        // === TRANSFORM EXTENSIONS ===
        
        /// <summary>
        /// Transform의 모든 자식을 삭제
        /// </summary>
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Object.Destroy(transform.GetChild(i).gameObject);
                else
                    Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        
        /// <summary>
        /// 특정 이름의 자식 찾기 (재귀)
        /// </summary>
        public static Transform FindChildRecursive(this Transform transform, string name)
        {
            Transform child = transform.Find(name);
            if (child != null) return child;
            
            for (int i = 0; i < transform.childCount; i++)
            {
                child = FindChildRecursive(transform.GetChild(i), name);
                if (child != null) return child;
            }
            
            return null;
        }
        
        // === CANVASGROUP EXTENSIONS ===
        
        /// <summary>
        /// CanvasGroup 페이드 인
        /// </summary>
        public static IEnumerator FadeIn(this CanvasGroup canvasGroup, float duration = 0.5f)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / duration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }
        
        /// <summary>
        /// CanvasGroup 페이드 아웃
        /// </summary>
        public static IEnumerator FadeOut(this CanvasGroup canvasGroup, float duration = 0.5f)
        {
            canvasGroup.interactable = false;
            
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.gameObject.SetActive(false);
        }
        
        // === VECTOR EXTENSIONS ===
        
        /// <summary>
        /// Vector3를 Vector2로 변환 (z 제거)
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.y);
        }
        
        /// <summary>
        /// Vector2를 Vector3로 변환 (z = 0)
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector2, float z = 0f)
        {
            return new Vector3(vector2.x, vector2.y, z);
        }
        
        // === COLLECTION EXTENSIONS ===
        
        /// <summary>
        /// 리스트에서 랜덤 요소 선택
        /// </summary>
        public static T GetRandomElement<T>(this List<T> list)
        {
            if (list == null || list.Count == 0) return default(T);
            return list[Random.Range(0, list.Count)];
        }
        
        /// <summary>
        /// 배열에서 랜덤 요소 선택
        /// </summary>
        public static T GetRandomElement<T>(this T[] array)
        {
            if (array == null || array.Length == 0) return default(T);
            return array[Random.Range(0, array.Length)];
        }
        
        // === STRING EXTENSIONS ===
        
        /// <summary>
        /// 문자열이 null이거나 비어있는지 확인
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        
        /// <summary>
        /// 문자열이 null이거나 공백인지 확인
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
    }
}