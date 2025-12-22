using UnityEngine;
public class SaveHelperComponent : MonoBehaviour
{
    void Awake()
    {
    }
    public void SaveByName(string saveType)
    {
        switch (saveType.ToLower())
        {
            case "world": SaveWorld(); break;
            case "zone": SaveZone(); break;
            case "card": SaveCard(); break;
            case "inventory": SaveCard(); break;
            case "settings": SaveSettings(); break;
            case "achievements": SaveAchievements(); break;
            case "moment": SaveMoment(); break;
            case "menu": SaveMenu(); break;
            case "quit": SaveQuit(); break;
            default:
            SaveWorld();
            break;
        }
    }
    public void SaveByEnum(SaveId saveId)
    {
        SaveHelper.SaveByEnum(saveId);
    }
    public void SaveWorld()
    {
        SaveHelper.SaveWorld();
    }
    public void SaveZone()
    {
        SaveHelper.SaveZone();
    }
    public void SaveCard()
    {
        SaveHelper.SaveCard();
    }
    public void SaveAchievements()
    {
        SaveHelper.SaveAchievements();
    }
    public void SaveSettings()
    {
        SaveHelper.SaveSettings();
    }
    public void SaveMenu()
    {
        SaveHelper.SaveByEnum(SaveId.SaveMenu);
    }
    public void SaveQuit()
    {
        SaveHelper.SaveByEnum(SaveId.SaveQuit);
    }
    public void SaveMoment()
    {
        SaveHelper.SaveByEnum(SaveId.SaveMoment);
    }
    public void SetFlag(SOZoneFlag flag)
    {
        if (flag == null) return;
        SaveClientZone saveZone = Object.FindFirstObjectByType<SaveClientZone>();
        if (saveZone != null)
        {
            saveZone.SetFlag(flag, 1);
            SaveZone();
        }
    }
    public void SetFlagValue(SOZoneFlag flag, int value)
    {
        if (flag == null) return;
        SaveClientZone saveZone = Object.FindFirstObjectByType<SaveClientZone>();
        if (saveZone != null)
        {
            saveZone.SetFlag(flag, value);
            SaveZone();
        }
    }
}

