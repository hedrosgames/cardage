using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/SameWall")]
public class RuleSameWall : SOCapture
{
    private const int WALL_VALUE = 10;
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
        List<(CardSlot slot, int placedVal, int neighVal)> matches = new List<(CardSlot, int, int)>();
        CheckMatch(origin, card.top, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y + 1), matches, s => s.currentCardView.cardData.bottom, origin.gridPosition.y == 2);
        CheckMatch(origin, card.right, board.GetSlot(origin.gridPosition.x + 1, origin.gridPosition.y), matches, s => s.currentCardView.cardData.left, origin.gridPosition.x == 2);
        CheckMatch(origin, card.bottom, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y - 1), matches, s => s.currentCardView.cardData.top, origin.gridPosition.y == 0);
        CheckMatch(origin, card.left, board.GetSlot(origin.gridPosition.x - 1, origin.gridPosition.y), matches, s => s.currentCardView.cardData.right, origin.gridPosition.x == 0);
        if (IsOnEdge(origin))
        {
            if (card.top == WALL_VALUE || card.right == WALL_VALUE ||
            card.bottom == WALL_VALUE || card.left == WALL_VALUE)
            {
            }
        }
        if (matches.Count < 2)
        return;
        bool sameTriggered = false;
        bool isAttackerPlayer = (ownerId == ManagerGame.ID_PLAYER);
        List<CardSlot> involvedSlots = new List<CardSlot>();
        foreach (var m in matches)
        {
            if (m.slot != null && m.slot.IsOccupied && m.slot.currentCardView.ownerId != ownerId)
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
    private void CheckMatch(CardSlot origin, int placedVal, CardSlot neigh,
    List<(CardSlot, int, int)> matches, System.Func<CardSlot, int> getNeighVal, bool isWall)
    {
        if (isWall)
        {
            if (placedVal == WALL_VALUE)
            {
            }
        }
        else if (neigh != null && neigh.IsOccupied)
        {
            int neighVal = getNeighVal(neigh);
            if (placedVal == neighVal)
            matches.Add((neigh, placedVal, neighVal));
        }
        if (neigh != null && neigh.IsOccupied && IsOnEdge(neigh))
        {
            int neighVal = getNeighVal(neigh);
            if (placedVal == WALL_VALUE && neighVal == WALL_VALUE)
            {
                matches.Add((neigh, placedVal, neighVal));
            }
        }
    }
    private bool IsOnEdge(CardSlot slot)
    {
        Vector2Int pos = slot.gridPosition;
        return pos.x == 0 || pos.x == 2 || pos.y == 0 || pos.y == 2;
    }
}

