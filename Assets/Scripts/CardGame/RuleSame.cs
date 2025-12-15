using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/Same")]
public class RuleSame : SOCapture
{
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
        List<(CardSlot slot, int placedVal, int neighVal)> matches = new List<(CardSlot, int, int)>();
        TryMatch(origin, card.top, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y + 1), matches, s => s.currentCardView.cardData.bottom);
        TryMatch(origin, card.right, board.GetSlot(origin.gridPosition.x + 1, origin.gridPosition.y), matches, s => s.currentCardView.cardData.left);
        TryMatch(origin, card.bottom, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y - 1), matches, s => s.currentCardView.cardData.top);
        TryMatch(origin, card.left, board.GetSlot(origin.gridPosition.x - 1, origin.gridPosition.y), matches, s => s.currentCardView.cardData.right);
        if (matches.Count < 2)
        return;
        bool sameTriggered = false;
        bool isAttackerPlayer = (ownerId == ManagerGame.ID_PLAYER);
        List<CardSlot> involvedSlots = new List<CardSlot>();
        foreach (var m in matches)
        {
            if (m.slot.currentCardView.ownerId != ownerId)
            {
                bool wasPlayer = m.slot.currentCardView.isPlayerOwner;
                m.slot.currentCardView.SetOwnerId(ownerId);
                if (ManagerGame.Instance != null)
                {
                    if (isAttackerPlayer && !wasPlayer)
                    UIScoreService.AddPointPlayer();
                    else if (!isAttackerPlayer && wasPlayer)
                    UIScoreService.AddPointOpponent();
                }
                if (!captured.Contains(m.slot))
                captured.Add(m.slot);
                involvedSlots.Add(m.slot);
                sameTriggered = true;
            }
        }
        if (sameTriggered)
        {
            GameEvents.OnSameTriggered?.Invoke(involvedSlots);
        }
    }
    private void TryMatch(CardSlot origin, int placedVal, CardSlot neigh,
    List<(CardSlot, int, int)> matches, System.Func<CardSlot, int> getNeighVal)
    {
        if (neigh == null || !neigh.IsOccupied) return;
        int neighVal = getNeighVal(neigh);
        if (placedVal == neighVal)
        matches.Add((neigh, placedVal, neighVal));
    }
}

