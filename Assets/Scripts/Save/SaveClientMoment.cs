using UnityEngine;
using System.Collections.Generic;
using System;
public class SaveClientMoment : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    private Dictionary<string, int> momentFlags = new Dictionary<string, int>();
    public event Action OnLoadComplete;
    public bool IsReady { get; private set; } = false;
    [System.Serializable]
    class Data { public string[] keys; public int[] values; }
    void OnEnable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.RegisterClient(saveDefinition, this);
            if (!ManagerSave.Instance.definitions.Contains(saveDefinition))
            ManagerSave.Instance.definitions.Add(saveDefinition);
        }
    }
    public void SetFlag(SOZoneFlag flag, int value)
    {
        if (flag == null || string.IsNullOrEmpty(flag.id)) return;
        momentFlags[flag.id] = value;
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.SaveSpecific(saveDefinition.id);
    }
    public bool HasFlag(SOZoneFlag flag)
    {
        if (flag == null || !momentFlags.ContainsKey(flag.id)) return false;
        return momentFlags[flag.id] > 0;
    }
    public string Save(SOSaveDefinition definition)
    {
        List<string> k = new List<string>(momentFlags.Keys);
        List<int> v = new List<int>(momentFlags.Values);
        return JsonUtility.ToJson(new Data { keys = k.ToArray(), values = v.ToArray() });
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        momentFlags.Clear();
        if (!string.IsNullOrEmpty(json))
        {
            var d = JsonUtility.FromJson<Data>(json);
            if (d != null && d.keys != null)
            {
                for (int i = 0; i < d.keys.Length; i++) momentFlags[d.keys[i]] = d.values[i];
            }
        }
        IsReady = true;
        OnLoadComplete?.Invoke();
    }
}

