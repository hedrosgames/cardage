using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/PlusWall")]
public class RulePlusWall : SOCapture
{
    private const int WALL_VALUE = 10;
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
        Dictionary<CardSlot, int> sums = new Dictionary<CardSlot, int>();
        TrySumWithWall(origin, card.top, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y + 1), sums, s => s.currentCardView.cardData.bottom, origin.gridPosition.y == 2);
        TrySumWithWall(origin, card.right, board.GetSlot(origin.gridPosition.x + 1, origin.gridPosition.y), sums, s => s.currentCardView.cardData.left, origin.gridPosition.x == 2);
        TrySumWithWall(origin, card.bottom, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y - 1), sums, s => s.currentCardView.cardData.top, origin.gridPosition.y == 0);
        TrySumWithWall(origin, card.left, board.GetSlot(origin.gridPosition.x - 1, origin.gridPosition.y), sums, s => s.currentCardView.cardData.right, origin.gridPosition.x == 0);
        List<CardSlot> matched = new List<CardSlot>();
        foreach (var kvA in sums)
        {
            foreach (var kvB in sums)
            {
                if (kvA.Key == kvB.Key) continue;
                if (kvA.Value == kvB.Value)
                {
                    if (!matched.Contains(kvA.Key)) matched.Add(kvA.Key);
                    if (!matched.Contains(kvB.Key)) matched.Add(kvB.Key);
                }
            }
        }
        bool plusTriggered = false;
        bool isAttackerPlayer = (ownerId == ManagerGame.ID_PLAYER);
        List<CardSlot> involvedSlots = new List<CardSlot>();
        foreach (var slot in matched)
        {
            if (slot.currentCardView.ownerId != ownerId)
            {
                bool wasPlayer = slot.currentCardView.isPlayerOwner;
                slot.currentCardView.SetOwnerId(ownerId);
                if (ManagerGame.Instance != null)
                {
                    if (isAttackerPlayer && !wasPlayer)
                    UIScoreService.AddPointPlayer();
                    else if (!isAttackerPlayer && wasPlayer)
                    UIScoreService.AddPointOpponent();
                }
                if (!captured.Contains(slot)) captured.Add(slot);
                involvedSlots.Add(slot);
                plusTriggered = true;
            }
        }
        if (plusTriggered)
        {
            GameEvents.OnPlusTriggered?.Invoke(involvedSlots);
        }
    }
    private void TrySumWithWall(CardSlot origin, int placedVal, CardSlot neigh, Dictionary<CardSlot, int> sums,
    System.Func<CardSlot, int> getNeighVal, bool isWall)
    {
        if (isWall)
        {
            if (!sums.ContainsKey(origin))
            sums[origin] = 0;
            sums[origin] = placedVal + WALL_VALUE;
        }
        else if (neigh != null && neigh.IsOccupied)
        {
            int neighVal = getNeighVal(neigh);
            sums[neigh] = placedVal + neighVal;
        }
        else if (neigh != null && !neigh.IsOccupied)
        {
            Vector2Int neighPos = new Vector2Int(
            origin.gridPosition.x + (origin.gridPosition.y == 2 ? 0 : (origin.gridPosition.y == 0 ? 0 : 0)),
            origin.gridPosition.y + (origin.gridPosition.x == 2 ? 0 : (origin.gridPosition.x == 0 ? 0 : 0))
            );
            if (neighPos.x < 0 || neighPos.x > 2 || neighPos.y < 0 || neighPos.y > 2)
            {
                if (!sums.ContainsKey(origin))
                sums[origin] = 0;
                sums[origin] = placedVal + WALL_VALUE;
            }
        }
    }
}

