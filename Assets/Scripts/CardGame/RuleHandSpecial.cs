using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/HandSpecial")]
public class RuleHandSpecial : SOCapture
{
    private Dictionary<int, int> specialCardsUsed = new Dictionary<int, int>();
    public override void OnMatchStart()
    {
        specialCardsUsed.Clear();
        GameEvents.OnCardPlayed += TrackSpecialCard;
        GameEvents.OnMatchReset += ResetTracking;
    }
    private void OnDestroy()
    {
        GameEvents.OnCardPlayed -= TrackSpecialCard;
        GameEvents.OnMatchReset -= ResetTracking;
    }
    private void TrackSpecialCard(CardSlot slot, SOCardData card, int ownerId)
    {
        if (card != null && card.rarity == CardRarity.Special)
        {
            if (!specialCardsUsed.ContainsKey(ownerId))
            specialCardsUsed[ownerId] = 0;
            specialCardsUsed[ownerId]++;
        }
    }
    private void ResetTracking()
    {
        specialCardsUsed.Clear();
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public bool CanPlaySpecialCard(int ownerId)
    {
        if (!specialCardsUsed.ContainsKey(ownerId))
        return true;
        return specialCardsUsed[ownerId] < 1;
    }
}

