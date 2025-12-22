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
        public string uniqueKey;
        public bool isChecked = false;
    }
    [MenuItem("Central de Configuração/Central de Diálogos")]
    public static void ShowWindow()
    {
        GetWindow<EditorDialogueWindow>("Central de Diálogos");
    }
    private void OnEnable()
    {
        RefreshDialogueList();
    }
    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Central de Diálogos", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Atualizar Lista", GUILayout.Width(120)))
        {
            RefreshDialogueList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (allDialogues.Count == 0)
        {
            EditorGUILayout.HelpBox("Nenhum diálogo encontrado no projeto. Crie diálogos usando: Create > Game > Dialogue", MessageType.Info);
            return;
        }
        EditorGUILayout.HelpBox($"Total de linhas de diálogo: {allDialogues.Count}", MessageType.Info);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Nome do SO", EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("Text", EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("Speaker", EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("✓", EditorStyles.boldLabel, GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
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
        Color originalColor = GUI.backgroundColor;
        if (entry.isChecked)
        {
            GUI.backgroundColor = Color.green;
        }
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField(entry.soName, GUILayout.Width(200));
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginChangeCheck();
        string newText = EditorGUILayout.TextField(entry.line.text, GUILayout.Width(200));
        bool textChanged = EditorGUI.EndChangeCheck();
        if (textChanged)
        {
            entry.line.text = newText;
            entry.isChecked = false;
            SaveCheckboxState(entry);
            MarkDirty(entry.sequence);
        }
        EditorGUI.BeginChangeCheck();
        string newSpeaker = EditorGUILayout.TextField(entry.line.speaker, GUILayout.Width(200));
        bool speakerChanged = EditorGUI.EndChangeCheck();
        if (speakerChanged)
        {
            entry.line.speaker = newSpeaker;
            entry.isChecked = false;
            SaveCheckboxState(entry);
            MarkDirty(entry.sequence);
        }
        EditorGUI.BeginChangeCheck();
        entry.isChecked = EditorGUILayout.Toggle(entry.isChecked, GUILayout.Width(30));
        if (EditorGUI.EndChangeCheck())
        {
            SaveCheckboxState(entry);
        }
        EditorGUILayout.EndHorizontal();
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
                        string uniqueKey = guid + "_" + i;
                        var entry = new DialogueEntry
                        {
                            sequence = sequence,
                            line = line,
                            lineIndex = i,
                            soName = soName,
                            uniqueKey = uniqueKey
                        };
                        entry.isChecked = LoadCheckboxState(uniqueKey);
                        allDialogues.Add(entry);
                    }
                }
            }
        }
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
        return Regex.Replace(str.ToLowerInvariant(), @"\d+", match =>
        {
            int number = int.Parse(match.Value);
            return number.ToString("D4");
        });
    }
}
