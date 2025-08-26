using UnityEngine;

namespace AFKS.Core.Data
{
	[CreateAssetMenu(menuName = "AFKS/Project Config")]
	public sealed class ProjectConfig : ScriptableObject
	{
		public StageDefinition[] stages;
		public AudioDatabase audio;
	}
}


