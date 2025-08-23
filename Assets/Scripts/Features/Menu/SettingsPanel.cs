using UnityEngine;

namespace AFKS.Features.Menu
{
	/// <summary>
	/// 메인 메뉴의 설정 패널을 표시/숨김하는 단순 컨트롤러입니다.
	/// 패널 루트를 지정하지 않으면 자기 자신을 루트로 사용합니다.
	/// </summary>
	[AddComponentMenu("AFKS/Menu/Settings Panel")]
	public sealed class SettingsPanel : MonoBehaviour
	{
		#region 필드
		[SerializeField]
		[InspectorName("패널 루트")]
		[Tooltip("표시/숨김을 제어할 루트 오브젝트. 비우면 자기 자신을 사용합니다.")]
		private GameObject panelRoot;

		[SerializeField]
		[InspectorName("초기 활성화")]
		[Tooltip("시작 시 패널을 활성화할지 여부입니다.")]
		private bool startActive = false;
		#endregion

		#region 유니티 수명주기
		private void Awake()
		{
			if (panelRoot == null)
			{
				panelRoot = gameObject;
			}
			panelRoot.SetActive(startActive);
		}
		#endregion

		#region 공개 API
		/// <summary>패널을 표시합니다.</summary>
		public void Show() => SetActive(true);

		/// <summary>패널을 숨깁니다.</summary>
		public void Hide() => SetActive(false);

		/// <summary>표시/숨김을 전환합니다.</summary>
		public void Toggle() => SetActive(!panelRoot.activeSelf);
		#endregion

		#region 내부 메서드
		private void SetActive(bool active)
		{
			if (panelRoot != null)
			{
				panelRoot.SetActive(active);
			}
		}
		#endregion
	}
}


