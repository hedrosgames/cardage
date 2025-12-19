using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RulePenalty2")]
public class RulePenalty2 : SOCapture
{
    public int GetBonus(SOCardData card, CardSlot slot, ManagerBoard board, int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        if (SlotBonusManager.Instance != null)
        {
            CardGeneralRule slotBonus = SlotBonusManager.Instance.GetSlotBonus(slot);
            if (slotBonus == CardGeneralRule.Penalty2)
            return -2;
        }
        return 0;
    }
    public override void OnMatchStart()
    {
        ManagerBoard board = ManagerGame.Instance != null ? ManagerGame.Instance.boardManager : FindFirstObjectByType<ManagerBoard>();
        if (SlotBonusManager.Instance != null && board != null)
        {
            SlotBonusManager.Instance.InitializeSlotBonuses(board, CardGeneralRule.Penalty2);
        }
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
    }
}

