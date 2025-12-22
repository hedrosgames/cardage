using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
public class ManagerSave : MonoBehaviour
{
    public static ManagerSave Instance { get; private set;
}
public List<SOSaveDefinition> definitions = new List<SOSaveDefinition>();
Dictionary<string, ISaveClient> clients = new Dictionary<string, ISaveClient>();
string FilePath => Path.Combine(Application.persistentDataPath, "save.json");
void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    if (transform.parent != null)
    {
        transform.SetParent(null);
    }
    DontDestroyOnLoad(gameObject);
    var localClients = GetComponents<ISaveClient>();
    foreach (var client in localClients)
    {
        var field = client.GetType().GetField("saveDefinition");
        if (field == null) field = client.GetType().GetField("_saveDefinition");
        if (field != null)
        {
            SOSaveDefinition def = field.GetValue(client) as SOSaveDefinition;
            if (def != null) RegisterClient(def, client);
        }
    }
    SceneManager.sceneLoaded += OnSceneLoaded;
    SaveEvents.RequestSave += SaveAll;
    SaveEvents.RequestLoad += LoadAll;
    SaveEvents.RequestSaveSpecific += SaveSpecific;
    LoadAll();
}
void OnDestroy()
{
    if (Instance == this)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SaveEvents.RequestSave -= SaveAll;
        SaveEvents.RequestLoad -= LoadAll;
        SaveEvents.RequestSaveSpecific -= SaveSpecific;
        Instance = null;
    }
}
public void RegisterClient(SOSaveDefinition definition, ISaveClient client)
{
    if (definition == null || client == null) return;
    if (clients.ContainsKey(definition.id) && client is MonoBehaviour mb)
    {
        if (Instance != null && mb.gameObject != Instance.gameObject) return;
    }
    clients[definition.id] = client;
    if (!definitions.Contains(definition))
    {
        definitions.Add(definition);
    }
}
public void UnregisterClient(SOSaveDefinition definition, ISaveClient client)
{
    if (definition == null) return;
    if (clients.TryGetValue(definition.id, out var existing) && existing == client)
    {
        clients.Remove(definition.id);
    }
}
public int GetRegisteredClientsCount()
{
    return clients.Count;
}
[Serializable]
class Entry
{
    public string key;
    public string value;
}
[Serializable]
class SaveFile
{
    public List<Entry> entries = new List<Entry>();
}
public void SaveAll()
{
    var file = new SaveFile();
    if (File.Exists(FilePath))
    {
        try
        {
            string text = File.ReadAllText(FilePath);
            var existingFile = JsonUtility.FromJson<SaveFile>(text);
            if (existingFile != null && existingFile.entries != null)
            {
                file.entries = new List<Entry>(existingFile.entries);
            }
        }
        catch { }
    }
    var savedData = new System.Collections.Generic.Dictionary<string, string>();
    string caller = GetCallerName();
    int savedCount = 0;
    foreach (var def in definitions)
    {
        if (def == null) continue;
        if (!clients.TryGetValue(def.id, out var client) || client == null)
        {
            continue;
        }
        string json = client.Save(def);
        if (string.IsNullOrEmpty(json))
        {
            continue;
        }
        file.entries.RemoveAll(entry => entry.key == def.id);
        var entry = new Entry
        {
            key = def.id,
            value = json
        };
        file.entries.Add(entry);
        savedData[def.id] = json.Length > 100 ? json.Substring(0, 100) + "..." : json;
        savedCount++;
    }
    if (savedCount > 0 || file.entries.Count > 0)
    {
        string finalJson = JsonUtility.ToJson(file, true);
        try
        {
            File.WriteAllText(FilePath, finalJson);
            SaveEvents.NotifySaveExecuted(caller, savedData);
        }
        catch (System.Exception)
        {
        }
    }
}
public void LoadAll()
{
    string caller = GetCallerName();
    var loadedData = new System.Collections.Generic.Dictionary<string, string>();
    if (!File.Exists(FilePath))
    {
        SaveEvents.NotifyLoadExecuted(caller, loadedData);
        return;
    }
    string text;
    try
    {
        text = File.ReadAllText(FilePath);
    }
    catch
    {
        SaveEvents.NotifyLoadExecuted(caller, loadedData);
        return;
    }
    if (string.IsNullOrEmpty(text))
    {
        SaveEvents.NotifyLoadExecuted(caller, loadedData);
        return;
    }
    var file = JsonUtility.FromJson<SaveFile>(text);
    if (file == null || file.entries == null)
    {
        SaveEvents.NotifyLoadExecuted(caller, loadedData);
        return;
    }
    var dict = new Dictionary<string, string>();
    foreach (var entry in file.entries)
    {
        if (string.IsNullOrEmpty(entry.key)) continue;
        dict[entry.key] = entry.value ?? string.Empty;
    }
    foreach (var def in definitions)
    {
        if (def == null) continue;
        if (def.id == "savequit") continue;
        if (!clients.TryGetValue(def.id, out var client) || client == null)
        {
            continue;
        }
        dict.TryGetValue(def.id, out var json);
        client.Load(def, json);
        if (!string.IsNullOrEmpty(json))
        {
            loadedData[def.id] = json.Length > 100 ? json.Substring(0, 100) + "..." : json;
        }
    }
    SaveEvents.NotifyLoadExecuted(caller, loadedData);
}
public void LoadSpecific(string saveId)
{
    if (string.IsNullOrEmpty(saveId)) return;
    SOSaveDefinition definition = definitions.FirstOrDefault(d => d.id == saveId);
    if (definition == null) return;
    if (!clients.TryGetValue(saveId, out var client) || client == null) return;
    if (!File.Exists(FilePath)) return;
    string text;
    try
    {
        text = File.ReadAllText(FilePath);
    }
    catch
    {
        return;
    }
    if (string.IsNullOrEmpty(text)) return;
    var file = JsonUtility.FromJson<SaveFile>(text);
    if (file == null || file.entries == null) return;
    string json = string.Empty;
    foreach (var entry in file.entries)
    {
        if (entry.key == saveId)
        {
            json = entry.value ?? string.Empty;
            break;
        }
    }
    client.Load(definition, json);
    string caller = GetCallerName();
    var loadedData = new Dictionary<string, string>();
    if (!string.IsNullOrEmpty(json))
    {
        loadedData[saveId] = json.Length > 100 ? json.Substring(0, 100) + "..." : json;
    }
    SaveEvents.NotifyLoadExecuted(caller, loadedData);
}
public void SaveSpecific(string saveId)
{
    if (string.IsNullOrEmpty(saveId)) return;
    if (!clients.ContainsKey(saveId))
    {
        var localClients = GetComponents<ISaveClient>();
        foreach (var c in localClients)
        {
            var field = c.GetType().GetField("saveDefinition");
            if (field != null && field.GetValue(c) is SOSaveDefinition def && def.id == saveId)
            {
                RegisterClient(def, c);
                break;
            }
        }
    }
    SOSaveDefinition definition = definitions.FirstOrDefault(d => d.id == saveId);
    if (definition == null)
    {
        return;
    }
    if (!clients.TryGetValue(saveId, out var client) || client == null)
    {
        return;
    }
    string json = client.Save(definition);
    if (string.IsNullOrEmpty(json)) return;
    var file = new SaveFile();
    if (File.Exists(FilePath))
    {
        try
        {
            string text = File.ReadAllText(FilePath);
            var existingFile = JsonUtility.FromJson<SaveFile>(text);
            if (existingFile != null && existingFile.entries != null)
            {
                file.entries = existingFile.entries;
            }
        }
        catch { }
    }
    file.entries.RemoveAll(entry => entry.key == saveId);
    file.entries.Add(new Entry { key = saveId, value = json });
    string finalJson = JsonUtility.ToJson(file, true);
    try
    {
        File.WriteAllText(FilePath, finalJson);
        string caller = GetCallerName();
        var savedData = new Dictionary<string, string>();
        savedData[saveId] = json.Length > 100 ? json.Substring(0, 100) + "..." : json;
        SaveEvents.NotifySaveExecuted(caller, savedData);
    }
    catch (System.Exception)
    {
    }
}
public void SaveByEnum(SaveId saveId)
{
    string saveIdString = ConvertSaveIdToString(saveId);
    if (!string.IsNullOrEmpty(saveIdString))
    {
        SaveSpecific(saveIdString);
    }
}
private string ConvertSaveIdToString(SaveId saveId)
{
    switch (saveId)
    {
        case SaveId.SaveWorld:
        return "saveworld";
        case SaveId.SaveZone:
        return "savezone";
        case SaveId.SaveCard:
        return "savecard";
        case SaveId.SaveSettings:
        return "savesettings";
        case SaveId.SaveMenu:
        return "savemenu";
        case SaveId.SaveAchievements:
        return "saveachievements";
        case SaveId.SaveQuit:
        return "savequit";
        case SaveId.SaveMoment:
        return "savemoment";
        default:
        return string.Empty;
    }
}
public void WipeClientData(string idToWipe)
{
    SOSaveDefinition definition = definitions.FirstOrDefault(d => d.id == idToWipe);
    if (clients.TryGetValue(idToWipe, out var client))
    {
        client.Load(definition, string.Empty);
    }
    if (!File.Exists(FilePath)) return;
    string text = File.ReadAllText(FilePath);
    var file = JsonUtility.FromJson<SaveFile>(text);
    if (file == null || file.entries == null) return;
    file.entries.RemoveAll(entry => entry.key == idToWipe);
    string finalJson = JsonUtility.ToJson(file, true);
    File.WriteAllText(FilePath, finalJson);
}
void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    LoadAll();
}
public void LoadMoment()
{
    LoadSpecific("savemoment");
    NpcHelper[] allSwapScripts = FindObjectsByType<NpcHelper>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    foreach (var swapScript in allSwapScripts)
    {
        if (swapScript != null)
        {
            swapScript.ForceCheckAndSwap();
        }
    }
}
string GetCallerName()
{
    try
    {
        var stackTrace = new StackTrace(true);
        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            var frame = stackTrace.GetFrame(i);
            if (frame == null) continue;
            var method = frame.GetMethod();
            if (method == null) continue;
            string className = method.DeclaringType != null ? method.DeclaringType.Name : "Unknown";
            if (className != "ManagerSave" && className != "SaveEvents")
            {
                string methodName = method.Name;
                return $"{className}.{methodName}";
            }
        }
    }
    catch { }
    return "Unknown";
}
}

