using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

public class EditorAchievementCentral : EditorWindow
{
    private SOAchievementLibrary library;
    private Vector2 scrollPosition;
    private string searchFilter = "";
    private bool showOnlyWithCode = false;
    private bool showOnlyWithoutCode = false;
    private Dictionary<string, string> codeLocationCache = new Dictionary<string, string>();
    
    [MenuItem("Central de ConfiguraÃ§Ã£o/Central de Achievements")]
    public static void ShowWindow()
    {
        EditorAchievementCentral window = GetWindow<EditorAchievementCentral>("Central de Achievements");
        window.minSize = new Vector2(800, 600);
        window.Show();
    }
    
    void OnEnable()
    {
        LoadLibrary();
        ScanCodeForAchievements();
    }
    
    void LoadLibrary()
    {
        string[] guids = AssetDatabase.FindAssets("t:SOAchievementLibrary");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            library = AssetDatabase.LoadAssetAtPath<SOAchievementLibrary>(path);
        }
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Central de Achievements", EditorStyles.boldLabel);
        if (GUILayout.Button("Recarregar", GUILayout.Width(100)))
        {
            LoadLibrary();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Filtros
        EditorGUILayout.BeginHorizontal();
        searchFilter = EditorGUILayout.TextField("Buscar:", searchFilter);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        showOnlyWithCode = EditorGUILayout.Toggle("Apenas com cÃ³digo encontrado", showOnlyWithCode);
        showOnlyWithoutCode = EditorGUILayout.Toggle("Apenas sem cÃ³digo encontrado", showOnlyWithoutCode);
        if (GUILayout.Button("Buscar no CÃ³digo", GUILayout.Width(150)))
        {
            ScanCodeForAchievements();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (library == null)
        {
            EditorGUILayout.HelpBox("Biblioteca de Achievements nÃ£o encontrada! Procure por um asset do tipo SOAchievementLibrary.", MessageType.Warning);
            if (GUILayout.Button("Criar Nova Biblioteca"))
            {
                CreateNewLibrary();
            }
            return;
        }
        
        if (library.achievements == null || library.achievements.Count == 0)
        {
            EditorGUILayout.HelpBox("Nenhum achievement encontrado na biblioteca.", MessageType.Info);
            return;
        }
        
        // EstatÃ­sticas
        int total = library.achievements.Count;
        int withCode = library.achievements.Count(a => a != null && codeLocationCache.ContainsKey(a.id));
        int withoutCode = total - withCode;
        
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField($"Total: {total}", GUILayout.Width(100));
        EditorGUILayout.LabelField($"Com cÃ³digo encontrado: {withCode}", GUILayout.Width(180));
        EditorGUILayout.LabelField($"Sem cÃ³digo encontrado: {withoutCode}", GUILayout.Width(180));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Lista de achievements
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        var filteredAchievements = library.achievements.Where(a => a != null).ToList();
        
        // Aplicar filtros
        if (!string.IsNullOrEmpty(searchFilter))
        {
            filteredAchievements = filteredAchievements.Where(a => 
                a.id.ToLower().Contains(searchFilter.ToLower()) ||
                a.titleKey.ToLower().Contains(searchFilter.ToLower()) ||
                (codeLocationCache.ContainsKey(a.id) && codeLocationCache[a.id].ToLower().Contains(searchFilter.ToLower()))
            ).ToList();
        }
        
        if (showOnlyWithCode)
        {
            filteredAchievements = filteredAchievements.Where(a => codeLocationCache.ContainsKey(a.id)).ToList();
        }
        
        if (showOnlyWithoutCode)
        {
            filteredAchievements = filteredAchievements.Where(a => !codeLocationCache.ContainsKey(a.id)).ToList();
        }
        
        foreach (var achievement in filteredAchievements)
        {
            DrawAchievementCard(achievement);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void DrawAchievementCard(SOAchievement achievement)
    {
        EditorGUILayout.BeginVertical("box");
        
        // CabeÃ§alho
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(achievement.name, EditorStyles.boldLabel, GUILayout.Width(200));
        
        // Status do cÃ³digo (baseado na busca, nÃ£o no asset)
        if (codeLocationCache.ContainsKey(achievement.id))
        {
            GUI.color = Color.green;
            EditorGUILayout.LabelField("âœ“ ENCONTRADO", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("âš  NÃƒO ENCONTRADO", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }
        
        // Campo editÃ¡vel para descriÃ§Ã£o de como o jogador ganha
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Como ganha:", GUILayout.Width(80));
        string newDescription = EditorGUILayout.TextField(achievement.editorDescription ?? "", GUILayout.ExpandWidth(true));
        if (newDescription != achievement.editorDescription)
        {
            achievement.editorDescription = newDescription;
            EditorUtility.SetDirty(achievement);
            AssetDatabase.SaveAssets();
        }
        
        if (GUILayout.Button("Selecionar", GUILayout.Width(80)))
        {
            Selection.activeObject = achievement;
            EditorGUIUtility.PingObject(achievement);
        }
        EditorGUILayout.EndHorizontal();
        
        // ID
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ID:", GUILayout.Width(50));
        EditorGUILayout.LabelField(achievement.id, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndHorizontal();
        
        // LocalizaÃ§Ã£o do cÃ³digo (apenas mostra o resultado da busca)
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Onde Ã© desbloqueado:", GUILayout.Width(130));
        if (codeLocationCache.ContainsKey(achievement.id))
        {
            GUI.color = Color.green;
            EditorGUILayout.LabelField(codeLocationCache[achievement.id], EditorStyles.wordWrappedLabel);
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.gray;
            EditorGUILayout.LabelField("(nÃ£o encontrado no cÃ³digo)", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }
        EditorGUILayout.EndHorizontal();
        
        // Chaves de localizaÃ§Ã£o
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("TÃ­tulo:", GUILayout.Width(50));
        EditorGUILayout.LabelField(achievement.titleKey, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("DescriÃ§Ã£o:", GUILayout.Width(70));
        EditorGUILayout.LabelField(achievement.descriptionKey, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    void CreateNewLibrary()
    {
        SOAchievementLibrary newLibrary = ScriptableObject.CreateInstance<SOAchievementLibrary>();
        newLibrary.achievements = new List<SOAchievement>();
        
        string path = EditorUtility.SaveFilePanelInProject(
            "Criar Biblioteca de Achievements",
            "AchievementsConfig",
            "asset",
            "Escolha onde salvar a biblioteca de achievements"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newLibrary, path);
            AssetDatabase.SaveAssets();
            library = newLibrary;
        }
    }
    
    void ScanCodeForAchievements()
    {
        codeLocationCache.Clear();
        
        if (library == null || library.achievements == null) return;
        
        string scriptsPath = Application.dataPath + "/Scripts";
        if (!Directory.Exists(scriptsPath)) return;
        
        // Busca por AchievementSystem.Unlock("achiev1") ou AchievementSystem.Unlock(AchievementIds.XXX)
        string[] csFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);
        
        // Mapear AchievementIds para IDs reais
        Dictionary<string, string> achievementIdConstants = new Dictionary<string, string>();
        foreach (var achievement in library.achievements)
        {
            if (achievement == null || string.IsNullOrEmpty(achievement.id)) continue;
            
            // Busca a constante correspondente no AchievementIds.cs
            string idsFile = csFiles.FirstOrDefault(f => Path.GetFileName(f) == "AchievementIds.cs");
            if (idsFile != null)
            {
                try
                {
                    string idsContent = File.ReadAllText(idsFile);
                    // Procura por: public const string XXX = "achiev1";
                    string pattern = $@"public\s+const\s+string\s+(\w+)\s*=\s*[""']{Regex.Escape(achievement.id)}[""']";
                    Match match = Regex.Match(idsContent, pattern);
                    if (match.Success)
                    {
                        achievementIdConstants[match.Groups[1].Value] = achievement.id;
                    }
                }
                catch { }
            }
        }
        
        foreach (var achievement in library.achievements)
        {
            if (achievement == null || string.IsNullOrEmpty(achievement.id)) continue;
            
            foreach (var filePath in csFiles)
            {
                // Ignora arquivos Editor
                if (filePath.Contains("\\Editor\\") || filePath.Contains("/Editor/")) continue;
                
                try
                {
                    string content = File.ReadAllText(filePath);
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    
                    // Busca por: AchievementSystem.Unlock("achiev1")
                    string pattern1 = $@"AchievementSystem\.Unlock\s*\(\s*[""']{Regex.Escape(achievement.id)}[""']\s*\)";
                    
                    // Busca por AchievementIds.XXX que corresponde a este achievement
                    bool foundViaConstant = false;
                    string constantName = "";
                    foreach (var kvp in achievementIdConstants)
                    {
                        if (kvp.Value == achievement.id)
                        {
                            constantName = kvp.Key;
                            if (content.Contains($"AchievementIds.{constantName}"))
                            {
                                foundViaConstant = true;
                                break;
                            }
                        }
                    }
                    
                    if (Regex.IsMatch(content, pattern1) || foundViaConstant)
                    {
                        // Encontrou! Agora precisa encontrar a classe e mÃ©todo
                        string[] lines = content.Split('\n');
                        string currentClass = "";
                        string currentMethod = "";
                        int unlockLineIndex = -1;
                        
                        for (int i = 0; i < lines.Length; i++)
                        {
                            // Detecta classe
                            if (Regex.IsMatch(lines[i], @"^\s*(public\s+)?(abstract\s+)?(sealed\s+)?class\s+\w+"))
                            {
                                Match match = Regex.Match(lines[i], @"class\s+(\w+)");
                                if (match.Success) currentClass = match.Groups[1].Value;
                                currentMethod = ""; // Reset mÃ©todo quando muda de classe
                            }
                            
                            // Detecta mÃ©todo (mais flexÃ­vel)
                            Match methodMatch = Regex.Match(lines[i], @"^\s*(public|private|protected|internal|static)\s+(static\s+)?(async\s+)?(void|bool|int|string|IEnumerator|\w+)\s+(\w+)\s*\(");
                            if (methodMatch.Success)
                            {
                                currentMethod = methodMatch.Groups[5].Value;
                            }
                            
                            // Verifica se esta linha contÃ©m o Unlock
                            bool lineHasUnlock = false;
                            if (lines[i].Contains($"Unlock(\"{achievement.id}\""))
                            {
                                lineHasUnlock = true;
                            }
                            else if (foundViaConstant && lines[i].Contains($"AchievementIds.{constantName}"))
                            {
                                lineHasUnlock = true;
                            }
                            
                            if (lineHasUnlock)
                            {
                                unlockLineIndex = i;
                                if (!string.IsNullOrEmpty(currentClass) && !string.IsNullOrEmpty(currentMethod))
                                {
                                    codeLocationCache[achievement.id] = $"{currentClass}.{currentMethod}";
                                    break;
                                }
                            }
                        }
                        
                        // Se encontrou a linha mas nÃ£o o mÃ©todo, tenta encontrar o mÃ©todo mais prÃ³ximo acima
                        if (unlockLineIndex >= 0 && !codeLocationCache.ContainsKey(achievement.id))
                        {
                            // Procura mÃ©todo acima da linha encontrada
                            for (int i = unlockLineIndex; i >= 0; i--)
                            {
                                Match methodMatch = Regex.Match(lines[i], @"^\s*(public|private|protected|internal|static)\s+(static\s+)?(async\s+)?(void|bool|int|string|IEnumerator|\w+)\s+(\w+)\s*\(");
                                if (methodMatch.Success)
                                {
                                    currentMethod = methodMatch.Groups[5].Value;
                                    if (!string.IsNullOrEmpty(currentClass) && !string.IsNullOrEmpty(currentMethod))
                                    {
                                        codeLocationCache[achievement.id] = $"{currentClass}.{currentMethod}";
                                        break;
                                    }
                                }
                                
                                // Se chegou na declaraÃ§Ã£o da classe, para
                                if (Regex.IsMatch(lines[i], @"^\s*(public\s+)?(abstract\s+)?(sealed\s+)?class\s+\w+"))
                                {
                                    break;
                                }
                            }
                        }
                        
                        // Se ainda nÃ£o encontrou, usa o nome do arquivo
                        if (!codeLocationCache.ContainsKey(achievement.id))
                        {
                            codeLocationCache[achievement.id] = fileName;
                        }
                        
                        break; // Encontrou, pode parar de procurar neste achievement
                    }
                }
                catch
                {
                    // Ignora erros de leitura
                }
            }
        }
    }
}
