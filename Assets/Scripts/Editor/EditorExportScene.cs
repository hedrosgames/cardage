using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class EditorExportScene : EditorWindow
{
    [MenuItem("Export/Cena")]
    public static void RunExport()
    {
        string content = GenerateSceneDump();
        string prompt = GeneratePrompt();
        
        string finalOutput = prompt + "\n\n" + content;

        // Caminho da Pasta no Desktop
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string folder = Path.Combine(desktop, "CyberRedCode");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        
        // Nome do Arquivo com Timestamp
        string fileName = "SceneExport_" + SceneManager.GetActiveScene().name + "_" + DateTime.Now.ToString("ddMM-HHmm") + ".txt";
        string path = Path.Combine(folder, fileName);
        
        // Salvar Direto
        File.WriteAllText(path, finalOutput, Encoding.UTF8);
        
        // Abrir a pasta onde foi salvo
        EditorUtility.RevealInFinder(path);
        Debug.Log($"<color=green>Cena exportada (Arquivo Salvo): {path}</color>");
    }

    static string GeneratePrompt()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== CONTEXTO DE CENA (SCENE DUMP) =====");
        sb.AppendLine("Este arquivo representa o estado atual da hierarquia e inspector da cena aberta no Unity.");
        sb.AppendLine("Regras importantes:");
        sb.AppendLine("1. Esta é a fonte da verdade sobre como os objetos estão configurados na cena.");
        sb.AppendLine("2. [A] = Ativo, [I] = Inativo (Desativado).");
        sb.AppendLine("3. Scripts customizados mostram suas variáveis públicas/serializadas.");
        sb.AppendLine("4. Componentes padrões da Unity (Transform, Image, Collider) estão resumidos para economizar espaço.");
        sb.AppendLine("=========================================");
        return sb.ToString();
    }

    static string GenerateSceneDump()
    {
        StringBuilder sb = new StringBuilder();
        Scene scene = SceneManager.GetActiveScene();
        
        sb.AppendLine($"SCENE: {scene.name}");
        sb.AppendLine($"PATH: {scene.path}");
        sb.AppendLine(new string('-', 30));

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject go in roots)
        {
            ProcessGameObject(go, sb, 0);
        }

        return sb.ToString();
    }

    static void ProcessGameObject(GameObject go, StringBuilder sb, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 2);
        string state = go.activeSelf ? "[A]" : "[I]";
        
        string posInfo = go.transform.localPosition == Vector3.zero ? "" : $" @ Pos:{go.transform.localPosition}";
        string tagLayer = (go.CompareTag("Untagged") && go.layer == 0) ? "" : $" ({go.tag}|{LayerMask.LayerToName(go.layer)})";
        
        sb.AppendLine($"{indent}{state} {go.name}{tagLayer}{posInfo}");

        Component[] components = go.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null) continue;
            if (comp is Transform) continue; 

            DumpComponent(comp, sb, indent + "  ");
        }

        foreach (Transform child in go.transform)
        {
            ProcessGameObject(child.gameObject, sb, indentLevel + 1);
        }
    }

    static void DumpComponent(Component comp, StringBuilder sb, string indent)
    {
        Type type = comp.GetType();
        string typeName = type.Name;

        // Resumo de componentes "ruidosos" da Unity
        if (IsNoisyUnityComponent(typeName))
        {
            string summary = GetUnityComponentSummary(comp);
            if (!string.IsNullOrEmpty(summary))
                sb.AppendLine($"{indent}- {typeName}: {summary}");
            return;
        }

        // Dump detalhado para scripts customizados
        sb.AppendLine($"{indent}- {typeName}:");
        
        SerializedObject so = new SerializedObject(comp);
        SerializedProperty prop = so.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (prop.name == "m_Script") continue;
            if (prop.name == "m_EditorHideFlags") continue;
            if (prop.name == "m_CorrespondingSourceObject") continue;
            if (prop.name == "m_PrefabInstance") continue;
            if (prop.name == "m_PrefabAsset") continue;
            if (prop.name == "m_GameObject") continue;
            if (prop.name == "m_Enabled") continue;

            string val = GetValueAsString(prop);
            
            if (val != "Null" && val != "0" && val != "" && val != "False [ ]")
            {
                sb.AppendLine($"{indent}  .{prop.displayName}: {val}");
            }
        }
    }

    static bool IsNoisyUnityComponent(string typeName)
    {
        return typeName == "SpriteRenderer" || 
               typeName == "BoxCollider2D" || 
               typeName == "CanvasRenderer" ||
               typeName == "Canvas" ||
               typeName == "CanvasScaler" ||
               typeName == "GraphicRaycaster" ||
               typeName == "Image" ||
               typeName == "AudioSource" ||
               typeName == "Animator" ||
               typeName == "RectTransform"; 
    }

    static string GetUnityComponentSummary(Component comp)
    {
        if (comp is SpriteRenderer sr)
            return sr.sprite != null ? $"[{sr.sprite.name}] Color:{sr.color}" : "No Sprite";
        
        if (comp is UnityEngine.UI.Image img)
            return img.sprite != null ? $"[{img.sprite.name}] Color:{img.color}" : $"Color:{img.color}";
            
        if (comp is BoxCollider2D box)
            return $"Size:{box.size} IsTrigger:{box.isTrigger}";
            
        if (comp is UnityEngine.UI.Text txt)
            return $"Text: \"{txt.text}\"";
            
        if (comp is TMPro.TextMeshProUGUI tmp)
            return $"Text: \"{tmp.text}\" Font:{tmp.font?.name}";

        if (comp is Animator anim)
            return anim.runtimeAnimatorController != null ? $"Controller: {anim.runtimeAnimatorController.name}" : "No Controller";

        return ""; 
    }

    static string GetValueAsString(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer: return prop.intValue.ToString();
            case SerializedPropertyType.Boolean: return prop.boolValue ? "TRUE [X]" : "False [ ]";
            case SerializedPropertyType.Float: return prop.floatValue.ToString("0.##");
            case SerializedPropertyType.String: return string.IsNullOrEmpty(prop.stringValue) ? "" : $"\"{prop.stringValue}\"";
            case SerializedPropertyType.Color: return prop.colorValue.ToString();
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue != null ? $"[{prop.objectReferenceValue.name}]" : "Null";
            case SerializedPropertyType.Enum:
                return prop.enumDisplayNames.Length > 0 && prop.enumValueIndex >= 0 && prop.enumValueIndex < prop.enumDisplayNames.Length 
                    ? prop.enumDisplayNames[prop.enumValueIndex] 
                    : prop.enumValueIndex.ToString();
            case SerializedPropertyType.Vector2: return prop.vector2Value.ToString();
            case SerializedPropertyType.Vector3: return prop.vector3Value.ToString();
            case SerializedPropertyType.LayerMask: return "";
            default: return "";
        }
    }
}
