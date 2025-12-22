using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class SaveClientMenu : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    public UILocalizedText playButtonLocalizer;
    public bool hasSaveFile = false;
    [System.Serializable]
    class Data
    {
        public bool hasSaveFile;
    }
    void OnEnable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.RegisterClient(saveDefinition, this);
        ManagerLocalization.OnLanguageChanged += UpdatePlayButtonText;
    }
    void OnDisable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.UnregisterClient(saveDefinition, this);
        ManagerLocalization.OnLanguageChanged -= UpdatePlayButtonText;
    }
    public string Save(SOSaveDefinition definition)
    {
        var d = new Data { hasSaveFile = hasSaveFile };
        return JsonUtility.ToJson(d);
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            hasSaveFile = false;
            UpdatePlayButtonText();
            return;
        }
        var d = JsonUtility.FromJson<Data>(json);
        if (d != null) hasSaveFile = d.hasSaveFile;
        UpdatePlayButtonText();
    }
    public void SetSaveState(bool fileExists)
    {
        hasSaveFile = fileExists;
        UpdatePlayButtonText();
    }
    public void InitializeNewGame()
    {
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.WipeClientData("savecard");
            ManagerSave.Instance.WipeClientData("saveworld");
        }
        hasSaveFile = true;
    }
    public void UpdatePlayButtonText()
    {
        if (playButtonLocalizer != null && playButtonLocalizer.gameObject != null)
        {
            string key = hasSaveFile ? "MENU_LOAD_GAME" : "MENU_NEW_GAME";
            playButtonLocalizer.localizationKey = key;
            playButtonLocalizer.UpdateText();
        }
    }
}

