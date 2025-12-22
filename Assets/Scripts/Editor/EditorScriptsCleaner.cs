using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class EditorScriptsCleaner : EditorWindow
{
    [MenuItem("Scripts/Limpar")]
    public static void CleanAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:MonoScript");
        int changedCount = 0;

        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            // Filtros de segurança
            if (!path.StartsWith("Assets")) continue;
            if (path.Contains("/Plugins/") || path.Contains("/PackageCache/")) continue;
            if (path.Contains("/Tests/") || path.Contains("/Editor/")) continue;

            string full = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!File.Exists(full)) continue;

            try 
            {
                string original = File.ReadAllText(full);
                string cleaned = CleanCode(original);

                if (cleaned != original)
                {
                    File.WriteAllText(full, cleaned);
                    changedCount++;
                }
            }
            catch {}
        }

        AssetDatabase.Refresh();

        if (changedCount > 0)
        {
            EditorFeedback.ShowFeedback("Sucesso", $"Códigos Limpos ({changedCount} arquivos)", true);
        }
        else
        {
            // Se rodou e não achou nada pra limpar, tecnicamente não é uma "falha" do script,
            // mas como você pediu a mensagem específica de falha/erro caso não limpe:
            EditorFeedback.ShowFeedback("Info", "Nenhum código precisou ser limpo", false); 
        }
    }

    // --- Lógica de Limpeza Mantida ---
    public static string CleanCode(string code)
    {
        code = RemoveCommentsSafe(code);
        code = RemoveDebugs(code);
        code = NormalizeWhitespace(code);
        code = IndentCode(code);
        return code;
    }

    static string RemoveDebugs(string input) => Regex.Replace(input, @"^\s*(Debug\.[A-Za-z]+|print)\s*\(.*?\);\s*$", "", RegexOptions.Multiline);

    static string NormalizeWhitespace(string code)
    {
        var lines = code.Split('\n');
        StringBuilder sb = new StringBuilder();
        foreach (var l in lines)
        {
            string t = l.TrimEnd();
            if (t.Length == 0) { if (sb.Length > 0 && sb[sb.Length - 1] != '\n') sb.Append("\n"); }
            else sb.AppendLine(t);
        }
        return sb.ToString();
    }

    static string IndentCode(string code)
    {
        var lines = code.Split('\n');
        StringBuilder sb = new StringBuilder();
        int indent = 0;
        foreach (var raw in lines)
        {
            string line = raw.Trim();
            if (line.StartsWith("}")) indent = Mathf.Max(0, indent - 1);
            sb.Append(new string(' ', indent * 4));
            sb.AppendLine(line);
            if (line.EndsWith("{")) indent++;
        }
        return sb.ToString();
    }

    static string RemoveCommentsSafe(string code)
    {
        StringBuilder result = new StringBuilder();
        bool inString = false, inChar = false, inLineComment = false, inBlockComment = false;
        for (int i = 0; i < code.Length; i++)
        {
            char c = code[i];
            char n = i < code.Length - 1 ? code[i + 1] : '\0';
            if (inLineComment) { if (c == '\n') { inLineComment = false; result.Append(c); } continue; }
            if (inBlockComment) { if (c == '*' && n == '/') { inBlockComment = false; i++; } continue; }
            if (!inString && !inChar)
            {
                if (c == '/' && n == '/') { inLineComment = true; i++; continue; }
                if (c == '/' && n == '*') { inBlockComment = true; i++; continue; }
            }
            if (c == '"' && !inChar && (i == 0 || code[i - 1] != '\\')) inString = !inString;
            if (c == '\'' && !inString && (i == 0 || code[i - 1] != '\\')) inChar = !inChar;
            result.Append(c);
        }
        return result.ToString();
    }
}
