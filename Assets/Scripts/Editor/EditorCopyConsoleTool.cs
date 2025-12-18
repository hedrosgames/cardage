using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Text;
public static class EditorCopyConsoleTool
{
    [MenuItem("Export/Copiar Console")]
    public static void CopyConsoleToClipboard()
    {
        Type logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        Type logEntryType = Type.GetType("UnityEditor.LogEntry,UnityEditor.dll");
        if (logEntriesType == null || logEntryType == null)
        {
            return;
        }
        MethodInfo getCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
        MethodInfo getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (getCountMethod == null || getEntryMethod == null)
        {
            return;
        }
        object logEntry = Activator.CreateInstance(logEntryType);
        FieldInfo messageField = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);
        if (messageField == null)
        messageField = logEntryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public);
        FieldInfo stackTraceField = logEntryType.GetField("stackTrace", BindingFlags.Instance | BindingFlags.Public);
        FieldInfo modeField = logEntryType.GetField("mode", BindingFlags.Instance | BindingFlags.Public);
        int count = (int)getCountMethod.Invoke(null, null);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            object[] args = { i, logEntry };
            getEntryMethod.Invoke(null, args);
            string message = messageField != null ? (string)messageField.GetValue(logEntry) : "Sem Texto";
            string stackTrace = stackTraceField != null ? (string)stackTraceField.GetValue(logEntry) : "";
            int mode = modeField != null ? (int)modeField.GetValue(logEntry) : 0;
            string typeLabel = GetTypeLabel(mode);
            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine($"{typeLabel} {message}");
            if (!string.IsNullOrEmpty(stackTrace))
            sb.AppendLine(stackTrace);
        }
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
    }
    static string GetTypeLabel(int mode)
    {
        if ((mode & 16) != 0) return "[Exception]";
        if ((mode & 1) != 0) return "[Error]";
        if ((mode & 8) != 0) return "[Warning]";
        if ((mode & 4) != 0) return "[Log]";
        if ((mode & 2) != 0) return "[Assert]";
        return "[Log]";
    }
}

