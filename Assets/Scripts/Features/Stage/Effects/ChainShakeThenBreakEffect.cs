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
			
			// 흔들림 애니메이션
			while (Time.time - startTime < shakeDuration)
			{
				var offset = new Vector3(
					Random.Range(-shakeIntensity, shakeIntensity),
					Random.Range(-shakeIntensity, shakeIntensity),
					0f
				);
				chainObj.transform.localPosition = originalPosition + offset;
				yield return null;
			}
			
			// 원래 위치로 복원
			chainObj.transform.localPosition = originalPosition;
			
			// 끊어짐 애니메이션 대기
			yield return new WaitForSeconds(breakDelay);
			
			// 체인 비활성화 (끊어진 효과)
			chainObj.SetActive(false);
			
			GameDebug.Info(GameDebug.Category.Event, $"체인 {chainObj.name} 흔들림 및 끊어짐 애니메이션 완료");
		}
	}
}


