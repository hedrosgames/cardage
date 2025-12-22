using UnityEngine;
public static class SaveHelper
{
    public static void Save(SaveId id)
    {
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.SaveByEnum(id);
        }
    }
    public static void SetZoneFlag(SOZoneFlag flag, int value = 1)
    {
        var client = UnityEngine.Object.FindFirstObjectByType<SaveClientZone>();
        if (client != null)
        {
            client.SetFlag(flag, value);
            Save(SaveId.SaveZone);
        }
    }
    public static void SetFlowFlag(string flagId, int value = 1)
    {
        var client = UnityEngine.Object.FindFirstObjectByType<SaveClientGameFlow>();
        if (client != null)
        {
            client.SetFlag(flagId, value);
        }
    }
    public static void SetFlowFlag(SOGameFlowFlag flag, int value = 1)
    {
        if (flag == null) return;
        SetFlowFlag(flag.id, value);
    }
    public static bool HasFlowFlag(string flagId)
    {
        var client = UnityEngine.Object.FindFirstObjectByType<SaveClientGameFlow>();
        return client != null && client.HasFlag(flagId);
    }
    public static bool HasFlowFlag(SOGameFlowFlag flag)
    {
        if (flag == null) return false;
        return HasFlowFlag(flag.id);
    }
    [System.Obsolete("Use Save(SaveId.SaveZone)")]
    public static void SaveZone() => Save(SaveId.SaveZone);
    [System.Obsolete("Use Save(SaveId.SaveAchievements)")]
    public static void SaveAchievements() => Save(SaveId.SaveAchievements);
    [System.Obsolete("Use Save(SaveId.SaveWorld)")]
    public static void SaveWorld() => Save(SaveId.SaveWorld);
    [System.Obsolete("Use Save(SaveId.SaveCard)")]
    public static void SaveCard() => Save(SaveId.SaveCard);
    [System.Obsolete("Use Save(SaveId.SaveSettings)")]
    public static void SaveSettings() => Save(SaveId.SaveSettings);
    [System.Obsolete("Use Save(SaveId.SaveMenu)")]
    public static void SaveMenu() => Save(SaveId.SaveMenu);
    [System.Obsolete("Use Save(SaveId.SaveGameFlow)")]
    public static void SaveGameFlow() => Save(SaveId.SaveGameFlow);
    [System.Obsolete("Use Save(SaveId.SaveMoment)")]
    public static void SaveMoment() => Save(SaveId.SaveGameFlow);
}

