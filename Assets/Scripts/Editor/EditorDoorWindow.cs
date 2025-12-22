using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using Game.World;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public class EditorDoorWindow : EditorWindow
{
    // --- ESTADO GERAL ---
    private int currentTab = 0;
    private string[] tabNames = { "Door List", "Enum Editor (WorldAreaId)" };

    // --- VARIÃVEIS DA ABA DOOR LIST ---
    private Vector2 scrollPosDoors;
    private List<TeleportDoor> doors = new List<TeleportDoor>();
    private SOCameraConfig cameraConfig;

    // Cache para os nomes "bonitos" (sem underline)
    private string[] cleanEnumNames;
    private int[] enumValuesArray;

    // --- VARIÃVEIS DA ABA ENUM EDITOR ---
    private Vector2 scrollPosEnum;
    private List<string> enumValues = new List<string>();
    private string newEnumName = "";
    private string enumFilePath = "";
    private bool enumFileFound = false;
    private ReorderableList reorderableList;

    [MenuItem("Central de Configuração/Central de Door")]
    public static void ShowWindow()
    {
        GetWindow<EditorDoorWindow>("Door Manager");
    }

    private void OnEnable()
    {
        RefreshDoorList();
        FindEnumFile();
        FindCameraConfig();
        UpdateEnumDisplayCache(); // Gera os nomes sem underline
        
        if (enumFileFound) LoadEnumValues();
        if (enumValues != null) InitReorderableList();
    }

    // --- NOVO MÉTODO: Cria a lista de nomes legíveis ---
    private void UpdateEnumDisplayCache()
    {
        var values = System.Enum.GetValues(typeof(WorldAreaId));
        cleanEnumNames = new string[values.Length];
        enumValuesArray = new int[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            // Pega o valor original (ex: City1_Store)
            string rawName = System.Enum.GetName(typeof(WorldAreaId), values.GetValue(i));
            // Substitui underline por espaço para visualização
            cleanEnumNames[i] = rawName.Replace("_", " ");
            // Guarda o valor inteiro correspondente
            enumValuesArray[i] = (int)values.GetValue(i);
        }
    }

    private void FindCameraConfig()
    {
        if (cameraConfig != null) return;
        var manager = FindFirstObjectByType<ManagerCamera>();
        if (manager != null && manager.config != null)
        {
            cameraConfig = manager.config;
        }
    }

    // ===================================================================================
    // LISTA REORDENÃVEL (ENUM)
    // ===================================================================================
    private void InitReorderableList()
    {
        reorderableList = new ReorderableList(enumValues, typeof(string), true, true, false, false);

        reorderableList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "IDs (Ocultando 'None')");

        reorderableList.elementHeightCallback = (int index) =>
        {
            float defaultHeight = EditorGUIUtility.singleLineHeight + 4;
            float separatorHeight = 20f;
            if (index == 0) return IsCityGroup(index) ? defaultHeight + separatorHeight : defaultHeight;
            
            string currentGroup = GetGroupName(index);
            string prevGroup = GetGroupName(index - 1);
            return (currentGroup != prevGroup) ? defaultHeight + separatorHeight : defaultHeight;
        };

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            if (index >= enumValues.Count) return;

            string currentGroup = GetGroupName(index);
            string prevGroup = (index > 0) ? GetGroupName(index - 1) : "";
            
            if (currentGroup != prevGroup || (index == 0 && IsCityGroup(index)))
            {
                Rect labelHeaderRect = new Rect(rect.x, rect.y, rect.width, 18);
                EditorGUI.DrawRect(labelHeaderRect, new Color(0.2f, 0.2f, 0.2f));
                EditorGUI.LabelField(labelHeaderRect, currentGroup, EditorStyles.boldLabel);
                rect.y += 20; 
            }

            string val = enumValues[index];
            Rect labelRect = new Rect(rect.x, rect.y + 2, 30, EditorGUIUtility.singleLineHeight);
            Rect textRect = new Rect(rect.x + 35, rect.y + 2, rect.width - 70, EditorGUIUtility.singleLineHeight);
            Rect btnRect = new Rect(rect.x + rect.width - 30, rect.y + 2, 30, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(labelRect, $"{index + 1}.");

            GUIStyle coloredTextField = new GUIStyle(EditorStyles.textField);
            if (val.Contains("_ext_")) coloredTextField.normal.textColor = new Color(0.8f, 0.4f, 1f); 
            else coloredTextField.normal.textColor = new Color(1f, 0.6f, 0.1f);
            coloredTextField.focused.textColor = coloredTextField.normal.textColor;

            EditorGUI.BeginChangeCheck();
            string newVal = EditorGUI.TextField(textRect, val, coloredTextField);
            if (EditorGUI.EndChangeCheck()) enumValues[index] = newVal;

            if (GUI.Button(btnRect, "X"))
            {
                int i = index;
                EditorApplication.delayCall += () => RemoveEnumEntry(i);
            }
        };
    }

    private string GetGroupName(int index)
    {
        Match match = Regex.Match(enumValues[index], @"^City(\d+)_");
        return match.Success ? "City " + match.Groups[1].Value : "Outros / Genéricos";
    }
    private bool IsCityGroup(int index) => Regex.IsMatch(enumValues[index], @"^City(\d+)_");

    // ===================================================================================
    // GUI PRINCIPAL
    // ===================================================================================
    private void OnGUI()
    {
        GUILayout.Space(10);
        currentTab = GUILayout.Toolbar(currentTab, tabNames, GUILayout.Height(30));
        GUILayout.Space(10);

        if (currentTab == 0) DrawDoorListTab();
        else DrawEnumEditorTab();
    }

    // ===================================================================================
    // ABA 1: DOOR LIST (ATUALIZADA)
    // ===================================================================================
    private void DrawDoorListTab()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Refresh List", EditorStyles.toolbarButton)) 
        {
            RefreshDoorList();
            UpdateEnumDisplayCache(); // Atualiza nomes se houve recompilação
        }
        
        EditorGUILayout.LabelField("| Config:", GUILayout.Width(60));
        cameraConfig = (SOCameraConfig)EditorGUILayout.ObjectField(cameraConfig, typeof(SOCameraConfig), false, GUILayout.Width(200));
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (doors.Count == 0)
        {
            EditorGUILayout.HelpBox("No TeleportDoor components found.", MessageType.Info);
            return;
        }

        // Cabeçalho da Tabela
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Parent", EditorStyles.boldLabel, GUILayout.Width(100));
        EditorGUILayout.LabelField("Object", EditorStyles.boldLabel, GUILayout.Width(100));
        EditorGUILayout.LabelField("ID", EditorStyles.boldLabel, GUILayout.Width(140));
        EditorGUILayout.LabelField("Destination", EditorStyles.boldLabel, GUILayout.Width(140));
        EditorGUILayout.LabelField("Multi", EditorStyles.boldLabel, GUILayout.Width(40));
        EditorGUILayout.LabelField("Action", EditorStyles.boldLabel, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        scrollPosDoors = EditorGUILayout.BeginScrollView(scrollPosDoors);

        foreach (var door in doors)
        {
            if (door == null) continue;

            EditorGUILayout.BeginHorizontal("box");
            string parentName = door.transform.parent != null ? door.transform.parent.name : "---";
            EditorGUILayout.LabelField(parentName, GUILayout.Width(100));
            EditorGUILayout.ObjectField(door.gameObject, typeof(GameObject), true, GUILayout.Width(100));

            EditorGUI.BeginChangeCheck();

            // --- AQUI ESTÃ A MUDANÇA ---
            // Em vez de EnumPopup, usamos IntPopup com os nomes limpos
            
            // 1. Identification
            int idVal = EditorGUILayout.IntPopup((int)door.identification, cleanEnumNames, enumValuesArray, GUILayout.Width(140));
            WorldAreaId newId = (WorldAreaId)idVal;

            // 2. Destination
            int destVal = EditorGUILayout.IntPopup((int)door.destination, cleanEnumNames, enumValuesArray, GUILayout.Width(140));
            WorldAreaId newDest = (WorldAreaId)destVal;
            // ---------------------------

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(door, "Change Door Settings");
                door.identification = newId;
                door.destination = newDest;
                EditorUtility.SetDirty(door);
            }

            bool hasMultiLogic = door.multiDestination && door.conditionalDestinations != null && door.conditionalDestinations.Count > 0;
            if (hasMultiLogic) { GUI.color = Color.green; GUILayout.Label("YES", EditorStyles.miniBoldLabel, GUILayout.Width(40)); }
            else { GUI.color = new Color(1f, 0.4f, 0.4f); GUILayout.Label("NO", EditorStyles.miniLabel, GUILayout.Width(40)); }
            GUI.color = Color.white;

            if (cameraConfig != null && door.identification != WorldAreaId.None)
            {
                if (GUILayout.Button("ðŸ“· Save Cam", GUILayout.Width(100)))
                {
                    SaveCameraPositionToConfig(door.identification);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("-", GUILayout.Width(100));
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void SaveCameraPositionToConfig(WorldAreaId id)
    {
        if (SceneView.lastActiveSceneView == null)
        {
            EditorUtility.DisplayDialog("Erro", "Nenhuma Scene View ativa para capturar a câmera.", "OK");
            return;
        }

        Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
        Undo.RecordObject(cameraConfig, "Update Camera Position");

        bool found = false;
        if (cameraConfig.areas == null) cameraConfig.areas = new SOCameraConfig.AreaSettings[0];

        for (int i = 0; i < cameraConfig.areas.Length; i++)
        {
            if (cameraConfig.areas[i].id == id)
            {
                cameraConfig.areas[i].fixedPosition = camPos;
                cameraConfig.areas[i].followPlayer = false; 
                found = true;
                break;
            }
        }

        if (!found)
        {
            List<SOCameraConfig.AreaSettings> list = new List<SOCameraConfig.AreaSettings>(cameraConfig.areas);
            SOCameraConfig.AreaSettings newArea = new SOCameraConfig.AreaSettings();
            newArea.id = id;
            newArea.fixedPosition = camPos;
            newArea.followPlayer = false;
            newArea.name = id.ToString();
            
            list.Add(newArea);
            cameraConfig.areas = list.ToArray();
        }

        EditorUtility.SetDirty(cameraConfig);
        Debug.Log($"<color=green>[Door Manager]</color> Posição de Câmera salva para <b>{id}</b> em: {camPos}");
    }

    private void RefreshDoorList()
    {
        doors.Clear();
        TeleportDoor[] found = FindObjectsByType<TeleportDoor>(FindObjectsSortMode.None);
        System.Array.Sort(found, (a, b) => {
            string pA = a.transform.parent != null ? a.transform.parent.name : "";
            string pB = b.transform.parent != null ? b.transform.parent.name : "";
            return pA.CompareTo(pB);
        });
        doors.AddRange(found);
    }

    // ===================================================================================
    // ABA 2: ENUM EDITOR
    // ===================================================================================
    private void DrawEnumEditorTab()
    {
        if (!enumFileFound)
        {
            EditorGUILayout.HelpBox("Arquivo 'WorldAreaId.cs' não encontrado.", MessageType.Error);
            if (GUILayout.Button("Tentar Localizar Novamente")) FindEnumFile();
            return;
        }

        EditorGUILayout.LabelField($"Editando: {enumFilePath}", EditorStyles.miniLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("Novo ID:", GUILayout.Width(60));
        newEnumName = EditorGUILayout.TextField(newEnumName);
        if (GUILayout.Button("Adicionar", GUILayout.Width(80))) AddEnumEntry();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        scrollPosEnum = EditorGUILayout.BeginScrollView(scrollPosEnum);
        if (reorderableList != null) reorderableList.DoLayoutList();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Ao salvar, a ordem que você definiu acima será respeitada.", MessageType.Info);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("SALVAR ORDEM EXATA E RECOMPILAR", GUILayout.Height(40))) SaveEnumFile();
        GUI.backgroundColor = Color.white;
    }

    private void FindEnumFile()
    {
        string[] guids = AssetDatabase.FindAssets("WorldAreaId t:MonoScript");
        if (guids.Length > 0)
        {
            enumFilePath = AssetDatabase.GUIDToAssetPath(guids[0]);
            enumFileFound = true;
        }
        else
        {
            string manualPath = "Assets/Scripts/World/WorldAreaId.cs";
            if (File.Exists(manualPath)) { enumFilePath = manualPath; enumFileFound = true; }
            else enumFileFound = false;
        }
    }

    private void LoadEnumValues()
    {
        if (!File.Exists(enumFilePath)) return;
        string content = File.ReadAllText(enumFilePath);
        Match match = Regex.Match(content, @"public\s+enum\s+WorldAreaId\s*\{([^}]*)\}");
        if (match.Success)
        {
            string innerContent = match.Groups[1].Value;
            enumValues = innerContent.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            enumValues.RemoveAll(x => x == "None");
            InitReorderableList(); 
        }
    }

    private void AddEnumEntry()
    {
        if (string.IsNullOrEmpty(newEnumName)) return;
        if (!Regex.IsMatch(newEnumName, @"^[a-zA-Z_][a-zA-Z0-9_]*$")) { EditorUtility.DisplayDialog("Erro", "Nome inválido.", "OK"); return; }
        if (enumValues.Contains(newEnumName) || newEnumName == "None") { EditorUtility.DisplayDialog("Erro", "ID já existe.", "OK"); return; }
        enumValues.Add(newEnumName);
        newEnumName = "";
        GUI.FocusControl(null);
    }

    private void RemoveEnumEntry(int index) { if (index >= 0 && index < enumValues.Count) enumValues.RemoveAt(index); }

    private void SaveEnumFile()
    {
        if (!File.Exists(enumFilePath)) return;
        List<string> saveList = new List<string> { "None" };
        saveList.AddRange(enumValues);

        string originalContent = File.ReadAllText(enumFilePath);
        string newEnumBlock = "\n\t{\n";
        for (int i = 0; i < saveList.Count; i++)
        {
            newEnumBlock += $"\t\t{saveList[i]}";
            if (i < saveList.Count - 1) newEnumBlock += ",";
            newEnumBlock += "\n";
        }
        newEnumBlock += "\t}";

        string newFileContent = Regex.Replace(originalContent, @"public\s+enum\s+WorldAreaId\s*\{[^}]*\}", $"public enum WorldAreaId{newEnumBlock}");
        File.WriteAllText(enumFilePath, newFileContent);
        LoadEnumValues(); 
        AssetDatabase.Refresh();
    }
}
