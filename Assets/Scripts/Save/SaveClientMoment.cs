using UnityEngine;
using System;
public class SaveClientMoment : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    private int currentMoment = 1;
    public event Action<int> OnMomentChanged;
    [System.Serializable]
    class Data
    {
        public int moment;
    }
    void OnEnable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.RegisterClient(saveDefinition, this);
        }
    }
    void OnDisable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.UnregisterClient(saveDefinition, this);
        }
    }
    void Start()
    {
        SaveEvents.RaiseLoad();
    }
    public int GetMoment()
    {
        return currentMoment;
    }
    public void SetMoment(int moment)
    {
        if (moment < 1 || moment > 5) return;
        if (currentMoment == moment) return;
        currentMoment = moment;
        SaveEvents.RaiseSave();
        OnMomentChanged?.Invoke(currentMoment);
    }
    public string Save(SOSaveDefinition definition)
    {
        var d = new Data { moment = currentMoment };
        return JsonUtility.ToJson(d);
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            currentMoment = 1;
            OnMomentChanged?.Invoke(currentMoment);
            return;
        }
        var d = JsonUtility.FromJson<Data>(json);
        if (d != null)
        {
            currentMoment = Mathf.Clamp(d.moment, 1, 5);
            OnMomentChanged?.Invoke(currentMoment);
        }
    }
}

