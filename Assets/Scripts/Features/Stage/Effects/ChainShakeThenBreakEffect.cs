using UnityEngine;
using AFKS.Core.Utils;

namespace AFKS.Features.Stage.Effects
{
	[CreateAssetMenu(fileName = "ChainShakeThenBreakEffect", menuName = "AFKS/Stage/Effects/Chain Shake Then Break")] 
	public sealed class ChainShakeThenBreakEffect : StageEffect
	{
		[SerializeField] private string[] chainObjectIds = new string[] { "ChainTop", "ChainBottom" };
		[SerializeField] private bool playOnBoth = true;

		public override void Apply(AFKS.Features.Stage.StageEventSystem system)
		{
			if (system == null) return;
			if (chainObjectIds == null || chainObjectIds.Length == 0) return;
			
			for (int i = 0; i < chainObjectIds.Length; i++)
			{
				var obj = system.GetObject(chainObjectIds[i]);
				if (obj == null) continue;
				
				// 실제 체인 분리 효과 실행
				var breakEffect = obj.GetComponent<AFKS.Features.Stage.Effects.ChainBreakEffect>();
				if (breakEffect == null)
				{
					breakEffect = obj.AddComponent<AFKS.Features.Stage.Effects.ChainBreakEffect>();
				}
				breakEffect.StartBreak();
				
				if (!playOnBoth) break;
			}
		}
		

	}
}


