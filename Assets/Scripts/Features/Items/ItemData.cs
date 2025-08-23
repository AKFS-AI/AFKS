using UnityEngine;

namespace AFKS.Features.Items
{
	/// <summary>
	/// 인벤토리에 들어가는 아이템 데이터. 리소스/표시명을 포함.
	/// </summary>
	[CreateAssetMenu(menuName = "AFKS/Items/Item Data", fileName = "ItemData_", order = 0)]
	public sealed class ItemData : ScriptableObject
	{
		#region 필드
		[SerializeField]
		[InspectorName("아이템 ID")]
		[Tooltip("고유 아이템 ID(영문/스네이크/파스칼 등 일관). 예: Key_Bundle")] 
		private string itemId;

		[SerializeField]
		[InspectorName("표시 이름")]
		private string displayName;

		[SerializeField]
		[InspectorName("설명")]
		[TextArea]
		private string description;

		[SerializeField]
		[InspectorName("이미지")]
		private Sprite image;
		#endregion

		#region 공개 API
		public string ItemId => itemId;
		public string DisplayName => displayName;
		public string Description => description;
		public Sprite Image => image;
		#endregion
	}
}


