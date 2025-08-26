using UnityEngine;
using AFKS.Core.Composition;

namespace AFKS.Core.Mono
{
	[DisallowMultipleComponent]
	public sealed class ProgressServiceComponent : MonoBehaviour
	{
		public Core.Services.ProgressService Service { get; private set; }
		[SerializeField] private int _currentStage;

		private void Awake()
		{
			if (GameRuntime.Instance == null)
			{
				var root = new GameObject("GameRuntime");
				root.AddComponent<GameRuntime>();
			}
			Service = GameRuntime.Instance.Progress;
		}

		private void Update()
		{
			if (Service != null) _currentStage = Service.CurrentStageIndex;
		}
	}
}


