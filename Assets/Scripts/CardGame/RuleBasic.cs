using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/BasicCapture")]
public class RuleBasic : SOCapture
{
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
        int topVal = CardValueCalculator.GetCardValue(card, origin, board, ownerId, CardDirection.Top, true, captured, false);
        int rightVal = CardValueCalculator.GetCardValue(card, origin, board, ownerId, CardDirection.Right, true, captured, false);
        int bottomVal = CardValueCalculator.GetCardValue(card, origin, board, ownerId, CardDirection.Bottom, true, captured, false);
        int leftVal = CardValueCalculator.GetCardValue(card, origin, board, ownerId, CardDirection.Left, true, captured, false);
        TryCapture(origin, topVal, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y + 1), ownerId, captured, s => GetNeighborValue(s, CardDirection.Bottom, board, ownerId, false));
        TryCapture(origin, rightVal, board.GetSlot(origin.gridPosition.x + 1, origin.gridPosition.y), ownerId, captured, s => GetNeighborValue(s, CardDirection.Left, board, ownerId, false));
        TryCapture(origin, bottomVal, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y - 1), ownerId, captured, s => GetNeighborValue(s, CardDirection.Top, board, ownerId, false));
        TryCapture(origin, leftVal, board.GetSlot(origin.gridPosition.x - 1, origin.gridPosition.y), ownerId, captured, s => GetNeighborValue(s, CardDirection.Right, board, ownerId, false));
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
    }
    private int GetNeighborValue(CardSlot slot, CardDirection direction, ManagerBoard board, int attackerId, bool applyPostCaptureBonuses = false)
    {
        if (slot == null || !slot.IsOccupied) return 0;
        SOCardData card = slot.currentCardView.cardData;
        return CardValueCalculator.GetCardValue(card, slot, board, slot.currentCardView.ownerId, direction, false, null, applyPostCaptureBonuses);
    }
    private void TryCapture(CardSlot origin, int placedVal, CardSlot neigh, int ownerId,
    List<CardSlot> captured, System.Func<CardSlot, int> getNeighVal)
    {
        if (neigh == null || !neigh.IsOccupied) return;
        if (neigh.currentCardView.ownerId == ownerId) return;
        bool wasPlayer = neigh.currentCardView.isPlayerOwner;
        bool isAttackerPlayer = (ownerId == ManagerGame.ID_PLAYER);
        int neighVal = getNeighVal(neigh);
        if (placedVal > neighVal)
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

