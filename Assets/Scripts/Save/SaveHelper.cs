using UnityEngine;
public static class SaveHelper
{
    public static void SaveByEnum(SaveId saveId)
    {
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.SaveByEnum(saveId);
        }
    }
    public static void SaveZone()
    {
        SaveByEnum(SaveId.SaveZone);
    }
    public static void SaveAchievements()
    {
        SaveByEnum(SaveId.SaveAchievements);
    }
    public static void SaveWorld()
    {
        SaveByEnum(SaveId.SaveWorld);
    }
    public static void SaveCard()
    {
        SaveByEnum(SaveId.SaveCard);
    }
    public static void SaveSettings()
    {
        SaveByEnum(SaveId.SaveSettings);
    }
    public static void SaveMenu()
    {
        SaveByEnum(SaveId.SaveMenu);
    }
    public static void SaveQuit()
    {
        SaveByEnum(SaveId.SaveQuit);
    }
}

