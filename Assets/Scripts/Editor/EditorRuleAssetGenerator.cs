using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Gerador de assets para todas as regras do jogo
/// </summary>
public class EditorRuleAssetGenerator : EditorWindow
{
    private string outputPath = "Assets/Data/Rules";
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Gerador de Assets de Regras")]
    public static void ShowWindow()
    {
        EditorRuleAssetGenerator window = GetWindow<EditorRuleAssetGenerator>("Gerador de Regras");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("Gerador de Assets de Regras", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Pasta de Saída:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Selecionar", GUILayout.Width(100)))
        {
            string selectedPath = EditorUtility.SaveFolderPanel("Selecionar Pasta", outputPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                outputPath = "Assets" + selectedPath.Replace(Application.dataPath, "").Replace('\\', '/');
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Regras de Captura:", EditorStyles.boldLabel);
        if (GUILayout.Button("Criar Todas as Regras de Captura"))
        {
            CreateCaptureRules();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Regras de Vitória:", EditorStyles.boldLabel);
        if (GUILayout.Button("Criar Todas as Regras de Vitória"))
        {
            CreateVictoryRules();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Regras Especiais:", EditorStyles.boldLabel);
        if (GUILayout.Button("Criar Todas as Regras Especiais"))
        {
            CreateSpecialRules();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Tudo:", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("CRIAR TODOS OS ASSETS", GUILayout.Height(40)))
        {
            CreateAllAssets();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndScrollView();
    }
    
    private void CreateAllAssets()
    {
        CreateCaptureRules();
        CreateVictoryRules();
        CreateSpecialRules();
        
        EditorUtility.DisplayDialog(
            "Concluído",
            "Todos os assets de regras foram criados com sucesso!",
            "OK"
        );
    }
    
    private void CreateCaptureRules()
    {
        EnsureDirectoryExists($"{outputPath}/Capture");
        
        List<(string name, System.Type type)> captureRules = new List<(string, System.Type)>
        {
            ("Basic", typeof(RuleBasic)),
            ("Same", typeof(RuleSame)),
            ("Plus", typeof(RulePlus)),
            ("Combo", typeof(RuleCombo)),
            ("Reverse", typeof(RuleReverse)),
            ("Ace", typeof(RuleAce)),
            ("PlusWall", typeof(RulePlusWall)),
            ("SameWall", typeof(RuleSameWall)),
            ("Triad", typeof(RuleTriad)),
            ("SpecialBlock", typeof(RuleSpecialBlock))
        };
        
        int created = 0;
        int updated = 0;
        
        foreach (var (name, type) in captureRules)
        {
            string assetPath = $"{outputPath}/Capture/Rule{name}.asset";
            SOCapture rule = AssetDatabase.LoadAssetAtPath<SOCapture>(assetPath);
            
            if (rule == null)
            {
                rule = (SOCapture)ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(rule, assetPath);
                created++;
            }
            else
            {
                updated++;
            }
            
            rule.name = $"Rule{name}";
            EditorUtility.SetDirty(rule);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Regras de Captura: {created} criadas, {updated} atualizadas");
    }
    
    private void CreateVictoryRules()
    {
        EnsureDirectoryExists($"{outputPath}/Victory");
        
        List<(string name, System.Type type)> victoryRules = new List<(string, System.Type)>
        {
            ("WinChosen", typeof(RuleWinChosen)),
            ("WinDiff", typeof(RuleWinDiff)),
            ("WinAll", typeof(RuleWinAll)),
            ("SuddenDeath", typeof(RuleSuddenDeath)),
            ("WinNothing", typeof(RuleWinNothing))
        };
        
        int created = 0;
        int updated = 0;
        
        foreach (var (name, type) in victoryRules)
        {
            string assetPath = $"{outputPath}/Victory/Rule{name}.asset";
            SOVictoryRule rule = AssetDatabase.LoadAssetAtPath<SOVictoryRule>(assetPath);
            
            if (rule == null)
            {
                rule = (SOVictoryRule)ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(rule, assetPath);
                created++;
            }
            else
            {
                updated++;
            }
            
            rule.name = $"Rule{name}";
            EditorUtility.SetDirty(rule);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Regras de Vitória: {created} criadas, {updated} atualizadas");
    }
    
    private void CreateSpecialRules()
    {
        EnsureDirectoryExists($"{outputPath}/Special");
        
        List<(string name, System.Type type)> specialRules = new List<(string, System.Type)>
        {
            ("Open", typeof(RuleOpen)),
            ("Closed", typeof(RuleClosed)),
            ("Random", typeof(RuleRandom)),
            ("Roulette", typeof(RuleRoulette)),
            ("HandSpecial", typeof(RuleHandSpecial)),
            ("HandLegend", typeof(RuleHandLegend))
        };
        
        int created = 0;
        int updated = 0;
        
        foreach (var (name, type) in specialRules)
        {
            string assetPath = $"{outputPath}/Special/Rule{name}.asset";
            SOCapture rule = AssetDatabase.LoadAssetAtPath<SOCapture>(assetPath);
            
            if (rule == null)
            {
                rule = (SOCapture)ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(rule, assetPath);
                created++;
            }
            else
            {
                updated++;
            }
            
            rule.name = $"Rule{name}";
            EditorUtility.SetDirty(rule);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Regras Especiais: {created} criadas, {updated} atualizadas");
        
        // Criar regras de efeitos de carta
        CreateCardEffectRules();
    }
    
    private void CreateCardEffectRules()
    {
        EnsureDirectoryExists($"{outputPath}/CardEffects");
        
        List<(string name, System.Type type)> cardEffectRules = new List<(string, System.Type)>
        {
            ("Attack", typeof(RuleAttack)),
            ("Defense", typeof(RuleDefense)),
            ("Protection", typeof(RuleProtection)),
            ("Aura", typeof(RuleAuraEffect)),
            ("Domain", typeof(RuleDomain)),
            ("Corruption", typeof(RuleCorruption)),
            ("Wear", typeof(RuleWear)),
            ("Sacrifice", typeof(RuleSacrifice)),
            ("Echo", typeof(RuleEcho)),
            ("Retaliation", typeof(RuleRetaliation)),
            ("Territory", typeof(RuleTerritory)),
            ("TerritoryStrong", typeof(RuleTerritoryStrong)),
            ("Stealth", typeof(RuleStealth)),
            ("StealthEffect", typeof(RuleStealthEffect)),
            ("CenterStrong", typeof(RuleCenterStrong)),
            ("CornerStrong", typeof(RuleCornerStrong)),
            ("SideStrong", typeof(RuleSideStrong)),
            ("Bonus1", typeof(RuleBonus1)),
            ("Bonus2", typeof(RuleBonus2)),
            ("Penalty1", typeof(RulePenalty1)),
            ("Penalty2", typeof(RulePenalty2)),
            ("Betrayal", typeof(RuleBetrayal))
        };
        
        int created = 0;
        int updated = 0;
        
        foreach (var (name, type) in cardEffectRules)
        {
            string assetPath = $"{outputPath}/CardEffects/Rule{name}.asset";
            SOCapture rule = AssetDatabase.LoadAssetAtPath<SOCapture>(assetPath);
            
            if (rule == null)
            {
                rule = (SOCapture)ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(rule, assetPath);
                created++;
            }
            else
            {
                updated++;
            }
            
            rule.name = $"Rule{name}";
            EditorUtility.SetDirty(rule);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Regras de Efeitos de Carta: {created} criadas, {updated} atualizadas");
    }
    
    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
