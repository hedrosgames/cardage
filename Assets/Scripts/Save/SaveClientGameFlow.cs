using UnityEngine;
using System.Collections.Generic;
using System;

public class SaveClientGameFlow : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    private Dictionary<string, int> flowFlags = new Dictionary<string, int>();
    public event Action OnLoadComplete;
    public event Action<string> OnFlagChanged;
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

    public void SetFlag(string flagId, int value)
    {
        if (string.IsNullOrEmpty(flagId)) return;
        if (flowFlags.ContainsKey(flagId) && flowFlags[flagId] == value) return;
        
        flowFlags[flagId] = value;
        if (ManagerSave.Instance != null && saveDefinition != null)
            ManagerSave.Instance.SaveSpecific(saveDefinition.id);
            
        OnFlagChanged?.Invoke(flagId);
    }

    public void SetFlag(SOGameFlowFlag flag, int value)
    {
        if (flag == null) return;
        SetFlag(flag.id, value);
    }

    public void SetFlag(SOZoneFlag flag, int value)
    {
        if (flag == null) return;
        SetFlag(flag.id, value);
    }

    public int GetFlag(string flagId)
    {
        if (string.IsNullOrEmpty(flagId) || !flowFlags.ContainsKey(flagId)) return 0;
        return flowFlags[flagId];
    }

    public int GetFlag(SOGameFlowFlag flag)
    {
        if (flag == null) return 0;
        return GetFlag(flag.id);
    }

    public bool HasFlag(string flagId)
    {
        return GetFlag(flagId) > 0;
    }

    public bool HasFlag(SOGameFlowFlag flag)
    {
        if (flag == null) return false;
        return HasFlag(flag.id);
    }

    // Atalhos especÃ­ficos para diÃ¡logos
    public void markDialogueAsRead(string dialogueId)
    {
        SetFlag("dial_" + dialogueId, 1);
    }

    public bool IsDialogueRead(string dialogueId)
    {
        return HasFlag("dial_" + dialogueId);
    }

    public string Save(SOSaveDefinition definition)
    {
        List<string> k = new List<string>(flowFlags.Keys);
        List<int> v = new List<int>(flowFlags.Values);
        return JsonUtility.ToJson(new Data { keys = k.ToArray(), values = v.ToArray() });
    }

    public void Load(SOSaveDefinition definition, string json)
    {
        flowFlags.Clear();
        if (!string.IsNullOrEmpty(json))
        {
            var d = JsonUtility.FromJson<Data>(json);
            if (d != null && d.keys != null)
            {
                for (int i = 0; i < d.keys.Length; i++) flowFlags[d.keys[i]] = d.values[i];
            }
        }
        IsReady = true;
        OnLoadComplete?.Invoke();
    }
}
