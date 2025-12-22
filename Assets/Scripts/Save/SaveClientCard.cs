using UnityEngine;
public class SaveClientCard : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    public SOGameSetup setup;
    public SORuleSet ruleSet;
    [System.Serializable]
    class Data
    {
        public string deckId;
        public string opponentId;
        public string[] rules;
    }
    void OnEnable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.RegisterClient(saveDefinition, this);
    }
    void OnDisable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.UnregisterClient(saveDefinition, this);
    }
    public string Save(SOSaveDefinition definition)
    {
        if (ManagerSave.Instance != null) ManagerSave.Instance.RegisterClient(saveDefinition, this);
        var d = new Data();
        if (setup != null)
        {
            if (setup.playerDeck != null)
            d.deckId = setup.playerDeck.name;
            if (setup.opponent != null)
            d.opponentId = setup.opponent.name;
        }
        if (ruleSet != null && ruleSet.activeRules != null)
        {
            d.rules = new string[ruleSet.activeRules.Length];
            for (int i = 0; i < ruleSet.activeRules.Length; i++)
            {
                var r = ruleSet.activeRules[i];
                d.rules[i] = r != null ? r.name : string.Empty;
            }
        }
        return JsonUtility.ToJson(d);
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        var d = JsonUtility.FromJson<Data>(json);
        if (d == null) return;
        if (setup != null)
        {
            if (!string.IsNullOrEmpty(d.deckId))
            {
                var newDeck = Resources.Load<SODeckData>(d.deckId);
                if (newDeck != null)
                setup.playerDeck = newDeck;
            }
            if (!string.IsNullOrEmpty(d.opponentId))
            {
                var newOpp = Resources.Load<SOOpponentData>(d.opponentId);
                if (newOpp != null)
                setup.opponent = newOpp;
            }
        }
        if (ruleSet != null && d.rules != null)
        {
            ruleSet.activeRules = new SOCapture[d.rules.Length];
            for (int i = 0; i < d.rules.Length; i++)
            {
                var r = Resources.Load<SOCapture>(d.rules[i]);
                ruleSet.activeRules[i] = r;
            }
        }
    }
}

