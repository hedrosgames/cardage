using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RuleCenterStrong")]
public class RuleCenterStrong : SOCapture
{
    public int GetBonus(SOCardData card, CardSlot slot, ManagerBoard board, int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        return IsCenterSlot(slot) ? 1 : 0;
    }
    private bool IsCenterSlot(CardSlot slot)
    {
        return slot.gridPosition.x == 1 && slot.gridPosition.y == 1;
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
    }
}

