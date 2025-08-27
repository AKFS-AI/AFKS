using System;

namespace AFKS.Core.Services.Save
{
    public interface ISaveSerializer
    {
        string Serialize(SaveModel model, bool prettyPrint = true);
        SaveModel Deserialize(string json);
    }
}


