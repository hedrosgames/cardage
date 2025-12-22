using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
public class EditorLocalizationImporter : EditorWindow
{
    private string sheetId = "1HvY8sMGyzV5cVt_kzocyqmI90EF1KOCz5FF7wQZwPOE";
    [System.Serializable]
    public class SheetConfig
    {
        public string name;
        public string gid;
        public SheetConfig(string name, string gid)
        {
            this.name = name;
            this.gid = gid;
        }
    }
    private List<SheetConfig> sheets = new List<SheetConfig>
    {
        new SheetConfig("Menu", "0"),
        new SheetConfig("World", "910185638"),
        new SheetConfig("CardGame", "723334572"),
        new SheetConfig("Dialogues", "1042227826"),
        new SheetConfig("Cards", "258366397"),
        new SheetConfig("Tutorial", "1696503281")
    };
    private SOLocalizationData targetSO;
    private Vector2 scrollPos;
    [MenuItem("Tools/Importar Google Sheet")]
    public static void ShowWindow()
    {
        GetWindow<EditorLocalizationImporter>("Localization Importer");
    }
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.LabelField("Importador Multi-Abas (Cyber Red)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Usa o mÃ©todo '/export' com ID da planilha. Requer permissÃ£o de leitura no link.", MessageType.Info);
        EditorGUILayout.Space();
        targetSO = (SOLocalizationData)EditorGUILayout.ObjectField("Target SO", targetSO, typeof(SOLocalizationData), false);
        sheetId = EditorGUILayout.TextField("ID da Planilha", sheetId);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("ConfiguraÃ§Ã£o das Abas", EditorStyles.boldLabel);
        for (int i = 0; i < sheets.Count; i++)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Nome:", GUILayout.Width(40));
            sheets[i].name = EditorGUILayout.TextField(sheets[i].name, GUILayout.Width(100));
            GUILayout.Label("GID:", GUILayout.Width(30));
            sheets[i].gid = EditorGUILayout.TextField(sheets[i].gid);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                sheets.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("+ Adicionar Nova Aba"))
        {
            sheets.Add(new SheetConfig("Nova Aba", ""));
        }
        EditorGUILayout.Space();
        GUI.enabled = targetSO != null && !string.IsNullOrEmpty(sheetId) && sheets.Count > 0;
        if (GUILayout.Button("IMPORTAR E ORGANIZAR", GUILayout.Height(40)))
        {
            ImportAllSheetsAsync();
        }
        GUI.enabled = true;
        EditorGUILayout.EndScrollView();
    }
    private async void ImportAllSheetsAsync()
    {
        if (targetSO == null) return;
        targetSO.sheets.Clear();
        targetSO.entries_DEPRECATED.Clear();
        int successCount = 0;
        List<SheetConfig> sheetsToProcess = new List<SheetConfig>(sheets);
        foreach (var sheet in sheetsToProcess)
        {
            if (string.IsNullOrWhiteSpace(sheet.gid)) continue;
            string finalUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={sheet.gid}";
            string csvContent = await DownloadCSV(finalUrl);
            if (!string.IsNullOrEmpty(csvContent))
            {
                ProcessSheet(sheet.name, csvContent);
                successCount++;
            }
            else
            {
            }
        }
        if (successCount > 0)
        {
            EditorUtility.SetDirty(targetSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Sucesso", $"ImportaÃ§Ã£o concluÃ­da!\n{successCount} abas organizadas no SO.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Erro", "Nenhuma aba baixada. Verifique o console para detalhes.", "OK");
        }
    }
    private void ProcessSheet(string sheetName, string csvText)
    {
        List<string[]> rows = ParseCSV(csvText);
        if (rows.Count <= 1) return;
        SOLocalizationData.Sheet newSheet = new SOLocalizationData.Sheet();
        newSheet.sheetName = sheetName;
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
        targetSO.sheets.Add(newSheet);
    }
    private async Task<string> DownloadCSV(string url)
    {
        using (var www = UnityWebRequest.Get(url))
        {
            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();
            if (www.result != UnityWebRequest.Result.Success)
            {
                return null;
            }
            return www.downloadHandler.text;
        }
    }
    private List<string[]> ParseCSV(string text)
    {
        List<string[]> rows = new List<string[]>();
        List<string> currentRow = new List<string>();
        StringBuilder currentField = new StringBuilder();
        bool insideQuotes = false;
        for (int i = 0; i < text.Length; i++) { char c = text[i]; if (c == '"') { if (insideQuotes && i + 1 < text.Length && text[i + 1] == '"') { currentField.Append('"'); i++; } else { insideQuotes = !insideQuotes; } } else if (c == ',' && !insideQuotes) { currentRow.Add(currentField.ToString()); currentField.Clear(); } else if ((c == '\r' || c == '\n') && !insideQuotes) { if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++; currentRow.Add(currentField.ToString()); rows.Add(currentRow.ToArray()); currentRow.Clear(); currentField.Clear(); } else { currentField.Append(c); } }
        if (currentField.Length > 0 || currentRow.Count > 0) { currentRow.Add(currentField.ToString()); rows.Add(currentRow.ToArray()); }
        return rows;
    }
}

