using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

public class EditorCentralSave : EditorWindow
{
    Vector2 scroll;
    Dictionary<string, string> lastSaveResult = new Dictionary<string, string>();
    Dictionary<string, string> lastLoadResult = new Dictionary<string, string>();
    string lastSaveCaller = "";
    string lastLoadCaller = "";
    string selectedFilter = ""; // ID do save selecionado para filtrar
    
    // Mapeamento automático: caller -> lista de saves afetados
    static Dictionary<string, HashSet<string>> callerToSaveIds = new Dictionary<string, HashSet<string>>();
    
    [System.Serializable]
    class SaveLocation
    {
        public string name;
        public string description;
        public string className;
        public string methodName;
        public bool isSave;
        public List<string> affectedSaveIds = new List<string>(); 
        public string personalNote = ""; // Observação pessoal do usuário
    }
    
    Dictionary<string, string> personalNotes = new Dictionary<string, string>();
    
    static List<SaveLocation> saveLocations = new List<SaveLocation>
    {
        // Save locations
        new SaveLocation { name = "SaveClientZone - SetFlag", description = "Quando uma flag de zona é definida", className = "SaveClientZone", methodName = "SetFlag", isSave = true },
        new SaveLocation { name = "SaveClientZone - SetFlagString", description = "Quando uma flag string de zona é definida", className = "SaveClientZone", methodName = "SetFlagString", isSave = true },
        new SaveLocation { name = "PhoneApp_Drive - OnAppOpen", description = "Quando o app Drive é aberto pela primeira vez", className = "PhoneApp_Drive", methodName = "OnAppOpen", isSave = true },
        new SaveLocation { name = "PhoneApp_Zap - OnAppOpen", description = "Quando o app Zap é aberto", className = "PhoneApp_Zap", methodName = "OnAppOpen", isSave = true },
        new SaveLocation { name = "PhoneApp_Bank - OnAppOpen", description = "Quando o app Bank é aberto", className = "PhoneApp_Bank", methodName = "OnAppOpen", isSave = true },
        new SaveLocation { name = "ManagerGameFlow - OnAreaChanged", description = "Quando a área do jogo muda", className = "ManagerGameFlow", methodName = "OnAreaChanged", isSave = true },
        new SaveLocation { name = "ManagerGameFlow - OnCardGameFinished", description = "Quando um card game termina", className = "ManagerGameFlow", methodName = "OnCardGameFinished", isSave = true },
        new SaveLocation { name = "Painel de Quit", description = "Quando o jogador ganha o achievement ele salva o quit e o achiev1", className = "ManagerQuitLogic", methodName = "OnQuit", isSave = true },
        new SaveLocation { name = "SliderSaveHandler - OnPointerUp", description = "Quando um slider é ajustado", className = "SliderSaveHandler", methodName = "OnPointerUp", isSave = true },
        new SaveLocation { name = "ManagerLocalization - SetLanguage", description = "Quando o idioma é alterado", className = "ManagerLocalization", methodName = "SetLanguage", isSave = true },
        new SaveLocation { name = "SaveClientAchievements - Unlock", description = "Quando uma conquista é desbloqueada", className = "SaveClientAchievements", methodName = "Unlock", isSave = true },
        
        // Load locations
        new SaveLocation { name = "SaveClientZone - Start", description = "Quando SaveClientZone inicia", className = "SaveClientZone", methodName = "Start", isSave = false },
        new SaveLocation { name = "SaveClientAchievements - Start", description = "Quando SaveClientAchievements inicia", className = "SaveClientAchievements", methodName = "Start", isSave = false },
        new SaveLocation { name = "Painel de Quit", description = "Quando ManagerQuitLogic inicia", className = "ManagerQuitLogic", methodName = "Start", isSave = false },
        new SaveLocation { name = "ManagerSave - Awake", description = "Quando ManagerSave inicia", className = "ManagerSave", methodName = "Awake", isSave = false },
        new SaveLocation { name = "ManagerSave - OnSceneLoaded", description = "Quando uma cena é carregada", className = "ManagerSave", methodName = "OnSceneLoaded", isSave = false },
    };
    
    [MenuItem("Central de Configuração/Central de Save")]
    static void Open()
    {
        var window = GetWindow<EditorCentralSave>("Central de Save");
        window.DetectSaveMappings();
        window.LoadPersonalNotes();
    }
    
    void OnEnable()
    {
        SaveEvents.OnSaveExecuted += OnSaveExecuted;
        SaveEvents.OnLoadExecuted += OnLoadExecuted;
        DetectSaveMappings();
        LoadPersonalNotes();
    }
    
    void LoadPersonalNotes()
    {
        // Carrega observações pessoais salvas
        personalNotes.Clear();
        foreach (var location in saveLocations)
        {
            string key = $"EditorCentralSave_Note_{location.className}.{location.methodName}";
            string savedNote = UnityEditor.EditorPrefs.GetString(key, "");
            if (!string.IsNullOrEmpty(savedNote))
            {
                personalNotes[location.className + "." + location.methodName] = savedNote;
                location.personalNote = savedNote;
            }
        }
    }
    
    void SavePersonalNote(string locationKey, string note)
    {
        // Salva observação pessoal
        personalNotes[locationKey] = note;
        UnityEditor.EditorPrefs.SetString($"EditorCentralSave_Note_{locationKey}", note);
    }
    
    void DetectSaveMappings()
    {
        // Mapeamento estático baseado no conhecimento do código
        // Isso é mais confiável que análise dinâmica
        Dictionary<string, List<string>> staticMappings = new Dictionary<string, List<string>>
        {
            // Save locations
            { "SaveClientZone.SetFlag", new List<string> { "savezone" } },
            { "SaveClientZone.SetFlagString", new List<string> { "savezone" } },
            { "PhoneApp_Drive.OnAppOpen", new List<string> { "savezone" } },
            { "PhoneApp_Zap.OnAppOpen", new List<string> { "savezone" } },
            { "PhoneApp_Bank.OnAppOpen", new List<string> { "savezone" } },
            { "ManagerGameFlow.OnAreaChanged", new List<string> { "saveworld" } },
            { "ManagerGameFlow.OnCardGameFinished", new List<string> { "saveworld", "savecard" } },
            { "ManagerQuitLogic.OnQuit", new List<string> { "savequit", "saveachievements" } },
            { "SliderSaveHandler.OnPointerUp", new List<string> { "savesettings" } },
            { "ManagerLocalization.SetLanguage", new List<string> { "savesettings" } },
            { "SaveClientAchievements.Unlock", new List<string> { "saveachievements" } },
            
            // Load locations
            { "SaveClientZone.Start", new List<string> { "savezone" } },
            { "SaveClientAchievements.Start", new List<string> { "saveachievements" } },
            { "ManagerQuitLogic.Start", new List<string> { "savequit" } },
            { "ManagerSave.Awake", new List<string> { "saveworld", "savezone", "savecard", "savesettings", "savemenu", "saveachievements" } },
            { "ManagerSave.OnSceneLoaded", new List<string> { "saveworld", "savezone", "savecard", "savesettings", "savemenu", "saveachievements" } }
        };
        
        // Aplica mapeamentos estáticos
        foreach (var location in saveLocations)
        {
            string key = $"{location.className}.{location.methodName}";
            if (staticMappings.ContainsKey(key))
            {
                if (!callerToSaveIds.ContainsKey(key))
                {
                    callerToSaveIds[key] = new HashSet<string>();
                }
                foreach (var saveId in staticMappings[key])
                {
                    callerToSaveIds[key].Add(saveId);
                }
            }
        }
        
        UpdateSaveLocationsFromMapping();
    }
    
    void OnDisable()
    {
        SaveEvents.OnSaveExecuted -= OnSaveExecuted;
        SaveEvents.OnLoadExecuted -= OnLoadExecuted;
    }
    
    void OnSaveExecuted(string caller, Dictionary<string, string> savedData)
    {
        lastSaveCaller = caller;
        lastSaveResult = new Dictionary<string, string>(savedData);
        
        // Atualiza mapeamento automático: caller -> saves afetados
        if (!string.IsNullOrEmpty(caller) && savedData != null && savedData.Count > 0)
        {
            if (!callerToSaveIds.ContainsKey(caller))
            {
                callerToSaveIds[caller] = new HashSet<string>();
            }
            foreach (var saveId in savedData.Keys)
            {
                callerToSaveIds[caller].Add(saveId);
            }
            UpdateSaveLocationsFromMapping();
        }
        
        Repaint();
    }
    
    void OnLoadExecuted(string caller, Dictionary<string, string> loadedData)
    {
        lastLoadCaller = caller;
        lastLoadResult = new Dictionary<string, string>(loadedData);
        
        // Atualiza mapeamento automático para loads também
        if (!string.IsNullOrEmpty(caller) && loadedData != null && loadedData.Count > 0)
        {
            if (!callerToSaveIds.ContainsKey(caller))
            {
                callerToSaveIds[caller] = new HashSet<string>();
            }
            foreach (var saveId in loadedData.Keys)
            {
                callerToSaveIds[caller].Add(saveId);
            }
            UpdateSaveLocationsFromMapping();
        }
        
        Repaint();
    }
    
    void UpdateSaveLocationsFromMapping()
    {
        // Atualiza os affectedSaveIds de cada SaveLocation baseado no mapeamento
        foreach (var location in saveLocations)
        {
            string key = $"{location.className}.{location.methodName}";
            
            if (callerToSaveIds.ContainsKey(key))
            {
                location.affectedSaveIds.Clear();
                location.affectedSaveIds.AddRange(callerToSaveIds[key]);
            }
        }
    }
    
    void OnGUI()
    {
        GUILayout.Space(10);
        DrawHeader();
        GUILayout.Space(10);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawSaveLocations();
        DrawLoadLocations();
        DrawResults();
        EditorGUILayout.EndScrollView();
    }
    
    void DrawHeader()
    {
        EditorGUILayout.LabelField("FILTRO POR SAVE", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Busca todos os SaveDefinitions
        var definitions = FindAllSaveDefinitions();
        
        EditorGUILayout.LabelField("Filtrar por:", EditorStyles.miniLabel);
        
        float buttonWidth = 120f;
        float buttonSpacing = 5f;
        float margin = 20f;
        
        // Calcula quantos botões cabem por linha baseado na largura da janela
        float windowWidth = position.width;
        float availableWidth = windowWidth - margin;
        int buttonsPerRow = Mathf.Max(1, Mathf.FloorToInt(availableWidth / (buttonWidth + buttonSpacing)));
        
        int buttonCount = 0;
        EditorGUILayout.BeginHorizontal();
        
        // Botão "Todos" sempre visível na primeira linha
        bool isAllSelected = string.IsNullOrEmpty(selectedFilter);
        GUI.backgroundColor = isAllSelected ? new Color(0.4f, 0.8f, 0.4f) : Color.white;
        if (GUILayout.Button("Todos", GUILayout.Width(buttonWidth), GUILayout.ExpandWidth(false)))
        {
            selectedFilter = "";
        }
        GUI.backgroundColor = Color.white;
        buttonCount++;
        
        // Botões para cada SaveDefinition
        foreach (var def in definitions)
        {
            if (def == null) continue;
            
            // Quebra linha se necessário
            if (buttonCount > 0 && buttonCount % buttonsPerRow == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
            
            bool isSelected = selectedFilter == def.id;
            GUI.backgroundColor = isSelected ? new Color(0.4f, 0.8f, 0.4f) : Color.white;
            if (GUILayout.Button(def.id, GUILayout.Width(buttonWidth), GUILayout.ExpandWidth(false)))
            {
                selectedFilter = isSelected ? "" : def.id;
            }
            GUI.backgroundColor = Color.white;
            buttonCount++;
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (!string.IsNullOrEmpty(selectedFilter))
        {
            EditorGUILayout.HelpBox($"Mostrando apenas locais que afetam: {selectedFilter}", MessageType.Info);
        }
    }
    
    List<SOSaveDefinition> FindAllSaveDefinitions()
    {
        List<SOSaveDefinition> list = new List<SOSaveDefinition>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SOSaveDefinition");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var so = UnityEditor.AssetDatabase.LoadAssetAtPath<SOSaveDefinition>(path);
            if (so != null) list.Add(so);
        }
        return list;
    }
    
    void DrawSaveLocations()
    {
        EditorGUILayout.LabelField("LOCAIS DE SAVE", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        bool hasAnyVisible = false;
        foreach (var location in saveLocations)
        {
            if (!location.isSave) continue;
            
            // Aplica filtro
            if (!string.IsNullOrEmpty(selectedFilter))
            {
                bool matches = false;
                foreach (var saveId in location.affectedSaveIds)
                {
                    if (string.Equals(saveId, selectedFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        matches = true;
                        break;
                    }
                }
                if (!matches) continue;
            }
            
            hasAnyVisible = true;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(location.name, EditorStyles.boldLabel, GUILayout.Width(250));
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            bool canSave = ManagerSave.Instance != null;
            EditorGUI.BeginDisabledGroup(!canSave);
            if (GUILayout.Button("Executar Save", GUILayout.Width(120)))
            {
                if (ManagerSave.Instance != null)
                {
                    // Salva apenas os saves específicos que este local afeta usando evento
                    if (location.affectedSaveIds.Count > 0)
                    {
                        foreach (var saveId in location.affectedSaveIds)
                        {
                            SaveEvents.RaiseSaveSpecific(saveId);
                        }
                    }
                    else
                    {
                        // Se não há saves detectados, tenta salvar todos (comportamento antigo)
                        ManagerSave.Instance.SaveAll();
                    }
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("Aviso", "ManagerSave.Instance não está disponível. Inicie o jogo primeiro.", "OK");
                }
            }
            EditorGUI.EndDisabledGroup();
            if (!canSave)
            {
                EditorGUILayout.LabelField("(Jogo não está rodando)", EditorStyles.miniLabel);
            }
            else
            {
                // Mostra quantos clients estão registrados
                int registeredCount = ManagerSave.Instance != null ? ManagerSave.Instance.GetRegisteredClientsCount() : 0;
                if (registeredCount == 0)
                {
                    EditorGUILayout.LabelField("(Nenhum client registrado - nada será salvo)", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField($"({registeredCount} client(s) registrado(s))", EditorStyles.miniLabel);
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(location.description, EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Classe: {location.className} | Método: {location.methodName}", EditorStyles.miniLabel);
            if (location.affectedSaveIds.Count > 0)
            {
                string savesList = string.Join(", ", location.affectedSaveIds);
                EditorGUILayout.LabelField($"Afeta: {savesList}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Afeta: (não detectado ainda - execute para detectar)", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }
        
        if (!hasAnyVisible && !string.IsNullOrEmpty(selectedFilter))
        {
            EditorGUILayout.HelpBox($"Nenhum local de Save encontrado para: {selectedFilter}", MessageType.Info);
        }
    }
    
    void DrawLoadLocations()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("LOCAIS DE LOAD", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        bool hasAnyVisible = false;
        foreach (var location in saveLocations)
        {
            if (location.isSave) continue;
            
            // Aplica filtro
            if (!string.IsNullOrEmpty(selectedFilter))
            {
                bool matches = false;
                foreach (var saveId in location.affectedSaveIds)
                {
                    if (string.Equals(saveId, selectedFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        matches = true;
                        break;
                    }
                }
                if (!matches) continue;
            }
            
            hasAnyVisible = true;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(location.name, EditorStyles.boldLabel, GUILayout.Width(250));
            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.8f);
            bool canLoad = ManagerSave.Instance != null;
            EditorGUI.BeginDisabledGroup(!canLoad);
            if (GUILayout.Button("Executar Load", GUILayout.Width(120)))
            {
                if (ManagerSave.Instance != null)
                {
                    ManagerSave.Instance.LoadAll();
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("Aviso", "ManagerSave.Instance não está disponível. Inicie o jogo primeiro.", "OK");
                }
            }
            EditorGUI.EndDisabledGroup();
            if (!canLoad)
            {
                EditorGUILayout.LabelField("(Jogo não está rodando)", EditorStyles.miniLabel);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(location.description, EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Classe: {location.className} | Método: {location.methodName}", EditorStyles.miniLabel);
            if (location.affectedSaveIds.Count > 0)
            {
                string savesList = string.Join(", ", location.affectedSaveIds);
                EditorGUILayout.LabelField($"Afeta: {savesList}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Afeta: (não detectado ainda - execute para detectar)", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }
        
        if (!hasAnyVisible && !string.IsNullOrEmpty(selectedFilter))
        {
            EditorGUILayout.HelpBox($"Nenhum local de Load encontrado para: {selectedFilter}", MessageType.Info);
        }
    }
    
    void DrawResults()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("RESULTADOS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Resultado do último Save
        if (!string.IsNullOrEmpty(lastSaveCaller))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = new Color(0.4f, 0.8f, 0.4f);
            EditorGUILayout.LabelField($"ÚLTIMO SAVE - Chamado por: {lastSaveCaller}", EditorStyles.boldLabel);
            GUI.color = Color.white;
            if (lastSaveResult.Count > 0)
            {
                foreach (var kvp in lastSaveResult)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{kvp.Key}:", EditorStyles.boldLabel, GUILayout.Width(120));
                    string displayValue = kvp.Value.Length > 100 ? kvp.Value.Substring(0, 100) + "..." : kvp.Value;
                    EditorGUILayout.LabelField(displayValue, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Nenhum dado salvo", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
        }
        
        GUILayout.Space(5);
        
        // Resultado do último Load
        if (!string.IsNullOrEmpty(lastLoadCaller))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = new Color(0.4f, 0.4f, 0.8f);
            EditorGUILayout.LabelField($"ÚLTIMO LOAD - Chamado por: {lastLoadCaller}", EditorStyles.boldLabel);
            GUI.color = Color.white;
            if (lastLoadResult.Count > 0)
            {
                foreach (var kvp in lastLoadResult)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{kvp.Key}:", EditorStyles.boldLabel, GUILayout.Width(120));
                    string displayValue = kvp.Value.Length > 100 ? kvp.Value.Substring(0, 100) + "..." : kvp.Value;
                    EditorGUILayout.LabelField(displayValue, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Nenhum dado carregado", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
        }
        
        if (string.IsNullOrEmpty(lastSaveCaller) && string.IsNullOrEmpty(lastLoadCaller))
        {
            EditorGUILayout.HelpBox("Execute um Save ou Load para ver os resultados aqui.", MessageType.Info);
        }
    }
}
