using UnityEngine;
using System.Collections.Generic;
public class SlotBonusManager : MonoBehaviour
{
    public static SlotBonusManager Instance { get; private set; }
    private Dictionary<CardSlot, CardGeneralRule> slotBonuses = new Dictionary<CardSlot, CardGeneralRule>();
    private Dictionary<CardSlot, int> corruptionPenalties = new Dictionary<CardSlot, int>();
    private Dictionary<CardSlot, int> betrayalBonuses = new Dictionary<CardSlot, int>();
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
        GameEvents.OnMatchReset += HandleMatchReset;
        GameEvents.OnCardsCaptured += HandleCardsCaptured;
    }
    private void OnDisable()
    {
        GameEvents.OnMatchReset -= HandleMatchReset;
        GameEvents.OnCardsCaptured -= HandleCardsCaptured;
    }
    private void HandleMatchReset()
    {
        slotBonuses.Clear();
        corruptionPenalties.Clear();
        betrayalBonuses.Clear();
    }
    private void HandleCardsCaptured(List<CardSlot> captured, int ownerId)
    {
        foreach (var slot in captured)
        {
            if (slot == null || !slot.IsOccupied) continue;
            if (!corruptionPenalties.ContainsKey(slot))
            corruptionPenalties[slot] = 0;
            corruptionPenalties[slot]--;
            if (!betrayalBonuses.ContainsKey(slot))
            betrayalBonuses[slot] = 0;
            betrayalBonuses[slot]++;
        }
    }
    public void InitializeSlotBonuses(ManagerBoard board, CardGeneralRule bonusType)
    {
        if (board == null) return;
        CardSlot[] slots = board.GetAllSlots();
        if (slots == null || slots.Length == 0) return;
        bool atLeastOne = false;
        List<CardSlot> availableSlots = new List<CardSlot>(slots);
        foreach (var slot in availableSlots)
        {
            if (slot == null) continue;
            if (Random.value <= 0.3f)
            {
                slotBonuses[slot] = bonusType;
                atLeastOne = true;
            }
        }
        if (!atLeastOne && availableSlots.Count > 0)
        {
            CardSlot randomSlot = availableSlots[Random.Range(0, availableSlots.Count)];
            slotBonuses[randomSlot] = bonusType;
        }
    }
    public CardGeneralRule GetSlotBonus(CardSlot slot)
    {
        if (slot == null) return CardGeneralRule.None;
        return slotBonuses.ContainsKey(slot) ? slotBonuses[slot] : CardGeneralRule.None;
    }
    public int GetCorruptionPenalty(CardSlot slot)
    {
        if (slot == null) return 0;
        return corruptionPenalties.ContainsKey(slot) ? corruptionPenalties[slot] : 0;
    }
    public int GetBetrayalBonus(CardSlot slot)
    {
        if (slot == null) return 0;
        return betrayalBonuses.ContainsKey(slot) ? betrayalBonuses[slot] : 0;
    }
}

