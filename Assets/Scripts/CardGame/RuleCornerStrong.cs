using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RuleCornerStrong")]
public class RuleCornerStrong : SOCapture
{
    public int GetBonus(SOCardData card, CardSlot slot, ManagerBoard board, int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        return IsCornerSlot(slot) ? 1 : 0;
    }
    private bool IsCornerSlot(CardSlot slot)
    {
        Vector2Int pos = slot.gridPosition;
        return (pos.x == 0 && pos.y == 0) || (pos.x == 2 && pos.y == 0) ||
        (pos.x == 0 && pos.y == 2) || (pos.x == 2 && pos.y == 2);
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
    }
}

