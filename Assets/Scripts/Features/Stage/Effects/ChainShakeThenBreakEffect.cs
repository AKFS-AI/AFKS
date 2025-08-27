using UnityEngine;
using AFKS.Core.Utils;

namespace AFKS.Features.Stage.Effects
{
	[CreateAssetMenu(fileName = "ChainShakeThenBreakEffect", menuName = "AFKS/Stage/Effects/Chain Shake Then Break")] 
	public sealed class ChainShakeThenBreakEffect : StageEffect
	{
		[SerializeField] private string[] chainObjectIds = new string[] { "ChainTop", "ChainBottom" };
		[SerializeField] private bool playOnBoth = true;
		[SerializeField] private float shakeDuration = 1.0f;
		[SerializeField] private float shakeIntensity = 0.1f;
		[SerializeField] private float breakDelay = 0.5f;

		public override void Apply(AFKS.Features.Stage.StageEventSystem system)
		{
			var sim = AFKS.Features.Stage.StageInteractionManager.Instance;
			if (sim == null) return;
			if (chainObjectIds == null || chainObjectIds.Length == 0) return;
			
			for (int i = 0; i < chainObjectIds.Length; i++)
			{
				var obj = sim.GetObject(chainObjectIds[i]);
				if (obj == null) continue;
				
				// 체인 흔들림 애니메이션 시작
				system.StartCoroutine(ShakeThenBreakCoroutine(obj));
				
				if (!playOnBoth) break;
			}
		}
		
		private System.Collections.IEnumerator ShakeThenBreakCoroutine(GameObject chainObj)
		{
			var originalPosition = chainObj.transform.localPosition;
			var startTime = Time.time;
			
			GameDebug.Info(GameDebug.Category.Event, $"체인 {chainObj.name} 흔들림 시작 - {shakeDuration}초 동안");
			
			// 흔들림 애니메이션
			while (Time.time - startTime < shakeDuration)
			{
				var offset = new Vector3(
					Random.Range(-shakeIntensity, shakeIntensity),
					Random.Range(-shakeIntensity, shakeIntensity),
					0f
				);
				chainObj.transform.localPosition = originalPosition + offset;
				
				// 색상 변화로 흔들림 강조
				var spriteRenderer = chainObj.GetComponent<SpriteRenderer>();
				if (spriteRenderer != null)
				{
					var alpha = 0.5f + 0.5f * Mathf.Sin((Time.time - startTime) * 20f);
					spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
				}
				
				yield return new WaitForSeconds(0.05f); // 더 빠른 업데이트
			}
			
			// 원래 위치와 색상으로 복원
			chainObj.transform.localPosition = originalPosition;
			var finalSpriteRenderer = chainObj.GetComponent<SpriteRenderer>();
			if (finalSpriteRenderer != null)
			{
				finalSpriteRenderer.color = Color.white;
			}
			
			GameDebug.Info(GameDebug.Category.Event, $"체인 {chainObj.name} 흔들림 완료, {breakDelay}초 후 끊어짐");
			
			// 끊어짐 애니메이션 대기
			yield return new WaitForSeconds(breakDelay);
			
			// 체인 비활성화 (끊어진 효과)
			chainObj.SetActive(false);
			
			GameDebug.Info(GameDebug.Category.Event, $"체인 {chainObj.name} 흔들림 및 끊어짐 애니메이션 완료");
		}
	}
}


