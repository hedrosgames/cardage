using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RuleAuraEffect")]
public class RuleAuraEffect : SOCapture
{
    public int GetBonus(SOCardData card, CardSlot slot, ManagerBoard board, int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        if (board == null) return 0;
        Vector2Int pos = slot.gridPosition;
        CardSlot[] neighbors = {
            board.GetSlot(pos.x, pos.y + 1),
            board.GetSlot(pos.x + 1, pos.y),
            board.GetSlot(pos.x, pos.y - 1),
            board.GetSlot(pos.x - 1, pos.y)
        };
        foreach (var neigh in neighbors)
        {
            if (neigh != null && neigh.IsOccupied && neigh.currentCardView.ownerId != ownerId)
            return 1;
        }
        return 0;
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
    }
}

