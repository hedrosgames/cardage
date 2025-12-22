using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class EditorGameSetupGenerator : EditorWindow
{
    private string outputPath = "Assets/Data/GameSetup";
    private Vector2 scrollPosition;
    private List<SetupInfo> setupInfos = new List<SetupInfo>();
    
    private class SetupInfo
    {
        public string prefix;
        public SODeckData deck;
        public SOOpponentData opponent;
        public SOGameSetup existingSetup;
        public bool willCreate;
        public bool willUpdate;
    }
    
    [MenuItem("Tools/Gerador de Game Setup")]
    public static void ShowWindow()
    {
        EditorGameSetupGenerator window = GetWindow<EditorGameSetupGenerator>("Gerador de Game Setup");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("Gerador de Game Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pasta de Saída:", GUILayout.Width(120));
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Procurar", GUILayout.Width(80)))
        {
            string path = EditorUtility.SaveFolderPanel("Selecione a pasta de saída", outputPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    outputPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Analisar Oponentes e Decks", GUILayout.Height(30)))
        {
            AnalyzeAssets();
        }
        
        EditorGUILayout.Space();
        
        if (setupInfos.Count > 0)
        {
            EditorGUILayout.LabelField($"Encontrados: {setupInfos.Count} pares Deck/Oponente", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var info in setupInfos)
            {
                DrawSetupInfo(info);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Gerar/Atualizar Todos os Game Setups", GUILayout.Height(30)))
            {
                GenerateAllSetups();
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Clique em 'Analisar Oponentes e Decks' para encontrar pares de Deck e Oponente com o mesmo prefixo (ex: 001_).",
                MessageType.Info
            );
        }
    }
    
    void DrawSetupInfo(SetupInfo info)
    {
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Prefixo: {info.prefix}", EditorStyles.boldLabel);
        
        if (info.willCreate)
        {
            GUI.color = Color.green;
            EditorGUILayout.LabelField("CRIAR", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }
        else if (info.willUpdate)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("ATUALIZAR", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.gray;
            EditorGUILayout.LabelField("OK", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.LabelField($"Deck: {info.deck?.name ?? "NÃO ENCONTRADO"}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Oponente: {info.opponent?.name ?? "NÃO ENCONTRADO"}", EditorStyles.miniLabel);
        
        if (info.existingSetup != null)
        {
            EditorGUILayout.LabelField($"Game Setup Existente: {info.existingSetup.name}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    void AnalyzeAssets()
    {
        setupInfos.Clear();
        
        Dictionary<string, SODeckData> decks = new Dictionary<string, SODeckData>();
        Dictionary<string, SOOpponentData> opponents = new Dictionary<string, SOOpponentData>();
        Dictionary<string, SOGameSetup> existingSetups = new Dictionary<string, SOGameSetup>();
        
        string[] deckGuids = AssetDatabase.FindAssets("t:SODeckData");
        foreach (string guid in deckGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SODeckData deck = AssetDatabase.LoadAssetAtPath<SODeckData>(path);
            if (deck != null)
            {
                string prefix = ExtractPrefix(deck.name);
                if (!string.IsNullOrEmpty(prefix) && !decks.ContainsKey(prefix))
                {
                    decks[prefix] = deck;
                }
            }
        }
        
        string[] opponentGuids = AssetDatabase.FindAssets("t:SOOpponentData");
        foreach (string guid in opponentGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SOOpponentData opponent = AssetDatabase.LoadAssetAtPath<SOOpponentData>(path);
            if (opponent != null)
            {
                string prefix = ExtractPrefix(opponent.name);
                if (!string.IsNullOrEmpty(prefix) && !opponents.ContainsKey(prefix))
                {
                    opponents[prefix] = opponent;
                }
            }
        }
        
        string[] setupGuids = AssetDatabase.FindAssets("t:SOGameSetup");
        foreach (string guid in setupGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SOGameSetup setup = AssetDatabase.LoadAssetAtPath<SOGameSetup>(path);
            if (setup != null)
            {
                string prefix = ExtractPrefix(setup.name);
                if (!string.IsNullOrEmpty(prefix) && !existingSetups.ContainsKey(prefix))
                {
                    existingSetups[prefix] = setup;
                }
            }
        }
        
        HashSet<string> allPrefixes = new HashSet<string>();
        foreach (var key in decks.Keys) allPrefixes.Add(key);
        foreach (var key in opponents.Keys) allPrefixes.Add(key);
        
        foreach (string prefix in allPrefixes.OrderBy(p => p))
        {
            SetupInfo info = new SetupInfo
            {
                prefix = prefix,
                deck = decks.ContainsKey(prefix) ? decks[prefix] : null,
                opponent = opponents.ContainsKey(prefix) ? opponents[prefix] : null,
                existingSetup = existingSetups.ContainsKey(prefix) ? existingSetups[prefix] : null
            };
            
            if (info.deck != null && info.opponent != null)
            {
                if (info.existingSetup == null)
                {
                    info.willCreate = true;
                }
                else
                {
                    if (info.existingSetup.playerDeck != info.deck || info.existingSetup.opponent != info.opponent)
                    {
                        info.willUpdate = true;
                    }
                }
            }
            
            setupInfos.Add(info);
        }
    }
    
    string ExtractPrefix(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        
        int underscoreIndex = name.IndexOf('_');
        if (underscoreIndex > 0)
        {
            return name.Substring(0, underscoreIndex + 1);
        }
        
        return "";
    }
    
    void GenerateAllSetups()
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            EditorUtility.DisplayDialog("Erro", "Por favor, selecione uma pasta de saída.", "OK");
            return;
        }
        
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        int created = 0;
        int updated = 0;
        int skipped = 0;
        
        foreach (var info in setupInfos)
        {
            if (info.deck == null || info.opponent == null)
            {
                skipped++;
                continue;
            }
            
            string fileName = $"{info.prefix}GameSetup";
            string assetPath = $"{outputPath}/{fileName}.asset";
            
            SOGameSetup setup = info.existingSetup;
            
            if (setup == null)
            {
                setup = ScriptableObject.CreateInstance<SOGameSetup>();
                AssetDatabase.CreateAsset(setup, assetPath);
                created++;
            }
            else
            {
                if (setup.playerDeck == info.deck && setup.opponent == info.opponent)
                {
                    skipped++;
                    continue;
                }
                updated++;
            }
            
            setup.playerDeck = info.deck;
            setup.opponent = info.opponent;
            setup.name = fileName;
            
            EditorUtility.SetDirty(setup);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        AnalyzeAssets();
        
        EditorUtility.DisplayDialog(
            "Concluído",
            $"Game Setups processados!\n\n" +
            $"Criados: {created}\n" +
            $"Atualizados: {updated}\n" +
            $"Ignorados: {skipped}",
            "OK"
        );
    }
}
