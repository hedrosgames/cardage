using UnityEngine;
using System.Collections.Generic;
using System;
public class SaveClientZone : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    private Dictionary<string, int> zoneFlags = new Dictionary<string, int>();
    private Dictionary<string, string> zoneFlagStrings = new Dictionary<string, string>();
    public event Action OnLoadComplete;
    [System.Serializable]
    class Data
    {
        public string[] keys;
        public int[] values;
    }
    [System.Serializable]
    class StringData
    {
        public string[] keys;
        public string[] values;
    }
    void OnEnable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.RegisterClient(saveDefinition, this);
    }
    void Start()
    {
        SaveEvents.RaiseLoad();
    }
    void OnDisable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.UnregisterClient(saveDefinition, this);
    }
    public void SetFlag(SOZoneFlag flag, int value)
    {
        if (flag == null || string.IsNullOrEmpty(flag.id)) return;
        if (zoneFlags.ContainsKey(flag.id) && zoneFlags[flag.id] == value)
        return;
        zoneFlags[flag.id] = value;
        SaveEvents.RaiseSave();
        OnFlagChanged?.Invoke(flag);
    }
    public event Action<SOZoneFlag> OnFlagChanged;
    public int GetFlag(SOZoneFlag flag)
    {
        if (flag == null || string.IsNullOrEmpty(flag.id)) return 0;
        if (!zoneFlags.ContainsKey(flag.id)) return 0;
        return zoneFlags[flag.id];
    }
    public bool HasFlag(SOZoneFlag flag)
    {
        return GetFlag(flag) > 0;
    }
    public void AddIdToFlag(SOZoneFlag flag, string id)
    {
        if (flag == null || string.IsNullOrEmpty(flag.id) || string.IsNullOrEmpty(id)) return;
        string currentValue = GetFlagString(flag);
        if (string.IsNullOrEmpty(currentValue))
        {
            SetFlagString(flag, id);
            return;
        }
        string[] ids = currentValue.Split(',');
        foreach (string existingId in ids)
        {
            if (existingId.Trim() == id) return;
        }
        SetFlagString(flag, currentValue + "," + id);
    }
    public bool HasIdInFlag(SOZoneFlag flag, string id)
    {
        if (flag == null || string.IsNullOrEmpty(flag.id) || string.IsNullOrEmpty(id)) return false;
        string currentValue = GetFlagString(flag);
        if (string.IsNullOrEmpty(currentValue)) return false;
        string[] ids = currentValue.Split(',');
        foreach (string existingId in ids)
        {
            if (existingId.Trim() == id) return true;
        }
        return false;
    }
    public string GetFlagString(SOZoneFlag flag)
    {
        if (flag == null || string.IsNullOrEmpty(flag.id)) return string.Empty;
        if (!zoneFlagStrings.ContainsKey(flag.id)) return string.Empty;
        return zoneFlagStrings[flag.id];
    }
    public void SetFlagString(SOZoneFlag flag, string value)
    {
        if (flag == null || string.IsNullOrEmpty(flag.id)) return;
        if (zoneFlagStrings.ContainsKey(flag.id) && zoneFlagStrings[flag.id] == value) return;
        zoneFlagStrings[flag.id] = value;
        SaveEvents.RaiseSave();
        OnFlagChanged?.Invoke(flag);
    }
    public string Save(SOSaveDefinition definition)
    {
        List<string> k = new List<string>(zoneFlags.Keys);
        List<int> v = new List<int>(zoneFlags.Values);
        var d = new Data { keys = k.ToArray(), values = v.ToArray() };
        string json = JsonUtility.ToJson(d);
        if (zoneFlagStrings.Count > 0)
        {
            List<string> sk = new List<string>(zoneFlagStrings.Keys);
            List<string> sv = new List<string>(zoneFlagStrings.Values);
            var sd = new StringData { keys = sk.ToArray(), values = sv.ToArray() };
            string stringJson = JsonUtility.ToJson(sd);
            json += "|STRING_DATA|" + stringJson;
        }
        return json;
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        zoneFlags.Clear();
        zoneFlagStrings.Clear();
        if (string.IsNullOrEmpty(json))
        {
            OnLoadComplete?.Invoke();
            return;
        }
        string[] parts = json.Split(new[] { "|STRING_DATA|" }, StringSplitOptions.None);
        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
        {
            var d = JsonUtility.FromJson<Data>(parts[0]);
            if (d != null && d.keys != null && d.values != null)
            {
                int count = Mathf.Min(d.keys.Length, d.values.Length);
                for (int i = 0; i < count; i++)
                {
                    zoneFlags[d.keys[i]] = d.values[i];
                }
            }
        }
        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
        {
            var sd = JsonUtility.FromJson<StringData>(parts[1]);
            if (sd != null && sd.keys != null && sd.values != null)
            {
                int count = Mathf.Min(sd.keys.Length, sd.values.Length);
                for (int i = 0; i < count; i++)
                {
                    zoneFlagStrings[sd.keys[i]] = sd.values[i];
                }
            }
        }
        OnLoadComplete?.Invoke();
    }
}

