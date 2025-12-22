using UnityEngine;

public class SaveHelperComponent : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"[SaveHelperComponent] Presente no objeto: {gameObject.name}");
    }

    // Função genérica que centraliza a chamada de salvamento no Manager
    public void SaveByEnum(SaveId saveId)
    {
        Debug.Log($"[SaveHelperComponent] Tentando salvar via Enum: {saveId}");
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.SaveByEnum(saveId);
        }
        else
        {
            Debug.LogError("[SaveHelperComponent] ManagerSave.Instance não encontrado! O save não será realizado.");
        }
    }

    // Salva especificamente os dados da Zona (Progresso local, NPCs derrotados, etc)
    public void SaveZone()
    {
        SaveByEnum(SaveId.SaveZone);
    }

    // Salva conquistas desbloqueadas
    public void SaveAchievements()
    {
        SaveByEnum(SaveId.SaveAchievements);
    }

    // Salva o estado global do mundo
    public void SaveWorld()
    {
        Debug.Log("[SaveHelperComponent] SaveWorld solicitado via Evento Unity.");
        SaveByEnum(SaveId.SaveWorld);
    }

    // Salva o Deck e a coleção de cartas do jogador
    public void SaveCard()
    {
        SaveByEnum(SaveId.SaveCard);
    }

    // Salva configurações de volume, vídeo, etc
    public void SaveSettings()
    {
        SaveByEnum(SaveId.SaveSettings);
    }

    // Salva dados específicos do Menu Principal
    public void SaveMenu()
    {
        SaveByEnum(SaveId.SaveMenu);
    }

    // Executa um salvamento rápido antes de fechar o jogo
    public void SaveQuit()
    {
        SaveByEnum(SaveId.SaveQuit);
    }

    // Salva o "Momento" atual (checkpoint temporário ou estado de cena)
    public void SaveMoment()
    {
        SaveByEnum(SaveId.SaveMoment);
    }

    // Modifica uma Flag de Zona e salva automaticamente a Zona
    public void SetFlag(SOZoneFlag flag)
    {
        if (flag == null) return;
        SaveClientZone saveZone = Object.FindFirstObjectByType<SaveClientZone>();
        if (saveZone != null)
        {
            saveZone.SetFlag(flag, 1);
            SaveZone(); // Garante a persistência após a alteração
        }
    }

    // Define um valor específico para uma Flag de Zona e salva
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

    // Define uma Flag de "Momento" (curto prazo) e salva o Momento
    public void SetMomentFlag(SOZoneFlag flag, int value)
    {
        if (flag == null) return;
        SaveClientMoment saveMoment = Object.FindFirstObjectByType<SaveClientMoment>();
        if (saveMoment != null)
        {
            saveMoment.SetFlag(flag, value);
            SaveMoment();
        }
    }

    // Define flag, salva e recarrega o estado (útil para mudanças de estado imediatas)
    public void SetMomentFlagAndLoad(SOZoneFlag flag)
    {
        if (flag == null) return;
        SaveClientMoment saveMoment = Object.FindFirstObjectByType<SaveClientMoment>();
        if (saveMoment != null)
        {
            saveMoment.SetFlag(flag, 1);
            SaveMoment();
            if (ManagerSave.Instance != null)
            {
                ManagerSave.Instance.LoadMoment();
            }
        }
    }
}