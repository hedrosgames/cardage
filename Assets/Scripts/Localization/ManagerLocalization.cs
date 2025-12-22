using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
public class ManagerLocalization : MonoBehaviour
{
    public static ManagerLocalization Instance { get; private set; }
    public SOLocalizationData localizationData;
    public SaveClientSettings settingsClient;
    public string defaultLanguage = "EN";
    private string sheetId = "1HvY8sMGyzV5cVt_kzocyqmI90EF1KOCz5FF7wQZwPOE";
    private List<string> sheetGids = new List<string>
    {
        "0", "910185638", "723334572", "1042227826", "258366397", "1696503281"
    };
    private Dictionary<string, string> currentLanguageDictionary = new Dictionary<string, string>();
    public static event Action OnLanguageChanged;
    private static bool isQuitting = false;
    private string CachePath => Path.Combine(Application.persistentDataPath, "localization_cache.json");
    [Serializable]
    private class SerializationWrapper
    {
        public List<SOLocalizationData.Sheet> sheets;
    }
    private void Awake()
    {
        OnLanguageChanged = null;
        isQuitting = false;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (settingsClient == null)
        settingsClient = FindFirstObjectByType<SaveClientSettings>();
        DontDestroyOnLoad(gameObject);
    }
    private async void Start()
    {
        ApplyLanguageFromData();
        if (LoadFromCache())
        {
            ApplyLanguageFromData();
        }
        await CheckForUpdatesAsync();
    }
    private void OnApplicationQuit()
    {
        isQuitting = true;
    }
    private async Task CheckForUpdatesAsync()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        return;
        var masterData = new List<SOLocalizationData.Sheet>();
        foreach (string gid in sheetGids)
        {
            string url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gid}";
            string csv = await DownloadText(url);
            if (!string.IsNullOrEmpty(csv))
            {
                var sheet = ParseSheet(gid, csv);
                if (sheet != null) masterData.Add(sheet);
            }
        }
        if (masterData.Count == 0) return;
        SerializationWrapper wrapper = new SerializationWrapper { sheets = masterData };
        string newJson = JsonUtility.ToJson(wrapper);
        string newHash = ComputeHash(newJson);
        string oldHash = PlayerPrefs.GetString("LocHash", "");
        if (newHash != oldHash)
        {
            File.WriteAllText(CachePath, newJson);
            PlayerPrefs.SetString("LocHash", newHash);
            PlayerPrefs.Save();
            if (localizationData != null)
            {
                localizationData.sheets = masterData;
                string currentLang = settingsClient != null ? settingsClient.language : defaultLanguage;
                SetLanguage(currentLang);
            }
        }
        else
        {
        }
    }
    private async Task<string> DownloadText(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();
            if (www.result == UnityWebRequest.Result.Success)
            return www.downloadHandler.text;
            return null;
        }
    }
    private bool LoadFromCache()
    {
        if (!File.Exists(CachePath)) return false;
        try
        {
            string json = File.ReadAllText(CachePath);
            SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);
            if (wrapper != null && wrapper.sheets != null && wrapper.sheets.Count > 0)
            {
                if (localizationData != null)
                {
                    localizationData.sheets = wrapper.sheets;
                    return true;
                }
            }
        }
        catch (Exception)
        {
        }
        return false;
    }
    private SOLocalizationData.Sheet ParseSheet(string gid, string csvText)
    {
        List<string[]> rows = ParseCSV(csvText);
        if (rows.Count <= 1) return null;
        SOLocalizationData.Sheet newSheet = new SOLocalizationData.Sheet();
        newSheet.sheetName = $"Runtime_{gid}";
        string[] header = rows[0];
        List<string> languages = new List<string>();
        Dictionary<int, string> colToLang = new Dictionary<int, string>();
        for (int i = 1; i < header.Length; i++)
        {
            string langCode = header[i].Trim().ToUpper();
            if (langCode.Length <= 3 && !string.IsNullOrEmpty(langCode))
            {
                colToLang[i] = langCode;
                if (!languages.Contains(langCode)) languages.Add(langCode);
            }
        }
        foreach (string lang in languages)
        {
            newSheet.entries.Add(new SOLocalizationData.LanguageEntry
            {
                languageCode = lang,
                keys = new string[0],
                texts = new string[0]
            });
        }
        var tempLists = new Dictionary<string, (List<string> k, List<string> t)>();
        foreach(var l in languages) tempLists[l] = (new List<string>(), new List<string>());
        for (int i = 1; i < rows.Count; i++)
        {
            string[] row = rows[i];
            if (row.Length < 1) continue;
            string key = row[0].Trim();
            if (string.IsNullOrEmpty(key)) continue;
            foreach (var kvp in colToLang)
            {
                string text = kvp.Key < row.Length ? row[kvp.Key] : "";
                tempLists[kvp.Value].k.Add(key);
                tempLists[kvp.Value].t.Add(text);
            }
        }
        foreach (var entry in newSheet.entries)
        {
            if (tempLists.TryGetValue(entry.languageCode, out var lists))
            {
                entry.keys = lists.k.ToArray();
                entry.texts = lists.t.ToArray();
            }
        }
        return newSheet;
    }
    private List<string[]> ParseCSV(string text)
    {
        List<string[]> rows = new List<string[]>();
        List<string> currentRow = new List<string>();
        StringBuilder currentField = new StringBuilder();
        bool insideQuotes = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"')
            {
                if (insideQuotes && i + 1 < text.Length && text[i + 1] == '"') { currentField.Append('"'); i++; }
                else insideQuotes = !insideQuotes;
            }
            else if (c == ',' && !insideQuotes)
            {
                currentRow.Add(currentField.ToString());
                currentField.Clear();
            }
            else if ((c == '\r' || c == '\n') && !insideQuotes)
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                currentRow.Add(currentField.ToString());
                rows.Add(currentRow.ToArray());
                currentRow.Clear();
                currentField.Clear();
            }
            else currentField.Append(c);
        }
        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField.ToString());
            rows.Add(currentRow.ToArray());
        }
        return rows;
    }
    private string ComputeHash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
    public void ApplyLanguageFromData()
    {
        if (settingsClient != null)
        SetLanguage(settingsClient.language);
        else
        SetLanguage(defaultLanguage);
    }
    public void SetLanguage(string langCode)
    {
        if (isQuitting || localizationData == null) return;
        langCode = langCode.ToUpper();
        currentLanguageDictionary.Clear();
        if (localizationData.sheets != null)
        {
            foreach (var sheet in localizationData.sheets)
            ProcessEntries(sheet.entries, langCode);
        }
        if (localizationData.entries_DEPRECATED != null && localizationData.entries_DEPRECATED.Count > 0)
        {
            ProcessEntries(localizationData.entries_DEPRECATED, langCode);
        }
        if (settingsClient != null)
        {
            if (settingsClient.language != langCode)
            {
                settingsClient.language = langCode;
                SaveEvents.RaiseSave();
            }
        }
        OnLanguageChanged?.Invoke();
    }
    void ProcessEntries(List<SOLocalizationData.LanguageEntry> entries, string langCode)
    {
        var targetEntry = entries.FirstOrDefault(e => e.languageCode.ToUpper() == langCode);
        if (targetEntry == null)
        targetEntry = entries.FirstOrDefault(e => e.languageCode.ToUpper() == defaultLanguage);
        if (targetEntry != null)
        {
            for (int i = 0; i < targetEntry.keys.Length; i++)
            {
                if (i < targetEntry.texts.Length && !currentLanguageDictionary.ContainsKey(targetEntry.keys[i]))
                currentLanguageDictionary[targetEntry.keys[i]] = targetEntry.texts[i];
            }
        }
    }
    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key)) return "INVALID_KEY";
        if (currentLanguageDictionary.TryGetValue(key, out string text)) return text;
        return $"[{key}]";
    }
}

