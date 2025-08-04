namespace AFKS.Shared.Interfaces
{
    /// <summary>
    /// 저장 가능한 오브젝트를 위한 인터페이스
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// 저장 데이터 획득
        /// </summary>
        /// <returns>JSON 형태의 저장 데이터</returns>
        string GetSaveData();
        
        /// <summary>
        /// 저장 데이터 로드
        /// </summary>
        /// <param name="data">JSON 형태의 저장 데이터</param>
        void LoadSaveData(string data);
        
        /// <summary>
        /// 저장 가능한 고유 ID
        /// </summary>
        string SaveID { get; }
    }
}