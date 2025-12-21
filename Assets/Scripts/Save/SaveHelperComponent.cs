using UnityEngine;
public class SaveHelperComponent : MonoBehaviour
{
    public void SaveByEnum(SaveId saveId)
    {
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.SaveByEnum(saveId);
        }
    }
    public void SaveZone()
    {
        SaveByEnum(SaveId.SaveZone);
    }
    public void SaveAchievements()
    {
        SaveByEnum(SaveId.SaveAchievements);
    }
    public void SaveWorld()
    {
        SaveByEnum(SaveId.SaveWorld);
    }
    public void SaveCard()
    {
        SaveByEnum(SaveId.SaveCard);
    }
    public void SaveSettings()
    {
        SaveByEnum(SaveId.SaveSettings);
    }
    public void SaveMenu()
    {
        SaveByEnum(SaveId.SaveMenu);
    }
    public void SaveQuit()
    {
        SaveByEnum(SaveId.SaveQuit);
    }
    public void SaveMoment()
    {
        SaveByEnum(SaveId.SaveMoment);
    }
    public void SetFlag(SOZoneFlag flag)
    {
        if (flag == null) return;
        SaveClientZone saveZone = FindFirstObjectByType<SaveClientZone>();
        if (saveZone != null)
        {
            saveZone.SetFlag(flag, 1);
            SaveZone();
        }
    }
    public void SetFlagValue(SOZoneFlag flag, int value)
    {
        if (flag == null) return;
        SaveClientZone saveZone = FindFirstObjectByType<SaveClientZone>();
        if (saveZone != null)
        {
            saveZone.SetFlag(flag, value);
            SaveZone();
        }
    }
    public void SetMomentFlag(SOZoneFlag flag, int value)
    {
        if (flag == null) return;
        SaveClientMoment saveMoment = FindFirstObjectByType<SaveClientMoment>();
        if (saveMoment != null)
        {
            saveMoment.SetFlag(flag, value);
            SaveMoment();
        }
        else
        {
        }
    }
    public void SetMomentFlagAndLoad(SOZoneFlag flag)
    {
        if (flag == null) return;
        SaveClientMoment saveMoment = FindFirstObjectByType<SaveClientMoment>();
        if (saveMoment != null)
        {
            saveMoment.SetFlag(flag, 1);
            SaveMoment();
            if (ManagerSave.Instance != null)
            {
                ManagerSave.Instance.LoadMoment();
            }
        }
        else
        {
        }
    }
    public void SetMomentFlagAndLoad(SOZoneFlag flag, int value)
    {
        if (flag == null) return;
        SaveClientMoment saveMoment = FindFirstObjectByType<SaveClientMoment>();
        if (saveMoment != null)
        {
            saveMoment.SetFlag(flag, value);
            SaveMoment();
            if (ManagerSave.Instance != null)
            {
                ManagerSave.Instance.LoadMoment();
            }
        }
        else
        {
        }
    }
}

