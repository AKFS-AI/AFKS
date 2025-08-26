using System;

namespace AFKS.Core.Services.Save
{
    /// <summary>
    /// 저장 시스템 인터페이스. 진행/설정 등 저장 유무를 조회하고 저장/로드를 제공합니다.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// 어떤 형태로든 저장 데이터가 존재하는지 빠르게 확인합니다.
        /// </summary>
        bool HasAnySave();

        /// <summary>
        /// 모든 데이터를 저장합니다.
        /// </summary>
        void SaveAll();

        /// <summary>
        /// 저장 데이터를 로드합니다. 성공 여부를 반환합니다.
        /// </summary>
        bool TryLoadAll();

        /// <summary>
        /// 불린 값을 저장합니다.
        /// </summary>
        void SetBool(string key, bool value);

        /// <summary>
        /// 불린 값을 로드합니다. 기본값을 반환합니다.
        /// </summary>
        bool GetBool(string key, bool defaultValue = false);

        /// <summary>
        /// 문자열을 저장합니다.
        /// </summary>
        void SetString(string key, string value);

        /// <summary>
        /// 문자열을 로드합니다. 기본값을 반환합니다.
        /// </summary>
        string GetString(string key, string defaultValue = "");

        /// <summary>
        /// 정수를 저장합니다.
        /// </summary>
        void SetInt(string key, int value);

        /// <summary>
        /// 정수를 로드합니다. 기본값을 반환합니다.
        /// </summary>
        int GetInt(string key, int defaultValue = 0);

        /// <summary>
        /// 저장된 키를 삭제합니다.
        /// </summary>
        void DeleteKey(string key);

        /// <summary>
        /// 모든 저장 데이터를 삭제합니다.
        /// </summary>
        void DeleteAll();
    }
}


