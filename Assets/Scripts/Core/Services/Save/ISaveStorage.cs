namespace AFKS.Core.Services.Save
{
    public interface ISaveStorage
    {
        bool Exists(string path);
        string ReadAllText(string path);
        void WriteAllTextAtomic(string path, string content);
        void DeleteFile(string path);
    }
}


