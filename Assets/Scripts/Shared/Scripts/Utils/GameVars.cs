using AFKS.Shared.Core;

namespace AFKS.Shared.Utils
{
    /// <summary>
    /// SaveManager 변수 번들에 대한 간단 헬퍼
    /// </summary>
    public static class GameVars
    {
        public static void SetString(string key, string value)
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.SetVariable(key, value);
            }
        }

        public static string GetString(string key, string defaultValue = "")
        {
            if (SaveManager.HasInstance && SaveManager.Instance.TryGetVariable(key, out var v))
            {
                return v;
            }
            return defaultValue;
        }

        public static void SetBool(string key, bool value)
        {
            SetString(key, value ? "1" : "0");
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            var s = GetString(key, defaultValue ? "1" : "0");
            return s == "1" || s.ToLower() == "true";
        }

        public static void SetInt(string key, int value)
        {
            SetString(key, value.ToString());
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            var s = GetString(key, defaultValue.ToString());
            if (int.TryParse(s, out var v)) return v;
            return defaultValue;
        }
    }
}


