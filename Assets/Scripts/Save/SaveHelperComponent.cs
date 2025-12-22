using UnityEngine;

public class SaveHelperComponent : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"[SaveHelperComponent] Inicializado no objeto: {gameObject.name}");
    }

    // Permite que você chame via UnityEvent passando uma string como "world", "zone", "card", etc.
    public void SaveByName(string saveType)
    {
        Debug.Log($"[SaveHelperComponent] SaveByName chamado via evento com o parâmetro: {saveType}");
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
                Debug.LogWarning($"[SaveHelperComponent] Tipo de save '{saveType}' não reconhecido. Tentando salvar World por padrão.");
                SaveWorld(); 
                break;
        }
    }

    public void SaveByEnum(SaveId saveId)
    {
        Debug.Log($"[SaveHelperComponent] SaveByEnum chamado para: {saveId}");
        SaveHelper.SaveByEnum(saveId);
    }

    public void SaveWorld()
    {
        Debug.Log("[SaveHelperComponent] -> Executando SaveWorld");
        SaveHelper.SaveWorld();
    }

    public void SaveZone()
    {
        Debug.Log("[SaveHelperComponent] -> Executando SaveZone");
        SaveHelper.SaveZone();
    }

    public void SaveCard()
    {
        Debug.Log("[SaveHelperComponent] -> Executando SaveCard (Deck/Inventário)");
        SaveHelper.SaveCard();
    }

    public void SaveAchievements()
    {
        Debug.Log("[SaveHelperComponent] -> Executando SaveAchievements");
        SaveHelper.SaveAchievements();
    }

    public void SaveSettings()
    {
        Debug.Log("[SaveHelperComponent] -> Executando SaveSettings");
        SaveHelper.SaveSettings();
    }

    public void SaveMenu()
    {
        Debug.Log("[SaveHelperComponent] -> Executando SaveMenu");
        SaveHelper.SaveByEnum(SaveId.SaveMenu);
    }

    public void SaveQuit()
    {
        Debug.Log("[SaveHelperComponent] -> Executando SaveQuit");
        SaveHelper.SaveByEnum(SaveId.SaveQuit);
    }

    public void SaveMoment()
    {
        Debug.Log("[SaveHelperComponent] -> Executando SaveMoment");
        SaveHelper.SaveByEnum(SaveId.SaveMoment);
    }

    public void SetFlag(SOZoneFlag flag)
    {
        if (flag == null) return;
        Debug.Log($"[SaveHelperComponent] Definindo flag: {flag.name} e salvando zona.");
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
        Debug.Log($"[SaveHelperComponent] Definindo flag: {flag.name} com valor {value} e salvando zona.");
        SaveClientZone saveZone = Object.FindFirstObjectByType<SaveClientZone>();
        if (saveZone != null)
        {
            saveZone.SetFlag(flag, value);
            SaveZone();
        }
    }
}
