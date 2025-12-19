using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/HandLegend")]
public class RuleHandLegend : SOCapture
{
    private Dictionary<int, int> legendaryCardsUsed = new Dictionary<int, int>();
    public override void OnMatchStart()
    {
        legendaryCardsUsed.Clear();
        GameEvents.OnCardPlayed += TrackLegendaryCard;
        GameEvents.OnMatchReset += ResetTracking;
    }
    private void OnDestroy()
    {
        GameEvents.OnCardPlayed -= TrackLegendaryCard;
        GameEvents.OnMatchReset -= ResetTracking;
    }
    private void TrackLegendaryCard(CardSlot slot, SOCardData card, int ownerId)
    {
        if (card != null && card.rarity == CardRarity.Legendary)
        {
            if (!legendaryCardsUsed.ContainsKey(ownerId))
            legendaryCardsUsed[ownerId] = 0;
            legendaryCardsUsed[ownerId]++;
        }
    }
    private void ResetTracking()
    {
        legendaryCardsUsed.Clear();
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public bool CanPlayLegendaryCard(int ownerId)
    {
        if (!legendaryCardsUsed.ContainsKey(ownerId))
        return true;
        return legendaryCardsUsed[ownerId] < 2;
    }
}

