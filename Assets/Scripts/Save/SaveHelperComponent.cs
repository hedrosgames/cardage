using UnityEngine;
public class SaveHelperComponent : MonoBehaviour
{
    public void Save(SaveId id)
    {
        SaveHelper.Save(id);
    }
    public void SetZoneFlag(SOZoneFlag flag) => SaveHelper.SetZoneFlag(flag, 1);
    public void SetZoneFlagValue(SOZoneFlag flag, int value) => SaveHelper.SetZoneFlag(flag, value);
    public void SetFlowFlag(SOGameFlowFlag flag) => SaveHelper.SetFlowFlag(flag, 1);
    public void SetFlowFlagValue(SOGameFlowFlag flag, int value) => SaveHelper.SetFlowFlag(flag, value);
    public void SetFlowFlagString(string flagId) => SaveHelper.SetFlowFlag(flagId, 1);
    [System.Obsolete("Use SetZoneFlag em vez disso")]
    public void SetFlag(SOZoneFlag flag) => SetZoneFlag(flag);
    [System.Obsolete("Use SetZoneFlagValue em vez disso")]
    public void SetFlagValue(SOZoneFlag flag, int value) => SetZoneFlagValue(flag, value);
    [System.Obsolete("Use SetFlowFlagString em vez disso")]
    public void SetFlowFlag(string flagId) => SetFlowFlagString(flagId);
    [System.Obsolete("Use Save(SaveId.SaveWorld)")]
    public void SaveWorld() => Save(SaveId.SaveWorld);
    [System.Obsolete("Use Save(SaveId.SaveZone)")]
    public void SaveZone() => Save(SaveId.SaveZone);
    [System.Obsolete("Use Save(SaveId.SaveCard)")]
    public void SaveCard() => Save(SaveId.SaveCard);
    [System.Obsolete("Use Save(SaveId.SaveAchievements)")]
    public void SaveAchievements() => Save(SaveId.SaveAchievements);
    [System.Obsolete("Use Save(SaveId.SaveSettings)")]
    public void SaveSettings() => Save(SaveId.SaveSettings);
    [System.Obsolete("Use Save(SaveId.SaveMenu)")]
    public void SaveMenu() => Save(SaveId.SaveMenu);
    [System.Obsolete("Use Save(SaveId.SaveGameFlow)")]
    public void SaveGameFlow() => Save(SaveId.SaveGameFlow);
}

