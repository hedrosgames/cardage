using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(menuName = "Game/AI/Greedy")]
public class AIGreedyBehavior : AIBehaviorBase
{
    private CardSlot pendingSlotDecision;
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        pendingSlotDecision = null;
        var availableCards = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (availableCards.Count == 0) return null;
        if (ManagerGame.Instance == null) return availableCards[0];
        CardSlot[] board = ManagerGame.Instance.GetBoard();
        int myId = availableCards[0].cardView.ownerId;
        CardButton bestCard = null;
        CardSlot bestSlot = null;
        int bestScore = -1;
        int bestPower = -1;
        foreach (var cardBtn in availableCards)
        {
            SOCardData cardData = cardBtn.GetCardData();
            int cardPower = cardData.top + cardData.bottom + cardData.left + cardData.right;
            foreach (var slot in board)
            {
                if (slot.IsOccupied) continue;
                int score = SimulateCaptureScore(cardData, slot, board, myId);
                bool isBetter = false;
                if (score > bestScore)
                {
                    isBetter = true;
                }
                else if (score == bestScore)
                {
                    if (cardPower > bestPower)
                    {
                        isBetter = true;
                    }
                    else if (cardPower == bestPower)
                    {
                        if (Random.value > 0.5f) isBetter = true;
                    }
                }
                if (isBetter)
                {
                    bestScore = score;
                    bestPower = cardPower;
                    bestCard = cardBtn;
                    bestSlot = slot;
                }
            }
        }
        pendingSlotDecision = bestSlot;
        if (bestCard == null) return availableCards[Random.Range(0, availableCards.Count)];
        return bestCard;
    }
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        if (pendingSlotDecision != null && !pendingSlotDecision.IsOccupied)
        {
            var target = pendingSlotDecision;
            pendingSlotDecision = null;
            return target;
        }
        var empty = new List<CardSlot>();
        foreach (var slot in board)
        {
            if (!slot.IsOccupied) empty.Add(slot);
        }
        if (empty.Count == 0) return null;
        return empty[Random.Range(0, empty.Count)];
    }
    private int SimulateCaptureScore(SOCardData card, CardSlot targetSlot, CardSlot[] board, int myId)
    {
        int captures = 0;
        int x = targetSlot.gridPosition.x;
        int y = targetSlot.gridPosition.y;
        CheckNeighbor(x, y + 1, card.top, (neighbor) => neighbor.bottom, board, myId, ref captures);
        CheckNeighbor(x + 1, y, card.right, (neighbor) => neighbor.left, board, myId, ref captures);
        CheckNeighbor(x, y - 1, card.bottom, (neighbor) => neighbor.top, board, myId, ref captures);
        CheckNeighbor(x - 1, y, card.left, (neighbor) => neighbor.right, board, myId, ref captures);
        return captures;
    }
    private void CheckNeighbor(int nx, int ny, int myValue, System.Func<SOCardData, int> getEnemyValue, CardSlot[] board, int myId, ref int captures)
    {
        if (nx < 0 || nx > 2 || ny < 0 || ny > 2) return;
        CardSlot neighborSlot = null;
        foreach (var s in board)
        {
            if (s.gridPosition.x == nx && s.gridPosition.y == ny)
            {
                neighborSlot = s;
                break;
            }
        }
        if (neighborSlot == null || !neighborSlot.IsOccupied) return;
        if (neighborSlot.currentCardView.ownerId == myId) return;
        int enemyValue = getEnemyValue(neighborSlot.currentCardView.cardData);
        if (myValue > enemyValue)
        {
            captures++;
        }
    }
}

