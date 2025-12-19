using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/Ace")]
public class RuleAce : SOCapture
{
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
        TryCapture(origin, card.top, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y + 1), ownerId, captured, s => s.currentCardView.cardData.bottom);
        TryCapture(origin, card.right, board.GetSlot(origin.gridPosition.x + 1, origin.gridPosition.y), ownerId, captured, s => s.currentCardView.cardData.left);
        TryCapture(origin, card.bottom, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y - 1), ownerId, captured, s => s.currentCardView.cardData.top);
        TryCapture(origin, card.left, board.GetSlot(origin.gridPosition.x - 1, origin.gridPosition.y), ownerId, captured, s => s.currentCardView.cardData.right);
    }
    private void TryCapture(CardSlot origin, int placedVal, CardSlot neigh, int ownerId,
    List<CardSlot> captured, System.Func<CardSlot, int> getNeighVal)
    {
        if (neigh == null || !neigh.IsOccupied) return;
        if (neigh.currentCardView.ownerId == ownerId) return;
        bool wasPlayer = neigh.currentCardView.isPlayerOwner;
        bool isAttackerPlayer = (ownerId == ManagerGame.ID_PLAYER);
        int neighVal = getNeighVal(neigh);
        bool shouldCapture = false;
        if (neighVal == 9 || neighVal == 10)
        {
            shouldCapture = (placedVal == 1 || placedVal == 2);
        }
        else
        {
            shouldCapture = (placedVal > neighVal);
        }
        if (shouldCapture)
        {
            neigh.currentCardView.SetOwnerId(ownerId);
            if (isAttackerPlayer && !wasPlayer)
            UIScoreService.AddPointPlayer();
            else if (!isAttackerPlayer && wasPlayer)
            UIScoreService.AddPointOpponent();
            if (!captured.Contains(neigh))
            captured.Add(neigh);
        }
    }
}

