using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
public class EditorDefaultExecutionOrder : EditorWindow
{
    private List<MonoScript> managerScripts = new List<MonoScript>();
    private int executionOrderValue = 0;
    private Vector2 scrollPosition;
    [MenuItem("Tools/Set Default Execution Order")]
    public static void ShowWindow()
    {
        GetWindow<EditorDefaultExecutionOrder>("Execution Order Setter");
    }
    private void OnEnable()
    {
        FindRelevantScripts();
    }
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Configuração de Ordem de Execução", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        executionOrderValue = EditorGUILayout.IntField("Ordem (- primeiro)", executionOrderValue);
        if (GUILayout.Button("Aplicar a Todos", GUILayout.Width(150)))
        {
            ApplyOrderToScripts(managerScripts, executionOrderValue);
            FindRelevantScripts();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"Scripts Encontrados ({managerScripts.Count})", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var script in managerScripts)
        {
            if (script == null) continue;
            int currentOrder = MonoImporter.GetExecutionOrder(script);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
            EditorGUILayout.LabelField("Atual: " + currentOrder, GUILayout.Width(80));
            if (GUILayout.Button("Aplicar " + executionOrderValue, GUILayout.Width(120)))
            {
                ApplyOrderToScripts(new List<MonoScript> { script }, executionOrderValue);
                FindRelevantScripts();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Atualizar Lista"))
        {
            FindRelevantScripts();
        }
    }
    private void FindRelevantScripts()
    {
        managerScripts.Clear();
        var allScripts = AssetDatabase.FindAssets("t:MonoScript")
        .Select(guid => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid)));
        foreach (var script in allScripts)
        {
            if (script == null || script.GetClass() == null || !script.GetClass().IsSubclassOf(typeof(MonoBehaviour)))
            continue;
            string className = script.GetClass().Name;
            if (className.StartsWith("Manager") || className.StartsWith("Display"))
            managerScripts.Add(script);
        }
        managerScripts = managerScripts
        .OrderBy(s => MonoImporter.GetExecutionOrder(s))
        .ToList();
    }
    private void ApplyOrderToScripts(List<MonoScript> scripts, int order)
    {
        foreach (var script in scripts)
        {
            if (script == null) continue;
            MonoImporter.SetExecutionOrder(script, order);
        }
        AssetDatabase.Refresh();
    }
}
