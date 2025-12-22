using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
public class EditorScriptsFormatter : EditorWindow
{
    [MenuItem("Tools/Limpar Scripts Editor")]
    public static void FormatEditorScripts()
    {
        string editorPath = "Assets/Scripts/Editor";
        if (!Directory.Exists(editorPath))
        {
            EditorFeedback.ShowFeedback("Erro", "Pasta Assets/Scripts/Editor não encontrada", false);
            return;
        }
        string[] files = Directory.GetFiles(editorPath, "*.cs", SearchOption.AllDirectories);
        int changedCount = 0;
        foreach (string file in files)
        {
            try
            {
                string original = File.ReadAllText(file);
                string cleaned = CleanAndFormat(original);
                if (cleaned != original)
                {
                    File.WriteAllText(file, cleaned);
                    changedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao formatar {file}: {e.Message}");
            }
        }
        AssetDatabase.Refresh();
        if (changedCount > 0)
        {
            EditorFeedback.ShowFeedback("Sucesso", $"{changedCount} scripts de editor limpos e formatados!", true);
        }
        else
        {
            EditorFeedback.ShowFeedback("Info", "Nenhum script precisou de alteração", false);
        }
    }
    private static string CleanAndFormat(string code)
    {
        code = RemoveCommentsSafe(code);
        code = NormalizeWhitespace(code);
        code = IndentCode(code);
        return code;
    }
    private static string NormalizeWhitespace(string code)
    {
        var lines = code.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        StringBuilder sb = new StringBuilder();
        foreach (var l in lines)
        {
            string t = l.TrimEnd();
            if (t.Length == 0)
            {
                if (sb.Length > 0 && sb[sb.Length - 1] != '\n') sb.Append("\n");
            }
            else
            {
                sb.AppendLine(t);
            }
        }
        return sb.ToString().TrimEnd() + "\n";
    }
    private static string IndentCode(string code)
    {
        var lines = code.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        StringBuilder sb = new StringBuilder();
        int indent = 0;
        foreach (var raw in lines)
        {
            string line = raw.Trim();
            if (string.IsNullOrEmpty(line))
            {
                sb.AppendLine("");
                continue;
            }
            if (line.StartsWith("}")) indent = Mathf.Max(0, indent - 1);
        sb.Append(new string(' ', indent * 4));
        sb.AppendLine(line);
        int openBraces = line.Count(c => c == '{');
            int closeBraces = line.Count(c => c == '}');
        int netBraces = openBraces - closeBraces;
        if (line.StartsWith("}"))
    {
        indent += (netBraces + 1);
    }
    else
    {
        indent += netBraces;
    }
    indent = Mathf.Max(0, indent);
}
return sb.ToString().TrimEnd() + "\n";
}
private static string RemoveCommentsSafe(string code)
{
    StringBuilder result = new StringBuilder();
    bool inString = false, inChar = false, inLineComment = false, inBlockComment = false;
    for (int i = 0; i < code.Length; i++)
    {
        char c = code[i];
        char n = i < code.Length - 1 ? code[i + 1] : '\0';
        if (inLineComment)
        {
            if (c == '\n' || c == '\r')
            {
                inLineComment = false;
                result.Append(c);
            }
            continue;
        }
        if (inBlockComment)
        {
            if (c == '*' && n == '/')
            {
                inBlockComment = false;
                i++;
            }
            continue;
        }
        if (!inString && !inChar)
        {
            if (c == '/' && n == '/')
            {
                inLineComment = true;
                i++;
                continue;
            }
            if (c == '/' && n == '*')
            {
                inBlockComment = true;
                i++;
                continue;
            }
        }
        if (c == '"' && !inChar && (i == 0 || code[i - 1] != '\\')) inString = !inString;
        if (c == '\'' && !inString && (i == 0 || code[i - 1] != '\\')) inChar = !inChar;
        if (!inLineComment && !inBlockComment)
        {
            result.Append(c);
        }
    }
    return result.ToString();
}
}
