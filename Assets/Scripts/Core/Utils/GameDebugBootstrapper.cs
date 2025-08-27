using UnityEngine;

namespace AFKS.Core.Utils
{
	/// <summary>
	/// 씬 진입 시 GameDebug 카테고리 토글을 적용하는 부트스트래퍼.
	/// </summary>
	[AddComponentMenu("AFKS/Utils/Game Debug Bootstrapper")]
	public sealed class GameDebugBootstrapper : MonoBehaviour
	{
		[Header("Global")]
		[SerializeField] private bool enableGlobal = true;

		[Header("Categories")]
		[SerializeField] private bool enableCore = false;
		[SerializeField] private bool enableStage = false;
		[SerializeField] private bool enableEvent = true;
		[SerializeField] private bool enableInteraction = false;
		[SerializeField] private bool enableCamera = false;
		[SerializeField] private bool enableSave = false;

		private void Awake()
		{
			Apply();
		}

		private void OnValidate()
		{
			// 에디터에서 값 변경 시 즉시 반영(플레이 중)
			if (Application.isPlaying)
			{
				Apply();
			}
		}

		public void Apply()
		{
			GameDebug.SetGlobal(enableGlobal);
			GameDebug.Set(GameDebug.Category.Core, enableCore);
			GameDebug.Set(GameDebug.Category.Stage, enableStage);
			GameDebug.Set(GameDebug.Category.Event, enableEvent);
			GameDebug.Set(GameDebug.Category.Interaction, enableInteraction);
			GameDebug.Set(GameDebug.Category.Camera, enableCamera);
			GameDebug.Set(GameDebug.Category.Save, enableSave);
		}
	}
}


