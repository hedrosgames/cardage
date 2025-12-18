using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema centralizado para desbloquear achievements.
/// Use as constantes de AchievementIds para garantir consistência.
/// </summary>
public static class AchievementSystem
{
    /// <summary>
    /// Desbloqueia um achievement pelo ID.
    /// </summary>
    /// <param name="achievementId">ID do achievement (use AchievementIds para constantes)</param>
    public static void Unlock(string achievementId)
    {
        if (string.IsNullOrEmpty(achievementId))
        {
            Debug.LogWarning("[AchievementSystem] Tentativa de desbloquear achievement com ID vazio!");
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
                Debug.Log($"[AchievementSystem] Achievement desbloqueado: {achievementId}");
            }
        }
        else
        {
            Debug.LogWarning($"[AchievementSystem] SaveClientAchievements não encontrado! Achievement '{achievementId}' não foi salvo.");
        }
    }
    
    /// <summary>
    /// Verifica se um achievement está desbloqueado.
    /// </summary>
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
    
    /// <summary>
    /// Desbloqueia múltiplos achievements de uma vez.
    /// </summary>
    public static void UnlockMultiple(params string[] achievementIds)
    {
        foreach (var id in achievementIds)
        {
            Unlock(id);
        }
    }
    
    /// <summary>
    /// Retorna a lista de todos os achievements desbloqueados.
    /// </summary>
    public static List<string> GetUnlockedAchievements()
    {
        var saveClient = Object.FindFirstObjectByType<SaveClientAchievements>();
        if (saveClient != null)
        {
            return new List<string>(saveClient.unlockedAchievements);
        }
        return new List<string>();
    }
    
    /// <summary>
    /// Retorna o total de achievements desbloqueados.
    /// </summary>
    public static int GetUnlockedCount()
    {
        return GetUnlockedAchievements().Count;
    }
}

