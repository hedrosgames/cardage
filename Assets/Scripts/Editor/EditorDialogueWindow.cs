using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class EditorDialogueWindow : EditorWindow
{
    private List<DialogueEntry> allDialogues = new List<DialogueEntry>();
    private Vector2 scrollPos;
    private const string EDITOR_PREFS_KEY_PREFIX = "DialogueEditor_Checked_";

    [System.Serializable]
    private class DialogueEntry
    {
        public SODialogueSequence sequence;
        public SODialogueSequence.DialogueLine line;
        public int lineIndex;
        public string soName;
        public string uniqueKey; // Chave Ãºnica para identificar este diÃ¡logo
        public bool isChecked = false;
    }

    [MenuItem("Central de ConfiguraÃ§Ã£o/Central de DiÃ¡logos")]
    public static void ShowWindow()
    {
        GetWindow<EditorDialogueWindow>("Central de DiÃ¡logos");
    }

    private void OnEnable()
    {
        RefreshDialogueList();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Central de DiÃ¡logos", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // BotÃ£o de refresh
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Atualizar Lista", GUILayout.Width(120)))
        {
            RefreshDialogueList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // InformaÃ§Ãµes
        if (allDialogues.Count == 0)
        {
            EditorGUILayout.HelpBox("Nenhum diÃ¡logo encontrado no projeto. Crie diÃ¡logos usando: Create > Game > Dialogue", MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox($"Total de linhas de diÃ¡logo: {allDialogues.Count}", MessageType.Info);
        EditorGUILayout.Space();

        // CabeÃ§alho da tabela
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Nome do SO", EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("Text", EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("Speaker", EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("âœ“", EditorStyles.boldLabel, GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        // Lista scrollÃ¡vel
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var entry in allDialogues)
        {
            DrawDialogueRow(entry);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawDialogueRow(DialogueEntry entry)
    {
        if (entry == null || entry.sequence == null || entry.line == null) return;

        // Salvar cor original
        Color originalColor = GUI.backgroundColor;

        // Se o checkbox estÃ¡ marcado, pintar de verde
        if (entry.isChecked)
        {
            GUI.backgroundColor = Color.green;
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        // Nome do SO (nÃ£o editÃ¡vel)
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField(entry.soName, GUILayout.Width(200));
        EditorGUI.EndDisabledGroup();

        // Text (editÃ¡vel)
        EditorGUI.BeginChangeCheck();
        string newText = EditorGUILayout.TextField(entry.line.text, GUILayout.Width(200));
        bool textChanged = EditorGUI.EndChangeCheck();
        if (textChanged)
        {
            entry.line.text = newText;
            entry.isChecked = false; // Desmarca quando hÃ¡ alteraÃ§Ã£o
            SaveCheckboxState(entry); // Salvar estado (false)
            MarkDirty(entry.sequence);
        }

        // Speaker (editÃ¡vel)
        EditorGUI.BeginChangeCheck();
        string newSpeaker = EditorGUILayout.TextField(entry.line.speaker, GUILayout.Width(200));
        bool speakerChanged = EditorGUI.EndChangeCheck();
        if (speakerChanged)
        {
            entry.line.speaker = newSpeaker;
            entry.isChecked = false; // Desmarca quando hÃ¡ alteraÃ§Ã£o
            SaveCheckboxState(entry); // Salvar estado (false)
            MarkDirty(entry.sequence);
        }

        // Checkbox no final
        EditorGUI.BeginChangeCheck();
        entry.isChecked = EditorGUILayout.Toggle(entry.isChecked, GUILayout.Width(30));
        if (EditorGUI.EndChangeCheck())
        {
            // Salvar estado do checkbox quando alterado
            SaveCheckboxState(entry);
        }

        EditorGUILayout.EndHorizontal();

        // Restaurar cor original
        GUI.backgroundColor = originalColor;
    }

    private void MarkDirty(SODialogueSequence sequence)
    {
        if (sequence != null)
        {
            EditorUtility.SetDirty(sequence);
            AssetDatabase.SaveAssets();
        }
    }

    private void RefreshDialogueList()
    {
        allDialogues.Clear();
        string[] guids = AssetDatabase.FindAssets("t:SODialogueSequence");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SODialogueSequence sequence = AssetDatabase.LoadAssetAtPath<SODialogueSequence>(path);
            if (sequence != null && sequence.lines != null)
            {
                string soName = sequence.name;
                for (int i = 0; i < sequence.lines.Length; i++)
                {
                    var line = sequence.lines[i];
                    if (line != null)
                    {
                        // Criar chave Ãºnica: GUID do SO + Ã­ndice da linha
                        string uniqueKey = guid + "_" + i;
                        
                        var entry = new DialogueEntry
                        {
                            sequence = sequence,
                            line = line,
                            lineIndex = i,
                            soName = soName,
                            uniqueKey = uniqueKey
                        };
                        
                        // Carregar estado salvo do checkbox
                        entry.isChecked = LoadCheckboxState(uniqueKey);
                        
                        allDialogues.Add(entry);
                    }
                }
            }
        }

        // Ordenar alfabeticamente por text com ordenaÃ§Ã£o natural (considera nÃºmeros corretamente)
        allDialogues = allDialogues.OrderBy(d => NaturalCompareKey(d.line.text ?? "")).ToList();
    }
    
    private void SaveCheckboxState(DialogueEntry entry)
    {
        if (entry != null && !string.IsNullOrEmpty(entry.uniqueKey))
        {
            EditorPrefs.SetBool(EDITOR_PREFS_KEY_PREFIX + entry.uniqueKey, entry.isChecked);
        }
    }
    
    private bool LoadCheckboxState(string uniqueKey)
    {
        if (string.IsNullOrEmpty(uniqueKey))
            return false;
            
        return EditorPrefs.GetBool(EDITOR_PREFS_KEY_PREFIX + uniqueKey, false);
    }

    private string NaturalCompareKey(string str)
    {
        if (string.IsNullOrEmpty(str))
            return "";

        // Converte para minÃºsculas e adiciona padding zero para nÃºmeros de atÃ© 4 dÃ­gitos
        return Regex.Replace(str.ToLowerInvariant(), @"\d+", match =>
        {
            int number = int.Parse(match.Value);
            return number.ToString("D4"); // Preenche com zeros Ã  esquerda atÃ© 4 dÃ­gitos
        });
    }
}

