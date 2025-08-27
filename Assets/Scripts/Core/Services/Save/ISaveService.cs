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
        /// 실수를 저장합니다.
        /// </summary>
        void SetFloat(string key, float value);

        /// <summary>
        /// 실수를 로드합니다. 기본값을 반환합니다.
        /// </summary>
        float GetFloat(string key, float defaultValue = 0f);

        /// <summary>
        /// 저장된 키를 삭제합니다.
        /// </summary>
        void DeleteKey(string key);

        /// <summary>
        /// 모든 저장 데이터를 삭제합니다.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// 진행 관련 데이터만 초기화합니다(체크포인트/스테이지 상태/세이브 파일). 설정(볼륨 등)은 유지합니다.
        /// </summary>
        /// <param name="keepSettings">true면 오디오 등 환경 설정은 유지합니다.</param>
        void ResetProgress(bool keepSettings = true);

        // 진행도(체크포인트) 저장/조회
        void SetStageCheckpoint(string stageId, string checkpointId);
        string GetStageCheckpoint(string stageId);
    }
}


