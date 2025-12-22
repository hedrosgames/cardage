using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RuleTerritory")]
public class RuleTerritory : SOCapture
{
    public int GetBonus(SOCardData card, CardSlot slot, ManagerBoard board, int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        if (HasClanCardAdjacent(slot, board, card.type, ownerId))
        {
            bool hasStrong = card.generalRules != null && card.generalRules.Contains(CardGeneralRule.TerritoryStrong);
            return hasStrong ? 0 : 1;
        }
        return 0;
    }
    private bool HasClanCardAdjacent(CardSlot slot, ManagerBoard board, CardType clanType, int ownerId)
    {
        if (board == null) return false;
        Vector2Int pos = slot.gridPosition;
        CardSlot[] neighbors = {
            board.GetSlot(pos.x, pos.y + 1),
            board.GetSlot(pos.x + 1, pos.y),
            board.GetSlot(pos.x, pos.y - 1),
            board.GetSlot(pos.x - 1, pos.y)
        };
        foreach (var neigh in neighbors)
        {
            if (neigh != null && neigh.IsOccupied &&
            neigh.currentCardView.ownerId == ownerId &&
            neigh.currentCardView.cardData.type == clanType)
            return true;
        }
        return false;
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
    }
}

