using UnityEngine;

namespace AFKS.Features.Stage.Effects
{
	[CreateAssetMenu(fileName = "EnableInteractableEffect", menuName = "AFKS/Stage/Effects/Enable Interactable")] 
	public sealed class EnableInteractableEffect : StageEffect
	{
		[SerializeField] private string objectId = "";
		[SerializeField] private bool interactable = true;

		public override void Apply(AFKS.Features.Stage.StageEventSystem system)
		{
			if (system == null || string.IsNullOrEmpty(objectId)) return;
			system.SetObjectInteractable(objectId, interactable);
		}
	}
}


