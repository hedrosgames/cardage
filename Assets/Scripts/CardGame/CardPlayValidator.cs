using UnityEngine;
using System.Collections.Generic;
public static class CardPlayValidator
{
    public static bool CanPlayCard(SOCardData card, int ownerId)
    {
        if (card == null) return false;
        if (ManagerCapture.Instance != null)
        {
            foreach (var rule in ManagerCapture.Instance.activeRules)
            {
                if (rule is RuleHandSpecial handSpecial)
                {
                    if (card.rarity == CardRarity.Special && !handSpecial.CanPlaySpecialCard(ownerId))
                    return false;
                }
                else if (rule is RuleHandLegend handLegend)
                {
                    if (card.rarity == CardRarity.Legendary && !handLegend.CanPlayLegendaryCard(ownerId))
                    return false;
                }
            }
        }
        return true;
    }
}

