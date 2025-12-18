using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class EditorFlagWindow : EditorWindow
{
    private SaveClientZone saveZone;
    private List<SOZoneFlag> allFlags = new List<SOZoneFlag>();
    private Vector2 scrollPos;
    private bool autoRefresh = true;
    private float lastRefreshTime = 0f;
    private const float REFRESH_INTERVAL = 0.5f;

    [MenuItem("Central de Configuração/Central de Flags")]
    public static void ShowWindow()
    {
        GetWindow<EditorFlagWindow>("Flag Manager");
    }

    private void OnEnable()
    {
        RefreshFlagList();
        FindSaveZone();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Central de Flags", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Botão de refresh
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Atualizar Lista", GUILayout.Width(120)))
        {
            RefreshFlagList();
            FindSaveZone();
        }
        
        autoRefresh = EditorGUILayout.Toggle("Auto-refresh", autoRefresh);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Informações sobre SaveClientZone
        if (saveZone == null)
        {
            EditorGUILayout.HelpBox("SaveClientZone não encontrado na cena atual. Execute o jogo ou adicione um SaveClientZone à cena.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"SaveClientZone encontrado. Total de flags: {allFlags.Count}", MessageType.Info);
        }

        EditorGUILayout.Space();

        // Lista de flags
        if (allFlags.Count == 0)
        {
            EditorGUILayout.HelpBox("Nenhuma flag encontrada no projeto. Crie flags usando: Create > Config > ZoneFlag", MessageType.Info);
            return;
        }

        // Cabeçalho da tabela
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("Nome da Flag", EditorStyles.boldLabel, GUILayout.Width(400));
        EditorGUILayout.LabelField("Estado", EditorStyles.boldLabel, GUILayout.Width(100));
        EditorGUILayout.LabelField("Valor", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        // Lista scrollável
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var flag in allFlags.OrderBy(f => f.name))
        {
            DrawFlagRow(flag);
        }

        EditorGUILayout.EndScrollView();

        // Auto-refresh
        if (autoRefresh && Application.isPlaying && saveZone != null)
        {
            if (Time.realtimeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
            {
                Repaint();
                lastRefreshTime = Time.realtimeSinceStartup;
            }
        }
    }

    private void DrawFlagRow(SOZoneFlag flag)
    {
        if (flag == null) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        // Nome da flag
        EditorGUILayout.LabelField(flag.name, GUILayout.Width(400));

        // Estado (True/False)
        bool hasFlag = false;
        int flagValue = 0;
        
        if (saveZone != null && Application.isPlaying)
        {
            flagValue = saveZone.GetFlag(flag);
            hasFlag = saveZone.HasFlag(flag);
        }

        // Cor baseada no estado
        Color originalColor = GUI.color;
        if (hasFlag)
        {
            GUI.color = Color.green;
        }
        else
        {
            GUI.color = Color.red;
        }

        EditorGUILayout.LabelField(hasFlag ? "TRUE" : "FALSE", GUILayout.Width(100));
        GUI.color = originalColor;

        // Valor numérico
        EditorGUILayout.LabelField(flagValue.ToString(), GUILayout.Width(80));

        // Botão para setar manualmente (apenas em play mode)
        if (Application.isPlaying && saveZone != null)
        {
            if (hasFlag)
            {
                if (GUILayout.Button("Set 0", GUILayout.Width(60)))
                {
                    saveZone.SetFlag(flag, 0);
                    Repaint();
                }
            }
            else
            {
                if (GUILayout.Button("Set 1", GUILayout.Width(60)))
                {
                    saveZone.SetFlag(flag, 1);
                    Repaint();
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void RefreshFlagList()
    {
        allFlags.Clear();
        string[] guids = AssetDatabase.FindAssets("t:SOZoneFlag");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SOZoneFlag flag = AssetDatabase.LoadAssetAtPath<SOZoneFlag>(path);
            if (flag != null)
            {
                allFlags.Add(flag);
            }
        }
    }

    private void FindSaveZone()
    {
        // Tenta encontrar na cena atual (Editor)
        saveZone = Object.FindFirstObjectByType<SaveClientZone>();
    }

    private void Update()
    {
        // Auto-refresh durante play mode
        if (autoRefresh && Application.isPlaying)
        {
            Repaint();
        }
    }
}

