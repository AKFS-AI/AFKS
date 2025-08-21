using System;
using System.Threading.Tasks;
using AFKS.Core.Events;
using AFKS.Core.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AFKS.Core.Services
{
	/// <summary>
	/// 애디티브 스테이지 로드/언로드와 간단 페이드(즉시 완료)를 제공하는 최소 구현.
	/// </summary>
	public sealed class SceneService : MonoBehaviour, ISceneService
	{
		[SerializeField] private float defaultFadeSeconds = 0.6f;

		private string activeStageId;
		private AsyncOperation preloadOperation;

		private void OnEnable()
		{
			ServiceLocator.Register<ISceneService>(this);
			GameEvents.StageChangeRequested += OnStageChangeRequested;
		}

		private void OnDisable()
		{
			GameEvents.StageChangeRequested -= OnStageChangeRequested;
			if (ServiceLocator.TryGet<ISceneService>(out var current) && ReferenceEquals(current, this))
			{
				ServiceLocator.Register<ISceneService>(null);
			}
		}

		private async void OnStageChangeRequested(string nextStageId)
		{
			try
			{
				await FadeOutAsync(defaultFadeSeconds);
				await LoadStageAdditiveAsync(nextStageId, preload: false);
				await ActivateLoadedStageAsync(nextStageId);
				if (!string.IsNullOrEmpty(activeStageId))
				{
					await UnloadStageAsync(activeStageId);
				}
				activeStageId = nextStageId;
				GameEvents.RaiseStageLoaded(activeStageId);
				await FadeInAsync(defaultFadeSeconds);
			}
			catch (Exception ex)
			{
				Log.Error($"Stage change failed: {ex.Message}");
				// 실패 시 롤백: 이전 스테이지 유지(아무 것도 하지 않음)
			}
		}

		public Task FadeOutAsync(float seconds)
		{
			// TODO: 페이드 캔버스 구현 전까지 즉시 완료
			return Task.CompletedTask;
		}

		public Task FadeInAsync(float seconds)
		{
			// TODO: 페이드 캔버스 구현 전까지 즉시 완료
			return Task.CompletedTask;
		}

		public async Task LoadStageAdditiveAsync(string stageId, bool preload = false)
		{
			preloadOperation = SceneManager.LoadSceneAsync(stageId, LoadSceneMode.Additive);
			if (preloadOperation == null)
			{
				throw new InvalidOperationException($"Failed to start loading scene: {stageId}");
			}
			preloadOperation.allowSceneActivation = !preload;
			if (!preload)
			{
				while (!preloadOperation.isDone)
				{
					await Task.Yield();
				}
			}
		}

		public async Task ActivateLoadedStageAsync(string stageId)
		{
			if (preloadOperation == null)
			{
				return;
			}
			preloadOperation.allowSceneActivation = true;
			while (!preloadOperation.isDone)
			{
				await Task.Yield();
			}
			preloadOperation = null;
		}

		public async Task UnloadStageAsync(string stageId)
		{
			if (!SceneManager.GetSceneByName(stageId).isLoaded)
			{
				return;
			}
			var op = SceneManager.UnloadSceneAsync(stageId);
			if (op == null) return;
			while (!op.isDone)
			{
				await Task.Yield();
			}
			GameEvents.RaiseStageUnloaded(stageId);
		}
	}
}
