using UnityEngine;
using System.Collections.Generic;
public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance { get; private set; }
    private Dictionary<CardSlot, int> cardsInFieldTurns = new Dictionary<CardSlot, int>();
    private SOCardData lastPlayedCard;
    private int lastPlayedOwnerId;
    private CardSlot lastPlayedSlot;
    private Dictionary<int, bool> lostCardLastTurn = new Dictionary<int, bool>();
    private Dictionary<int, List<CardSlot>> capturedCardsByOwner = new Dictionary<int, List<CardSlot>>();
    private Dictionary<int, int> sacrificeBonuses = new Dictionary<int, int>();
    private Dictionary<CardSlot, bool> cardsThatCaptured = new Dictionary<CardSlot, bool>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        GameEvents.OnCardPlayed += HandleCardPlayed;
        GameEvents.OnCardsCaptured += HandleCardsCaptured;
        GameEvents.OnTurnChanged += HandleTurnChanged;
        GameEvents.OnMatchReset += HandleMatchReset;
    }
    private void OnDisable()
    {
        GameEvents.OnCardPlayed -= HandleCardPlayed;
        GameEvents.OnCardsCaptured -= HandleCardsCaptured;
        GameEvents.OnTurnChanged -= HandleTurnChanged;
        GameEvents.OnMatchReset -= HandleMatchReset;
    }
    private void HandleCardPlayed(CardSlot slot, SOCardData card, int ownerId)
    {
        lastPlayedCard = card;
        lastPlayedOwnerId = ownerId;
        lastPlayedSlot = slot;
        if (slot != null && !cardsInFieldTurns.ContainsKey(slot))
        {
            cardsInFieldTurns[slot] = 0;
        }
    }
    public CardSlot GetLastPlayedSlot()
    {
        return lastPlayedSlot;
    }
    private void HandleCardsCaptured(List<CardSlot> captured, int ownerId)
    {
        if (!capturedCardsByOwner.ContainsKey(ownerId))
        capturedCardsByOwner[ownerId] = new List<CardSlot>();
        foreach (var slot in captured)
        {
            if (!capturedCardsByOwner[ownerId].Contains(slot))
            capturedCardsByOwner[ownerId].Add(slot);
            if (slot.currentCardView != null)
            {
                int previousOwner = slot.currentCardView.ownerId == ManagerGame.ID_PLAYER
                ? ManagerGame.ID_OPPONENT
                : ManagerGame.ID_PLAYER;
                MarkLostCard(previousOwner);
            }
        }
    }
    public void MarkCardCaptured(CardSlot cardSlot)
    {
        if (cardSlot != null)
        cardsThatCaptured[cardSlot] = true;
    }
    public bool DidCardCapture(CardSlot cardSlot)
    {
        return cardSlot != null && cardsThatCaptured.ContainsKey(cardSlot) && cardsThatCaptured[cardSlot];
    }
    private void HandleTurnChanged(int turnNumber)
    {
        List<CardSlot> toRemove = new List<CardSlot>();
        foreach (var kvp in cardsInFieldTurns)
        {
            if (kvp.Key == null || !kvp.Key.IsOccupied)
            {
                toRemove.Add(kvp.Key);
            }
            else
            {
                cardsInFieldTurns[kvp.Key]++;
            }
        }
        foreach (var slot in toRemove)
        {
            cardsInFieldTurns.Remove(slot);
        }
        lostCardLastTurn.Clear();
    }
    private void HandleMatchReset()
    {
        cardsInFieldTurns.Clear();
        lastPlayedCard = null;
        lastPlayedOwnerId = -1;
        lastPlayedSlot = null;
        lostCardLastTurn.Clear();
        capturedCardsByOwner.Clear();
        sacrificeBonuses.Clear();
        cardsThatCaptured.Clear();
    }
    public int GetSacrificeBonus(int ownerId)
    {
        return sacrificeBonuses.ContainsKey(ownerId) ? sacrificeBonuses[ownerId] : 0;
    }
    public void AddSacrificeBonus(int ownerId, int amount)
    {
        if (!sacrificeBonuses.ContainsKey(ownerId))
        sacrificeBonuses[ownerId] = 0;
        sacrificeBonuses[ownerId] += amount;
    }
    public int GetTurnsInField(CardSlot slot)
    {
        if (slot == null || !cardsInFieldTurns.ContainsKey(slot))
        return 0;
        return cardsInFieldTurns[slot];
    }
    public bool WasLastCardMagic(int ownerId)
    {
        return lastPlayedCard != null &&
        lastPlayedOwnerId == ownerId &&
        lastPlayedCard.subType == CardSubType.Magic;
    }
    public bool LostCardLastTurn(int ownerId)
    {
        return lostCardLastTurn.ContainsKey(ownerId) && lostCardLastTurn[ownerId];
    }
    public void MarkLostCard(int ownerId)
    {
        lostCardLastTurn[ownerId] = true;
    }
}

