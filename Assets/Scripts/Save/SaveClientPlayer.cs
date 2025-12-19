using UnityEngine;
public class SaveClientPlayer : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    public SOPlayerData playerData;
    [System.Serializable]
    class Data
    {
        public string playerName;
        public int gender;
        public float colorR;
        public float colorG;
        public float colorB;
        public float colorA;
    }
    void OnEnable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.RegisterClient(saveDefinition, this);
        }
    }
    void OnDisable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.UnregisterClient(saveDefinition, this);
        }
    }
    public string Save(SOSaveDefinition definition)
    {
        if (playerData == null) return string.Empty;
        var d = new Data
        {
            playerName = playerData.playerName ?? "Player",
            gender = (int)playerData.gender,
            colorR = playerData.characterColor.r,
            colorG = playerData.characterColor.g,
            colorB = playerData.characterColor.b,
            colorA = playerData.characterColor.a
        };
        return JsonUtility.ToJson(d);
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        if (playerData == null) return;
        if (string.IsNullOrEmpty(json))
        {
            playerData.playerName = "Player";
            playerData.gender = PlayerGender.Male;
            playerData.characterColor = Color.white;
            return;
        }
        var d = JsonUtility.FromJson<Data>(json);
        if (d != null)
        {
            playerData.playerName = d.playerName ?? "Player";
            playerData.gender = (PlayerGender)Mathf.Clamp(d.gender, 0, 1);
            playerData.characterColor = new Color(d.colorR, d.colorG, d.colorB, d.colorA);
        }
    }
}

