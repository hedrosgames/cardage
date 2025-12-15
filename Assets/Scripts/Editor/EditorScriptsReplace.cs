using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class EditorScriptsReplace : EditorWindow
{
    [MenuItem("Scripts/Substituir")]
    public static void ReplaceFromClipboard()
    {
        string clip = EditorGUIUtility.systemCopyBuffer;

        if (string.IsNullOrEmpty(clip))
        {
            EditorFeedback.ShowFeedback("Falha", "Não tem script no clipboard", false);
            return;
        }

        string className = ExtractClassName(clip);
        if (string.IsNullOrEmpty(className))
        {
            EditorFeedback.ShowFeedback("Falha", "Nome da classe não encontrado no clipboard", false);
            return;
        }

        string[] guids = AssetDatabase.FindAssets(className + " t:MonoScript");
        
        // Busca exata
        var exactMatchGuid = guids.FirstOrDefault(g => 
            Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)) == className
        );

        if (string.IsNullOrEmpty(exactMatchGuid))
        {
            EditorFeedback.ShowFeedback("Falha", $"Script Não Existente: {className}.cs", false);
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(exactMatchGuid);
        string full = Path.Combine(Directory.GetCurrentDirectory(), path);

        try
        {
            File.WriteAllText(full, clip);
            AssetDatabase.Refresh();
            
            // Sucesso
            EditorFeedback.ShowFeedback("Sucesso", $"Script Substituído em: {path}", true);
        }
        catch
        {
            EditorFeedback.ShowFeedback("Erro", "Falha ao escrever no arquivo", false);
        }
    }

    static string ExtractClassName(string code)
    {
        Match m = Regex.Match(code, @"class\s+([A-Za-z0-9_]+)");
        if (m.Success) return m.Groups[1].Value;
        return null;
    }
}