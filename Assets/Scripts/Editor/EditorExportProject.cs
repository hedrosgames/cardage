using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class EditorExportProject
{
    [MenuItem("Export/Project")]
    public static void ExportNow()
    {
        string content = GenerateContent();
        content = UltraCompress(content);
        string prompt = GeneratePrompt();
        string formattedPrompt = "\n\n=====PROMPT=====\n" + prompt + "\n=====END PROMPT=====\n";
        content += formattedPrompt;
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string folder = Path.Combine(desktop, "CyberRedCode");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string fileName = "export_" + DateTime.Now.ToString("ddMM-HH-mm") + ".txt";
        string path = Path.Combine(folder, fileName);
        File.WriteAllText(path, content, Encoding.UTF8);
        AssetDatabase.Refresh();
        EditorGUIUtility.systemCopyBuffer = prompt;
        EditorUtility.RevealInFinder(path);
    }

    static string GeneratePrompt()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Você receberá um arquivo de exportação contendo todo o contexto do projeto Unity.");
        sb.AppendLine("Regras importantes:");
        sb.AppendLine("1. Considere o arquivo de exportação como a única fonte de verdade.");
        sb.AppendLine("2. Sempre descartar versões antigas do entendimento do projeto.");
        sb.AppendLine("3. Recriar sua visão do projeto APENAS baseado no arquivo mais recente.");
        sb.AppendLine("4. Quando eu pedir código:");
        sb.AppendLine(" - Sempre entregue arquivos C# completos (com using e classe).");
        sb.AppendLine(" - Nunca use reticências ou placeholders.");
        sb.AppendLine(" - Nunca descreva dentro do bloco de código.");
        sb.AppendLine("5. Ao responder:");
        sb.AppendLine(" - Seja direto e técnico.");
        sb.AppendLine(" - Não elogie, não enrole, não filosofe.");
        sb.AppendLine("6. Nunca quebre a API que já existe no projeto.");
        sb.AppendLine("7. Sempre leia o export inteiro antes de responder qualquer dúvida técnica.");
        return sb.ToString();
    }

    static string UltraCompress(string s)
    {
        s = RemoveEmptyLines(s);
        s = MinifyJSON(s);
        s = RemoveIndentation(s);
        s = RemoveExtraSpaces(s);
        return s;
    }

    static string RemoveEmptyLines(string s)
    {
        StringBuilder sb = new StringBuilder();
        using (StringReader r = new StringReader(s))
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                string t = line.Trim();
                if (t.Length > 0) sb.AppendLine(t);
            }
        }
        return sb.ToString();
    }

    static string MinifyJSON(string s)
    {
        s = s.Replace(",", ",");
        s = s.Replace("}", "}");
        return s;
    }

    static string RemoveIndentation(string s)
    {
        return s.Replace("\t", "").Replace("    ", "");
    }

    static string RemoveExtraSpaces(string s)
    {
        while (s.Contains("  ")) s = s.Replace("  ", " ");
        return s;
    }

    static string GenerateContent()
    {
        StringBuilder sb = new StringBuilder();
        AppendProjectInfo(sb);
        AppendScenes(sb);
        AppendScriptables(sb);
        AppendInput(sb);
        AppendScripts(sb);
        return sb.ToString();
    }

    static void AppendProjectInfo(StringBuilder sb)
    {
        sb.AppendLine("PRJ");
        sb.AppendLine(Application.productName);
        sb.AppendLine(Application.companyName);
        sb.AppendLine(Application.unityVersion);
        sb.AppendLine(EditorUserBuildSettings.activeBuildTarget.ToString());
        sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    static void AppendScenes(StringBuilder sb)
    {
        sb.AppendLine("SCN");
        var buildScenes = EditorBuildSettings.scenes ?? new EditorBuildSettingsScene[0];
        HashSet<string> scenePaths = new HashSet<string>();
        HashSet<string> buildPaths = new HashSet<string>();

        foreach (var s in buildScenes)
        {
            if (s.enabled && s.path != null)
            {
                scenePaths.Add(s.path);
                buildPaths.Add(s.path);
            }
        }

        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (!string.IsNullOrEmpty(p)) scenePaths.Add(p);
        }

        var ordered = scenePaths.ToList();
        ordered.Sort(StringComparer.OrdinalIgnoreCase);
        string originalScenePath = SceneManager.GetActiveScene().path;

        foreach (var p in ordered)
        {
            bool inBuild = buildPaths.Contains(p);
            try
            {
                Scene scene;
                bool isOriginal = (p == originalScenePath);
                
                if (isOriginal)
                {
                    scene = SceneManager.GetActiveScene();
                }
                else
                {
                    scene = EditorSceneManager.OpenScene(p, OpenSceneMode.Additive);
                }

                sb.AppendLine("S:" + p + (inBuild ? "|B" : ""));
                foreach (var root in scene.GetRootGameObjects())
                {
                    var monos = root.GetComponentsInChildren<MonoBehaviour>(true);
                    List<string> names = new List<string>();
                    foreach (var m in monos)
                    {
                        if (m == null) continue;
                        var t = m.GetType();
                        if (t.Namespace != null && t.Namespace.StartsWith("UnityEngine")) continue;
                        names.Add(t.Name);
                    }
                    if (names.Count > 0) sb.AppendLine("R:" + root.name + "|" + string.Join(",", names.Distinct()));
                    else sb.AppendLine("R:" + root.name);
                }

                if (!isOriginal)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            catch
            {
                sb.AppendLine("SERR:" + p);
            }
        }

        if (!string.IsNullOrEmpty(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }
    }

    static void AppendScriptables(StringBuilder sb)
    {
        sb.AppendLine("SO");
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (!p.StartsWith("Assets")) continue;
            if (!p.Contains("/Data/") && !p.Contains("/Config/") && !p.Contains("/SO/") && !p.Contains("/ScriptableObjects/")) continue;
            
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(p);
            if (so == null) continue;

            string json = JsonUtility.ToJson(so, false);
            sb.AppendLine("O:" + so.GetType().Name);
            sb.AppendLine("P:" + p);
            sb.AppendLine(json);
        }
    }

    static void AppendInput(StringBuilder sb)
    {
        sb.AppendLine("IN");
#if ENABLE_INPUT_SYSTEM
        string[] guids = AssetDatabase.FindAssets("t:InputActionAsset");
        foreach (string g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(p);
            if (asset == null) continue;
            sb.AppendLine("IA:" + asset.name);
            sb.AppendLine("P:" + p);
            foreach (var map in asset.actionMaps)
            {
                sb.AppendLine("M:" + map.name);
                foreach (var action in map.actions)
                {
                    sb.AppendLine("A:" + action.name + "|" + action.type);
                    foreach (var bind in action.bindings)
                        sb.AppendLine("B:" + bind.path + "|" + bind.groups);
                }
            }
        }
#else
        sb.AppendLine("NO_INPUT");
#endif
    }

    static void AppendScripts(StringBuilder sb)
    {
        sb.AppendLine("CS");
        string[] guids = AssetDatabase.FindAssets("t:MonoScript");
        List<string> paths = new List<string>();
        foreach (string g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (p.StartsWith("Assets") && p.EndsWith(".cs")) paths.Add(p);
        }
        paths = paths.Distinct().OrderBy(p => p).ToList();
        foreach (var p in paths)
        {
            try
            {
                string c = File.ReadAllText(p);
                c = MinifyCode(c);
                sb.AppendLine("F:" + p);
                sb.AppendLine(c);
                sb.AppendLine("E");
            }
            catch { }
        }
    }

    static string MinifyCode(string code)
    {
        code = code.Replace("\r", "");
        return code;
    }
}
