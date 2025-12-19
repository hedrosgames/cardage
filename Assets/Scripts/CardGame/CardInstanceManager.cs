using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class CardInstanceManager : MonoBehaviour
{
    public static CardInstanceManager Instance { get; private set; }
    private Dictionary<SOCardData, SOCardData> cardInstances = new Dictionary<SOCardData, SOCardData>();
    private List<SOCardData> allInstances = new List<SOCardData>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("CardInstanceManager");
            Instance = go.AddComponent<CardInstanceManager>();
            DontDestroyOnLoad(go);
        }
    }
    private void OnEnable()
    {
        GameEvents.OnMatchReset += ClearInstances;
    }
    private void OnDisable()
    {
        GameEvents.OnMatchReset -= ClearInstances;
    }
    public SOCardData GetCardInstance(SOCardData originalCard)
    {
        if (originalCard == null) return null;
        if (cardInstances.ContainsKey(originalCard))
        {
            return cardInstances[originalCard];
        }
        SOCardData instance = CloneCard(originalCard);
        cardInstances[originalCard] = instance;
        allInstances.Add(instance);
        return instance;
    }
    private SOCardData CloneCard(SOCardData original)
    {
        SOCardData clone = ScriptableObject.CreateInstance<SOCardData>();
        clone.cardName = original.cardName;
        clone.cardIndex = original.cardIndex;
        clone.rarity = original.rarity;
        clone.type = original.type;
        clone.subType = original.subType;
        clone.collection = original.collection;
        clone.special = original.special;
        clone.triad = original.triad;
        clone.top = original.top;
        clone.right = original.right;
        clone.bottom = original.bottom;
        clone.left = original.left;
        clone.customArt = original.customArt;
        if (original.generalRules != null)
        {
            clone.generalRules = new List<CardGeneralRule>(original.generalRules);
        }
        else
        {
            clone.generalRules = new List<CardGeneralRule>();
        }
        clone.name = $"{original.name}_Instance";
        return clone;
    }
    private void ClearInstances()
    {
        foreach (var instance in allInstances)
        {
            if (instance != null)
            {
                DestroyImmediate(instance);
            }
        }
        cardInstances.Clear();
        allInstances.Clear();
    }
    public void PreCloneDeck(SODeckData deck)
    {
        if (deck == null || deck.cards == null) return;
        foreach (var card in deck.cards)
        {
            if (card != null)
            {
                GetCardInstance(card);
            }
        }
    }
    public void PreCloneDecks(params SODeckData[] decks)
    {
        foreach (var deck in decks)
        {
            PreCloneDeck(deck);
        }
    }
}

