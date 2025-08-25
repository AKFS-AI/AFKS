using UnityEngine;
using AFKS.Core.Services;
using AFKS.Core.Services.Input;

namespace AFKS.Features.Interaction
{
	/// <summary>
	/// 활성화 동안 IInputService를 잠가 월드 오브젝트 클릭을 차단합니다.
	/// UI는 EventSystem으로 동작하므로 영향을 받지 않습니다.
	/// </summary>
	[AddComponentMenu("AFKS/Interaction/Input Lock While Active")]
	public sealed class InputLockWhileActive : MonoBehaviour
	{
		private IInputService inputService;

		private void OnEnable()
		{
			ServiceLocator.TryGet<IInputService>(out inputService);
			if (inputService != null) inputService.Lock(true);
		}

		private void OnDisable()
		{
			if (inputService != null) inputService.Lock(false);
			inputService = null;
		}
	}
}


