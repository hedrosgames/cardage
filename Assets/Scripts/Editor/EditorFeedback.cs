using UnityEngine;
using UnityEditor;
public class EditorFeedback : EditorWindow
{
    private static string _title;
    private static string _message;
    private static Color _bgColor;
    private static double _closeTime;
    public static void ShowFeedback(string title, string message, bool success)
    {
        _title = title;
        _message = message;
        _bgColor = success ? new Color(0.2f, 0.8f, 0.2f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);
        _closeTime = EditorApplication.timeSinceStartup + 2.0;
        EditorFeedback window = GetWindow<EditorFeedback>(true);
        window.minSize = new Vector2(400, 120);
        window.maxSize = new Vector2(400, 120);
        window.titleContent = new GUIContent(title);
        Rect main = EditorGUIUtility.GetMainWindowPosition();
        Rect pos = window.position;
        float centerWidth = (main.width - pos.width) * 0.5f;
        float centerHeight = (main.height - pos.height) * 0.5f;
        window.position = new Rect(main.x + centerWidth, main.y + centerHeight, pos.width, pos.height);
        window.ShowPopup();
    }
    private void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), _bgColor);
        EditorGUI.DrawRect(new Rect(5, 5, position.width - 10, position.height - 10), new Color(0.1f, 0.1f, 0.1f, 0.9f));
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = _bgColor }
        };
        GUIStyle msgStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
        GUILayout.BeginArea(new Rect(10, 10, position.width - 20, position.height - 20));
        GUILayout.FlexibleSpace();
        GUILayout.Label(_title.ToUpper(), titleStyle);
        GUILayout.Space(5);
        GUILayout.Label(_message, msgStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
    private void Update()
    {
        if (EditorApplication.timeSinceStartup > _closeTime)
        {
            Close();
        }
        Repaint();
    }
}
