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
    }
}


