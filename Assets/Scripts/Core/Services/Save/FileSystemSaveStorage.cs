using System.IO;
using UnityEngine;
using AFKS.Core.Services.Save;

namespace AFKS.Core.Services.Save
{
    /// <summary>
    /// 파일 시스템 기반 저장소 구현. 원자적 쓰기와 백업은 상위 서비스에서 처리합니다.
    /// </summary>
    public sealed class FileSystemSaveStorage : ISaveStorage
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllTextAtomic(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, content);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tempPath, path);
        }

        public void DeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (System.Exception e) { Debug.LogWarning($"[SaveStorage] 파일 삭제 실패: {e.Message}"); }
        }
    }
}


