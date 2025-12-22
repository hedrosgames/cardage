using UnityEngine;
using System.Collections.Generic;
using System;
public class SaveClientAchievements : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    public List<string> unlockedAchievements = new List<string>();
    public event Action OnLoadComplete;
    [System.Serializable]
    class Data
    {
        public string[] unlocked;
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
    public void Unlock(string id)
    {
        if (!unlockedAchievements.Contains(id))
        {
            unlockedAchievements.Add(id);
            if (saveDefinition != null)
            {
                SaveEvents.RaiseSaveSpecific(saveDefinition.id);
            }
        }
    }
    public bool IsUnlocked(string id)
    {
        return unlockedAchievements.Contains(id);
    }
    public string Save(SOSaveDefinition definition)
    {
        var d = new Data { unlocked = unlockedAchievements.ToArray() };
        return JsonUtility.ToJson(d);
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            OnLoadComplete?.Invoke();
            return;
        }
        var d = JsonUtility.FromJson<Data>(json);
        if (d != null && d.unlocked != null)
        {
            unlockedAchievements = new List<string>(d.unlocked);
        }
        OnLoadComplete?.Invoke();
    }
}

