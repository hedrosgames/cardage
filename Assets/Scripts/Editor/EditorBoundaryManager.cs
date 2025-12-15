using UnityEngine;
using UnityEditor;
using Game.World;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class EditorBoundaryManager : EditorWindow
{
    private SOBoundaryLibrary library;
    private Vector2 scrollPos;
    private string newBoundaryName = "";

    [MenuItem("Tools/Central de Boundaries")]
    public static void ShowWindow()
    {
        GetWindow<EditorBoundaryManager>("Boundary Manager");
    }

    private void OnEnable()
    {
        // Tenta encontrar a Library automaticamente
        if (library == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:SOBoundaryLibrary");
            if (guids.Length > 0)
                library = AssetDatabase.LoadAssetAtPath<SOBoundaryLibrary>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configuração de Limites", EditorStyles.boldLabel);
        
        library = (SOBoundaryLibrary)EditorGUILayout.ObjectField("Library Asset", library, typeof(SOBoundaryLibrary), false);

        if (library == null)
        {
            EditorGUILayout.HelpBox("Crie ou atribua um SOBoundaryLibrary para começar.", MessageType.Warning);
            if (GUILayout.Button("Criar Nova Library"))
            {
                CreateLibraryAsset();
            }
            return;
        }

        EditorGUILayout.Space();
        DrawAddSection();
        EditorGUILayout.Space();
        DrawList();
        EditorGUILayout.Space();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("SALVAR TUDO E GERAR ENUM", GUILayout.Height(40)))
        {
            SaveAndGenerateEnum();
        }
        GUI.backgroundColor = Color.white;
    }

    void DrawAddSection()
    {
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("Novo ID:", GUILayout.Width(60));
        newBoundaryName = EditorGUILayout.TextField(newBoundaryName);
        if (GUILayout.Button("Adicionar", GUILayout.Width(80)))
        {
            AddBoundary();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawList()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        for (int i = 0; i < library.boundaries.Count; i++)
        {
            var item = library.boundaries[i];
            EditorGUILayout.BeginVertical("helpbox");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i + 1}. {item.idName}", EditorStyles.boldLabel);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                library.boundaries.RemoveAt(i);
                EditorUtility.SetDirty(library);
                i--;
                continue;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            item.minLimit = EditorGUILayout.Vector2Field("Min Limit (Esq/Baixo)", item.minLimit);
            item.maxLimit = EditorGUILayout.Vector2Field("Max Limit (Dir/Cima)", item.maxLimit);
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(library);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();
    }

    void AddBoundary()
    {
        if (string.IsNullOrEmpty(newBoundaryName)) return;
        
        // Sanitiza o nome para ser um Enum válido
        string cleanName = Regex.Replace(newBoundaryName, @"[^a-zA-Z0-9_]", "");
        if (string.IsNullOrEmpty(cleanName) || char.IsDigit(cleanName[0])) 
            cleanName = "B_" + cleanName;

        if (library.boundaries.Any(b => b.idName == cleanName))
        {
            EditorUtility.DisplayDialog("Erro", "ID já existe!", "OK");
            return;
        }

        library.boundaries.Add(new SOBoundaryLibrary.BoundaryData 
        { 
            idName = cleanName,
            minLimit = new Vector2(-10, -10),
            maxLimit = new Vector2(10, 10)
        });
        
        newBoundaryName = "";
        EditorUtility.SetDirty(library);
        GUI.FocusControl(null);
    }

    void CreateLibraryAsset()
    {
        SOBoundaryLibrary newLib = CreateInstance<SOBoundaryLibrary>();
        string path = "Assets/Data/Config/BoundaryLibrary.asset";
        
        // Garante que a pasta existe
        if (!Directory.Exists("Assets/Data/Config"))
            Directory.CreateDirectory("Assets/Data/Config");

        AssetDatabase.CreateAsset(newLib, path);
        AssetDatabase.SaveAssets();
        library = newLib;
    }

    void SaveAndGenerateEnum()
    {
        if (library == null) return;

        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();

        // Gera o arquivo BoundaryId.cs
        string enumPath = "Assets/Scripts/World/BoundaryId.cs";
        
        List<string> ids = new List<string> { "None" };
        ids.AddRange(library.boundaries.Select(b => b.idName));

        string content = "namespace Game.World\n{\n\tpublic enum BoundaryId\n\t{\n";
        for (int i = 0; i < ids.Count; i++)
        {
            content += $"\t\t{ids[i]}";
            if (i < ids.Count - 1) content += ",";
            content += "\n";
        }
        content += "\t}\n}";

        File.WriteAllText(enumPath, content);
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>[Boundary Manager]</color> Enum gerado com {ids.Count} entradas!");
    }
}