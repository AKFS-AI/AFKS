using UnityEngine;
using UnityEngine.UI;
using AFKS.Core.Events;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;
using AFKS.Core.Services.Save;
using AFKS.Core.Services.Audio;

namespace AFKS.Core.UI
{
	/// <summary>
	/// ESC 메뉴(일시정지/옵션/저장하고 나가기)를 제어합니다.
	/// </summary>
	[AddComponentMenu("AFKS/UI/Pause Menu Controller")]
	public sealed class PauseMenuController : MonoBehaviour
	{
		[SerializeField] private CanvasGroup root;
		[SerializeField] private Button resumeButton;
		[SerializeField] private Button saveQuitButton;
		[SerializeField] private Button quitButton;
		[SerializeField] private Slider masterSlider;
		[SerializeField] private Slider bgmSlider;
		[SerializeField] private Slider sfxSlider;
		[SerializeField] private KeyCode toggleKey = KeyCode.Escape;

		private IInputService input;
		private ISaveService save;
		private IAudioService audioSvc;
		private bool isOpen;

		private void Awake()
		{
			if (root == null) root = GetComponent<CanvasGroup>();
			ServiceLocator.TryGet<IInputService>(out input);
			ServiceLocator.TryGet<ISaveService>(out save);
			ServiceLocator.TryGet<IAudioService>(out audioSvc);
			BindUI();
			HideImmediate();
		}

		private void Update()
		{
			if (UnityEngine.Input.GetKeyDown(toggleKey)) Toggle();
		}

		private void BindUI()
		{
			if (resumeButton != null) resumeButton.onClick.AddListener(() => Toggle(false));
			if (saveQuitButton != null) saveQuitButton.onClick.AddListener(SaveAndQuitToMenu);
			if (quitButton != null) quitButton.onClick.AddListener(Application.Quit);
			if (masterSlider != null) masterSlider.onValueChanged.AddListener(v => OnVolumeChanged());
			if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(v => OnVolumeChanged());
			if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(v => OnVolumeChanged());
		}

		public void Toggle() => Toggle(!isOpen);

		public void Toggle(bool open)
		{
			isOpen = open;
			if (open) Show(); else Hide();
		}

		private void Show()
		{
			Time.timeScale = 0f;
			if (root != null) { root.alpha = 1f; root.blocksRaycasts = true; root.interactable = true; }
			GameEvents.RaisePauseToggled(true);
		}

		private void Hide()
		{
			Time.timeScale = 1f;
			if (root != null) { root.alpha = 0f; root.blocksRaycasts = false; root.interactable = false; }
			GameEvents.RaisePauseToggled(false);
		}

		private void HideImmediate()
		{
			if (root != null) { root.alpha = 0f; root.blocksRaycasts = false; root.interactable = false; }
		}

		private void SaveAndQuitToMenu()
		{
			if (save != null) save.SaveAll();
			GameEvents.RaiseStageChangeRequested(AFKS.Features.Stage.StageIds.Menu);
		}

		private void OnVolumeChanged()
		{
			if (audioSvc == null || save == null) return;
			var master = masterSlider != null ? masterSlider.value : 1f;
			var bgm = bgmSlider != null ? bgmSlider.value : 1f;
			var sfx = sfxSlider != null ? sfxSlider.value : 1f;
			if (save is SaveService svc) svc.SetVolumes(master, bgm, sfx);
		}
	}
}


