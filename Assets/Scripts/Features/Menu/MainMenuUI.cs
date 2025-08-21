using AFKS.Core.Events;
using AFKS.Core.Services;
using AFKS.Core.Utils;
using AFKS.Features.Stage;
using UnityEngine;

namespace AFKS.Features.Menu
{
	/// <summary>
	/// 메인 메뉴 버튼 콜백. 버튼 OnClick에 각 메서드를 바인딩한다.
	/// </summary>
	public sealed class MainMenuUI : MonoBehaviour
	{
		[SerializeField] private string defaultStartStageId = StageIds.Stage_Lobby;

		public void OnClickNewGame()
		{
			if (ServiceLocator.TryGet<ISaveService>(out var save))
			{
				save.CurrentStageId = defaultStartStageId;
				save.SaveAll();
			}
			Log.Info($"New Game → {defaultStartStageId}");
			GameEvents.RaiseStageChangeRequested(defaultStartStageId);
		}

		public void OnClickContinue()
		{
			string target = defaultStartStageId;
			if (ServiceLocator.TryGet<ISaveService>(out var save))
			{
				if (save.TryLoadAll() && !string.IsNullOrEmpty(save.CurrentStageId))
				{
					target = save.CurrentStageId;
				}
			}
			Log.Info($"Continue → {target}");
			GameEvents.RaiseStageChangeRequested(target);
		}

		public void OnClickSettings()
		{
			Log.Info("Open Settings Panel");
			// SettingsPanel 열기 로직은 Core UI에서 처리(모달)
		}

		public void OnClickQuit()
		{
			Log.Info("Quit Game");
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
	}
}
