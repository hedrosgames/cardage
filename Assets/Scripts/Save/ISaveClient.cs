using UnityEngine;
public interface ISaveClient
{
    string Save(SOSaveDefinition definition);
    void Load(SOSaveDefinition definition, string json);
}

