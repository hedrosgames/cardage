using System;
using System.Collections.Generic;
public static class SaveEvents
{
    public static Action RequestSave;
    public static Action RequestLoad;
    public static Action<string> RequestSaveSpecific;
    public static Action<string, Dictionary<string, string>> OnSaveExecuted;
    public static Action<string, Dictionary<string, string>> OnLoadExecuted;
    public static void RaiseSave()
    {
        RequestSave?.Invoke();
    }
    public static void RaiseLoad()
    {
        RequestLoad?.Invoke();
    }
    public static void RaiseSaveSpecific(string saveId)
    {
        RequestSaveSpecific?.Invoke(saveId);
    }
    public static void NotifySaveExecuted(string caller, Dictionary<string, string> savedData)
    {
        OnSaveExecuted?.Invoke(caller, savedData);
    }
    public static void NotifyLoadExecuted(string caller, Dictionary<string, string> loadedData)
    {
        OnLoadExecuted?.Invoke(caller, loadedData);
    }
}

