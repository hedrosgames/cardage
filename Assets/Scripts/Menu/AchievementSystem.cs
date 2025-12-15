using UnityEngine;
public static class AchievementSystem
{
    public static void Unlock(string achievementId)
    {
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
}

