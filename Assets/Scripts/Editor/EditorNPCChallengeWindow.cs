using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
public class EditorNPCChallengeWindow : EditorWindow
{
    private SaveClientZone saveZone;
    private SOZoneFlag challengedNpcsFlag;
    private Vector2 scrollPosition;
    private bool autoRefresh = true;
    private List<SOZoneFlag> npcFlags = new List<SOZoneFlag>();
    private int selectedFlagIndex = -1;
    private List<NPCInfo> npcList = new List<NPCInfo>();
    private class NPCInfo
    {
        public GameObject gameObject;
        public InteractableCardGame interactable;
        public string npcId;
        public bool hasChallenged;
        public SOGameSetup gameSetup;
    }
    [MenuItem("Central de Configuração/Central de NPCs Desafiados")]
    public static void ShowWindow()
    {
        EditorNPCChallengeWindow window = GetWindow<EditorNPCChallengeWindow>("Central de NPCs");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }
    void OnEnable()
    {
        FindSaveZone();
        RefreshFlagList();
        RefreshNPCList();
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("Central de NPCs", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Flag de NPCs Desafiados:", GUILayout.Width(180));
        if (challengedNpcsFlag != null)
        {
            int currentIndex = npcFlags.IndexOf(challengedNpcsFlag);
            if (currentIndex != selectedFlagIndex)
            {
                selectedFlagIndex = currentIndex;
            }
        }
        else if (selectedFlagIndex >= 0)
        {
            selectedFlagIndex = -1;
        }
        string[] flagNames = npcFlags.Select(f => f != null ? f.name : "None").ToArray();
        if (flagNames.Length == 0)
        {
            flagNames = new string[] { "Nenhuma flag encontrada" };
        }
        int newIndex = EditorGUILayout.Popup(selectedFlagIndex >= 0 ? selectedFlagIndex : 0, flagNames);
        if (newIndex != selectedFlagIndex && newIndex >= 0 && newIndex < npcFlags.Count)
        {
            selectedFlagIndex = newIndex;
            challengedNpcsFlag = npcFlags[newIndex];
            RefreshNPCList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Atualizar Lista de NPCs"))
        {
            RefreshNPCList();
        }
        autoRefresh = EditorGUILayout.Toggle("Auto-refresh", autoRefresh);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (challengedNpcsFlag == null)
        {
            EditorGUILayout.HelpBox("Selecione uma flag para visualizar os NPCs.", MessageType.Info);
            return;
        }
        if (saveZone == null)
        {
            EditorGUILayout.HelpBox("SaveClientZone não encontrado na cena. Execute o jogo para visualizar os dados.", MessageType.Warning);
            if (GUILayout.Button("Procurar SaveClientZone"))
            {
                FindSaveZone();
            }
            return;
        }
        int challengedCount = npcList.Count(n => n.hasChallenged);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"NPCs na cena: {npcList.Count} | Desafiados: {challengedCount}", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lista de NPCs:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (npcList.Count == 0)
        {
            EditorGUILayout.HelpBox("Nenhum NPC encontrado na cena com tag 'NPC' e InteractableCardGame configurado.", MessageType.Info);
        }
        else
        {
            foreach (var npc in npcList)
            {
                DrawNPCRow(npc);
            }
        }
        EditorGUILayout.EndScrollView();
    }
    void DrawNPCRow(NPCInfo npc)
    {
        if (npc == null || npc.gameObject == null || npc.interactable == null) return;
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("NPC:", GUILayout.Width(50));
        EditorGUILayout.LabelField(npc.gameObject.name, EditorStyles.boldLabel);
        if (GUILayout.Button("Selecionar", GUILayout.Width(80)))
        {
            Selection.activeGameObject = npc.gameObject;
            EditorGUIUtility.PingObject(npc.gameObject);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ID:", GUILayout.Width(50));
        EditorGUILayout.LabelField(string.IsNullOrEmpty(npc.npcId) ? "<sem ID>" : npc.npcId, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Já Desafiou:", GUILayout.Width(100));
        Color originalColor = GUI.color;
        GUI.color = npc.hasChallenged ? Color.green : Color.red;
        EditorGUILayout.Toggle(npc.hasChallenged, GUILayout.Width(20));
        GUI.color = originalColor;
        EditorGUILayout.LabelField(npc.hasChallenged ? "SIM" : "NÃO", GUILayout.Width(50));
        if (Application.isPlaying && npc.hasChallenged && GUILayout.Button("Remover", GUILayout.Width(70)))
        {
            RemoveNpcId(npc.npcId);
            RefreshNPCList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Game Setup:", GUILayout.Width(100));
        SOGameSetup newSetup = (SOGameSetup)EditorGUILayout.ObjectField(npc.gameSetup, typeof(SOGameSetup), false);
        if (newSetup != npc.gameSetup && npc.interactable != null)
        {
            npc.interactable.gameSetup = newSetup;
            npc.gameSetup = newSetup;
            EditorUtility.SetDirty(npc.interactable);
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(npc.gameObject);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    void RemoveNpcId(string id)
    {
        if (saveZone == null || challengedNpcsFlag == null || string.IsNullOrEmpty(id)) return;
        string currentValue = saveZone.GetFlagString(challengedNpcsFlag);
        if (string.IsNullOrEmpty(currentValue)) return;
        List<string> ids = new List<string>(currentValue.Split(','));
        ids.RemoveAll(s => s.Trim() == id);
        string newValue = string.Join(",", ids);
        saveZone.SetFlagString(challengedNpcsFlag, newValue);
        RefreshNPCList();
    }
    void RefreshFlagList()
    {
        FindSaveZone();
        npcFlags.Clear();
        string[] guids = AssetDatabase.FindAssets("t:SOZoneFlag");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SOZoneFlag flag = AssetDatabase.LoadAssetAtPath<SOZoneFlag>(path);
            if (flag != null && flag.name.StartsWith("Flag_NPC_"))
            {
                npcFlags.Add(flag);
            }
        }
        npcFlags = npcFlags.OrderBy(f => f.name).ToList();
        if (challengedNpcsFlag != null)
        {
            selectedFlagIndex = npcFlags.IndexOf(challengedNpcsFlag);
        }
        Repaint();
    }
    void RefreshNPCList()
    {
        npcList.Clear();
        if (challengedNpcsFlag == null || saveZone == null) return;
        GameObject[] npcObjects = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject npcObj in npcObjects)
        {
            InteractableCardGame interactable = npcObj.GetComponent<InteractableCardGame>();
            if (interactable == null) continue;
            if (interactable.challengedNpcsFlag != challengedNpcsFlag) continue;
            NPCInfo info = new NPCInfo
            {
                gameObject = npcObj,
                interactable = interactable,
                npcId = interactable.npcId,
                gameSetup = interactable.gameSetup
            };
            if (!string.IsNullOrEmpty(info.npcId))
            {
                info.hasChallenged = saveZone.HasIdInFlag(challengedNpcsFlag, info.npcId);
            }
            npcList.Add(info);
        }
        npcList = npcList.OrderBy(n => n.gameObject.name).ToList();
        Repaint();
    }
    void FindSaveZone()
    {
        saveZone = Object.FindFirstObjectByType<SaveClientZone>();
    }
    void Update()
    {
        if (autoRefresh && Application.isPlaying)
        {
            RefreshNPCList();
        }
    }
}
