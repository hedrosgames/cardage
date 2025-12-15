using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
public class EditorSaveDebugger : EditorWindow
{
    Vector2 scroll;
    static Dictionary<string, Dictionary<string, string>> fieldDescriptions = new Dictionary<string, Dictionary<string, string>>();
    [System.Serializable]
    class DebugEntry
    {
        public string key;
        public string value;
    }
    [System.Serializable]
    class DebugSaveFile
    {
        public List<DebugEntry> entries = new List<DebugEntry>();
    }
    [MenuItem("Save/Save Debugger")]
    static void Open()
    {
        InitializeFieldDescriptions();
        GetWindow<EditorSaveDebugger>("Save Debugger");
    }
    static void InitializeFieldDescriptions()
    {
        fieldDescriptions.Clear();
        // SaveClientWorld - campos: px, py, pz, cx, cy, cz, areaId, tutorialsDone
        var worldFields = new Dictionary<string, string>
        {
            { "px", "Posição Player X" },
            { "py", "Posição Player Y" },
            { "pz", "Posição Player Z" },
            { "cx", "Posição Camera X" },
            { "cy", "Posição Camera Y" },
            { "cz", "Posição Camera Z" },
            { "areaId", "Área Atual" },
            { "areaid", "Área Atual" }, // minúsculo

        };
        fieldDescriptions["World"] = worldFields;
        fieldDescriptions["SaveWorld"] = worldFields;
        fieldDescriptions["saveworld"] = worldFields; // minúsculo
        // SaveClientCard - campos: deckId, opponentId, rules
        var cardFields = new Dictionary<string, string>
        {
            { "deckId", "Deck do Jogador" },
            { "opponentId", "Oponente" },
            { "rules", "Regras Ativas" }
        };
        fieldDescriptions["Card"] = cardFields;
        fieldDescriptions["SaveCard"] = cardFields;
        fieldDescriptions["savecard"] = cardFields; // minúsculo
        // SaveClientSettings - campos: musicVolume, sfxVolume, language, resolution
        var settingsFields = new Dictionary<string, string>
        {
            { "musicVolume", "Volume Música" },
            { "sfxVolume", "Volume SFX" },
            { "language", "Idioma" },
            { "resolution", "Resolução" }
        };
        fieldDescriptions["Settings"] = settingsFields;
        fieldDescriptions["SaveSettings"] = settingsFields;
        fieldDescriptions["savesettings"] = settingsFields; // minúsculo
        // SaveClientZone - campos: keys, values (arrays)
        var zoneFields = new Dictionary<string, string>
        {
            { "keys", "Flags de Zona (IDs)" },
            { "values", "Valores das Flags" }
        };
        fieldDescriptions["Zone"] = zoneFields;
        fieldDescriptions["SaveZone"] = zoneFields;
        fieldDescriptions["savezone"] = zoneFields; // minúsculo
    }
    [MenuItem("Save/Apagar Save")]
    static void QuickDelete()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else
        {
        }
        AssetDatabase.Refresh();
    }
    void OnGUI()
    {
        GUILayout.Space(10);
        DrawHeader();
        GUILayout.Space(10);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawSaveList();
        EditorGUILayout.EndScrollView();
    }
    void DrawHeader()
    {
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("APAGAR TODO OS SAVES", GUILayout.Height(30)))
        {
            QuickDelete();
        }
        GUI.backgroundColor = Color.white;
    }
    void DrawSaveList()
    {
        var loadedData = LoadDataDictionary();
        var definitions = FindAllDefinitions();
        if (definitions.Count == 0)
        {
            EditorGUILayout.HelpBox("Nenhum SOSaveDefinition encontrado no projeto.", MessageType.Info);
            return;
        }
        foreach (var def in definitions)
        {
            if (def == null) continue;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(def.id, EditorStyles.boldLabel, GUILayout.Width(120));
            if (loadedData.ContainsKey(def.id))
            {
                GUI.color = Color.green;
                GUILayout.Label("SALVO", EditorStyles.miniBoldLabel, GUILayout.Width(50));
                GUI.color = Color.white;
                GUILayout.Space(10);
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("Apagar Save", GUILayout.Width(100)))
                {
                    DeleteSpecificSave(def.id);
                    loadedData = LoadDataDictionary();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.color = new Color(1f, 0.6f, 0.6f);
                GUILayout.Label("NÃO SALVO", EditorStyles.miniBoldLabel, GUILayout.Width(80));
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
            if (loadedData.TryGetValue(def.id, out string json))
            {
                DrawFormattedJson(json, def.id);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }
    }
    List<SOSaveDefinition> FindAllDefinitions()
    {
        List<SOSaveDefinition> list = new List<SOSaveDefinition>();
        string[] guids = AssetDatabase.FindAssets("t:SOSaveDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<SOSaveDefinition>(path);
            if (so != null) list.Add(so);
        }
        return list;
    }
    Dictionary<string, string> LoadDataDictionary()
    {
        var dict = new Dictionary<string, string>();
        string path = GetPath();
        if (!File.Exists(path)) return dict;
        try
        {
            string json = File.ReadAllText(path);
            var file = JsonUtility.FromJson<DebugSaveFile>(json);
            if (file != null && file.entries != null)
            {
                foreach (var entry in file.entries)
                {
                    if (!string.IsNullOrEmpty(entry.key))
                    dict[entry.key] = entry.value;
                }
            }
        }
        catch { }
        return dict;
    }
    string FormatJson(string json, string saveId)
    {
        if (string.IsNullOrEmpty(json)) return "Vazio";
        // Tenta encontrar o mapeamento baseado no ID do save
        Dictionary<string, string> fieldMap = null;
        bool isWorld = false;
        foreach (var kvp in fieldDescriptions)
        {
            if (saveId.Contains(kvp.Key, System.StringComparison.OrdinalIgnoreCase))
            {
                fieldMap = kvp.Value;
                if (kvp.Key == "World" || kvp.Key == "SaveWorld")
                {
                    isWorld = true;
                }
                break;
            }
        }
        // Se for SaveClientZone, trata de forma especial (pode ter |STRING_DATA|)
        if (json.Contains("|STRING_DATA|"))
        {
            return FormatZoneJson(json, fieldMap);
        }
        // Se for SaveClientWorld, agrupa posições do player e câmera
        if (isWorld)
        {
            return FormatWorldJson(json, fieldMap);
        }
        // Usa regex para extrair pares chave-valor do JSON
        // Padrão: "chave":"valor" ou "chave":valor
        var matches = Regex.Matches(json, @"""([^""]+)""\s*:\s*(""[^""]*""|\[[^\]]*\]|[^,}]+)");
        List<string> formattedParts = new List<string>();
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value.Trim();
                // Remove aspas se houver
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                // Formata arrays - remove colchetes e aspas
                if (value.StartsWith("[") && value.EndsWith("]"))
                {
                    string arrayContent = value.Substring(1, value.Length - 2);
                    // Remove aspas de cada item
                    arrayContent = arrayContent.Replace("\"", "");
                    if (key == "rules")
                    {
                        value = $"Regras: {arrayContent}";
                    }
                    else
                    {
                        value = arrayContent;
                    }
                }
                string description = GetFieldDescription(key, saveId, fieldMap);
                formattedParts.Add($"{description}: {value}");
            }
        }
        if (formattedParts.Count == 0)
        {
            // Fallback: método simples se regex não funcionar
            string clean = json.Replace("{", "").Replace("}", "").Replace("\"", "");
            clean = clean.Replace(",", "  |  ");
            clean = clean.Replace(":", ": ");
            return clean;
        }
        return string.Join("  |  ", formattedParts);
    }
    string FormatWorldJson(string json, Dictionary<string, string> fieldMap)
    {
        // Extrai todos os campos do JSON (case-insensitive)
        Dictionary<string, string> fields = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        var matches = Regex.Matches(json, @"""([^""]+)""\s*:\s*(""[^""]*""|\[[^\]]*\]|[^,}]+)");
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value.Trim();
                // Remove aspas se houver
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                // Normaliza a chave para minúsculas para comparação
                fields[key.ToLowerInvariant()] = value;
            }
        }
        List<string> formattedParts = new List<string>();
        // Agrupa posição do player
        if (fields.ContainsKey("px") || fields.ContainsKey("py") || fields.ContainsKey("pz"))
        {
            string px = fields.ContainsKey("px") ? fields["px"] : "0";
            string py = fields.ContainsKey("py") ? fields["py"] : "0";
            string pz = fields.ContainsKey("pz") ? fields["pz"] : "0";
            formattedParts.Add($"Posição Player: {px} {py} {pz}");
        }
        // Agrupa posição da câmera
        if (fields.ContainsKey("cx") || fields.ContainsKey("cy") || fields.ContainsKey("cz"))
        {
            string cx = fields.ContainsKey("cx") ? fields["cx"] : "0";
            string cy = fields.ContainsKey("cy") ? fields["cy"] : "0";
            string cz = fields.ContainsKey("cz") ? fields["cz"] : "0";
            formattedParts.Add($"Posição Camera: {cx} {cy} {cz}");
        }
        // Adiciona outros campos
        foreach (var kvp in fields)
        {
            string key = kvp.Key;
            if (key == "px" || key == "py" || key == "pz" || key == "cx" || key == "cy" || key == "cz")
            {
                continue; // Já foram processados acima
            }
            string value = kvp.Value;
            // Formata arrays - remove colchetes e aspas
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                string arrayContent = value.Substring(1, value.Length - 2);
                // Remove aspas de cada item
                arrayContent = arrayContent.Replace("\"", "");
                value = arrayContent;
            }
            string description = GetFieldDescription(key, "World", fieldMap);
            formattedParts.Add($"{description}: {value}");
        }
        return string.Join("  |  ", formattedParts);
    }
    string FormatZoneJson(string json, Dictionary<string, string> fieldMap)
    {
        string[] parts = json.Split(new[] { "|STRING_DATA|" }, System.StringSplitOptions.None);
        List<string> results = new List<string>();
        // Primeira parte: flags inteiras
        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
        {
            string flagsJson = parts[0];
            // Usa regex para extrair keys e values arrays
            var keysMatch = Regex.Match(flagsJson, @"""keys""\s*:\s*\[([^\]]*)\]");
            var valuesMatch = Regex.Match(flagsJson, @"""values""\s*:\s*\[([^\]]*)\]");
            if (keysMatch.Success && valuesMatch.Success)
            {
                string keysStr = keysMatch.Groups[1].Value;
                string valuesStr = valuesMatch.Groups[1].Value;
                // Remove aspas dos IDs
                keysStr = keysStr.Replace("\"", "").Trim();
                valuesStr = valuesStr.Trim();
                // Divide por vírgulas
                string[] keys = string.IsNullOrEmpty(keysStr) ? new string[0] : keysStr.Split(',');
                string[] values = string.IsNullOrEmpty(valuesStr) ? new string[0] : valuesStr.Split(',');
                if (keys.Length > 0 && keys.Length == values.Length)
                {
                    List<string> flagPairs = new List<string>();
                    for (int i = 0; i < keys.Length; i++)
                    {
                    string flagId = keys[i].Trim().Replace("\"", "");
                    string flagValue = values[i].Trim();
                    flagPairs.Add($"{flagId}: {flagValue}");
                }
                results.Add($"Flags de Zona: {string.Join(", ", flagPairs)}");
                }
                else if (keys.Length > 0)
                {
                    string keysDesc = GetFieldDescription("keys", "Zone", fieldMap);
                    string valuesDesc = GetFieldDescription("values", "Zone", fieldMap);
                    results.Add($"{keysDesc}: {keysStr}");
                    results.Add($"{valuesDesc}: {valuesStr}");
                }
            }
        }
        // Segunda parte: flags string
        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
        {
            string stringJson = parts[1];
            var stringKeysMatch = Regex.Match(stringJson, @"""keys""\s*:\s*\[([^\]]*)\]");
            var stringValuesMatch = Regex.Match(stringJson, @"""values""\s*:\s*\[([^\]]*)\]");
            if (stringKeysMatch.Success && stringValuesMatch.Success)
            {
                string keysStr = stringKeysMatch.Groups[1].Value.Replace("\"", "").Trim();
                string valuesStr = stringValuesMatch.Groups[1].Value.Replace("\"", "").Trim();
                string[] keys = string.IsNullOrEmpty(keysStr) ? new string[0] : keysStr.Split(',');
                string[] values = string.IsNullOrEmpty(valuesStr) ? new string[0] : valuesStr.Split(',');
                if (keys.Length > 0 && keys.Length == values.Length)
                {
                    List<string> stringPairs = new List<string>();
                    for (int i = 0; i < keys.Length; i++)
                    {
                    string flagId = keys[i].Trim().Replace("\"", "");
                    string flagValue = values[i].Trim().Replace("\"", "");
                    stringPairs.Add($"{flagId}: {flagValue}");
                }
                results.Add($"Flags String: {string.Join(", ", stringPairs)}");
                }
                else if (keys.Length > 0)
                {
                    results.Add($"Flags String (IDs): {keysStr}");
                    results.Add($"Flags String (Valores): {valuesStr}");
                }
            }
        }
        return results.Count > 0 ? string.Join("\n", results) : json;
    }
    string GetFieldDescription(string fieldName, string saveId, Dictionary<string, string> fieldMap)
    {
        if (fieldMap != null)
        {
            // Tenta encontrar com case-insensitive
            foreach (var kvp in fieldMap)
            {
                if (string.Equals(kvp.Key, fieldName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
        }
        // Fallback: capitaliza o nome do campo
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fieldName);
    }
    void DrawFormattedJson(string json, string saveId)
    {
        string formatted = FormatJson(json, saveId);
        // Divide por "  |  " para obter os elementos
        string[] parts = formatted.Split(new[] { "  |  " }, System.StringSplitOptions.None);
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < parts.Length; i++)
        {
            // Alterna entre branco e cinza
            if (i % 2 == 0)
            {
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.7f, 0.7f, 0.7f); // Cinza
            }
            if (i > 0)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("|", GUILayout.Width(10));
            }
            GUI.color = (i % 2 == 0) ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(parts[i], EditorStyles.wordWrappedLabel);
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
    }
    static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "save.json");
    }
    void DeleteSpecificSave(string idToDelete)
    {
        string path = GetPath();
        if (!File.Exists(path)) return;
        try
        {
            string json = File.ReadAllText(path);
            var file = JsonUtility.FromJson<DebugSaveFile>(json);
            if (file != null && file.entries != null)
            {
                file.entries.RemoveAll(entry => entry.key == idToDelete);
                string finalJson = JsonUtility.ToJson(file, true);
                File.WriteAllText(path, finalJson);
                AssetDatabase.Refresh();
            }
        }
        catch { }
    }
}

