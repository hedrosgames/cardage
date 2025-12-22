using UnityEditor;
using UnityEngine;

public class EditorScriptsRecompile
{
    [MenuItem("Scripts/Reload")]
    public static void Reload()
    {
        Debug.Log("🔄 Forçando recompilação dos scripts...");
        
        EditorUtility.RequestScriptReload();
    }
}
