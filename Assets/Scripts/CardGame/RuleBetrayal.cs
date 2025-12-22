using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RuleBetrayal")]
public class RuleBetrayal : SOCapture
{
    public int GetBonus(SOCardData card, CardSlot slot, ManagerBoard board, int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        if (SlotBonusManager.Instance != null)
        return SlotBonusManager.Instance.GetBetrayalBonus(slot);
        return 0;
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
    }
}

