using UnityEngine;
using System.Collections.Generic;
public class SaveClientSettings : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.5f;
    public string language = "EN";
    public string resolution = "";
    [System.Serializable]
    class Data
    {
        public float musicVolume;
        public float sfxVolume;
        public string language;
        public string resolution;
    }
    void OnEnable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.RegisterClient(saveDefinition, this);
    }
    void Start()
    {
        SaveEvents.RaiseLoad();
        ApplySettings();
    }
    void OnDisable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.UnregisterClient(saveDefinition, this);
    }
    public string Save(SOSaveDefinition definition)
    {
        var d = new Data
        {
            musicVolume = musicVolume,
            sfxVolume = sfxVolume,
            language = language,
            resolution = resolution
        };
        return JsonUtility.ToJson(d);
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        var d = JsonUtility.FromJson<Data>(json);
        if (d == null) return;
        musicVolume = d.musicVolume;
        sfxVolume = d.sfxVolume;
        if (!string.IsNullOrEmpty(d.language)) language = d.language;
        if (!string.IsNullOrEmpty(d.resolution)) resolution = d.resolution;
        ApplySettings();
    }
    void ApplySettings()
    {
        if (ManagerLocalization.Instance != null)
        ManagerLocalization.Instance.SetLanguage(language);
        if (ManagerAudio.Instance != null)
        {
            ManagerAudio.Instance.SetMusicVolume(musicVolume);
            ManagerAudio.Instance.SetSFXVolume(sfxVolume);
        }
    }
}

