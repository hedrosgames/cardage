using UnityEngine;
using System.Collections.Generic;
public static class AchievementSystem
{
    public static void Unlock(string achievementId)
    {
        if (string.IsNullOrEmpty(achievementId))
        {
            return;
        }
        if (ManagerAchievements.Instance != null)
        {
            ManagerAchievements.Instance.UnlockAchievement(achievementId);
            return;
        }
        var saveClient = Object.FindFirstObjectByType<SaveClientAchievements>();
        if (saveClient != null)
        {
            if (!saveClient.IsUnlocked(achievementId))
            {
                saveClient.Unlock(achievementId);
            }
        }
        else
        {
        }
    }
    public static bool IsUnlocked(string achievementId)
    {
        if (string.IsNullOrEmpty(achievementId)) return false;
        var saveClient = Object.FindFirstObjectByType<SaveClientAchievements>();
        if (saveClient != null)
        {
            return saveClient.IsUnlocked(achievementId);
        }
        return false;
    }
    public static void UnlockMultiple(params string[] achievementIds)
    {
        foreach (var id in achievementIds)
        {
            Unlock(id);
        }
    }
    public static List<string> GetUnlockedAchievements()
    {
        var saveClient = Object.FindFirstObjectByType<SaveClientAchievements>();
        if (saveClient != null)
        {
            return new List<string>(saveClient.unlockedAchievements);
        }
        return new List<string>();
    }
    public static int GetUnlockedCount()
    {
        return GetUnlockedAchievements().Count;
    }
}

