using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RuleSideStrong")]
public class RuleSideStrong : SOCapture
{
    public int GetBonus(SOCardData card, CardSlot slot, ManagerBoard board, int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        return IsSideSlot(slot) ? 1 : 0;
    }
    private bool IsSideSlot(CardSlot slot)
    {
        Vector2Int pos = slot.gridPosition;
        bool isCorner = (pos.x == 0 && pos.y == 0) || (pos.x == 2 && pos.y == 0) ||
        (pos.x == 0 && pos.y == 2) || (pos.x == 2 && pos.y == 2);
        bool isCenter = (pos.x == 1 && pos.y == 1);
        return !isCorner && !isCenter;
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
    }
}

