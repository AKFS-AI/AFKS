using UnityEngine;

namespace AFKS.Core.Services.Save
{
    /// <summary>
    /// Unity JsonUtility 기반 직렬화기.
    /// </summary>
    public sealed class JsonUnitySaveSerializer : ISaveSerializer
    {
        public string Serialize(SaveModel model, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(model, prettyPrint);
        }

        public SaveModel Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonUtility.FromJson<SaveModel>(json);
        }
    }
}


