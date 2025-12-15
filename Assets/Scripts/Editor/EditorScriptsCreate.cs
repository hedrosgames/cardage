using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class EditorScriptsCreate : EditorWindow
{
    [MenuItem("Scripts/Novo")]
    public static void CreateFromClipboard()
    {
        string clip = EditorGUIUtility.systemCopyBuffer;
        
        // Validação Inicial
        if (string.IsNullOrEmpty(clip))
        {
            EditorFeedback.ShowFeedback("Falha", "Não tem script no clipboard", false);
            return;
        }

        string className = ExtractClassName(clip);
        if (string.IsNullOrEmpty(className))
        {
            EditorFeedback.ShowFeedback("Falha", "Não encontrei 'class Nome' no código", false);
            return;
        }

        // Define Caminho
        string targetFolder = "";
        if (className.StartsWith("Editor"))
        {
            targetFolder = "Assets/Scripts/Editor";
            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);
        }
        else
        {
            string folder = EditorUtility.OpenFolderPanel("Salvar script", Application.dataPath + "/Scripts", "");
            if (string.IsNullOrEmpty(folder)) return; // Cancelou janela

            if (folder.StartsWith(Application.dataPath))
                targetFolder = "Assets" + folder.Substring(Application.dataPath.Length);
            else
            {
                EditorFeedback.ShowFeedback("Falha", "Pasta deve estar dentro de Assets", false);
                return;
            }
        }

        string finalPath = Path.Combine(targetFolder, className + ".cs");

        // Verifica existência
        if (File.Exists(finalPath))
        {
            EditorFeedback.ShowFeedback("Falha", $"Script Existente: {className}", false);
            return;
        }

        // Criação
        try
        {
            File.WriteAllText(finalPath, clip);
            AssetDatabase.Refresh();
            
            // Sucesso
            EditorFeedback.ShowFeedback("Sucesso", $"Script Criado em: {finalPath}", true);
            
            // Seleciona o arquivo
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(finalPath);
            if(obj) EditorGUIUtility.PingObject(obj);
        }
        catch
        {
            EditorFeedback.ShowFeedback("Erro Crítico", "Erro ao gravar arquivo no disco", false);
        }
    }

    static string ExtractClassName(string code)
    {
        Match m = Regex.Match(code, @"class\s+([A-Za-z0-9_]+)");
        if (m.Success) return m.Groups[1].Value;
        return null;
    }
}