using System;
using UnityEngine;

namespace AFKS.Core.Data
{
	[CreateAssetMenu(menuName = "AFKS/Audio Database")]
	public sealed class AudioDatabase : ScriptableObject
	{
		public Entry[] entries;

		[Serializable]
		public sealed class Entry
		{
			public string id;
			public AudioClip clip;
		}

		public AudioClip Find(string id)
		{
			if (string.IsNullOrEmpty(id) || entries == null) return null;
			for (int i = 0; i < entries.Length; i++)
			{
				if (entries[i] != null && entries[i].id == id) return entries[i].clip;
			}
			return null;
		}
	}
}


