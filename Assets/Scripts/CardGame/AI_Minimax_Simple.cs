using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Game/AI/MinimaxSimple")]
public class AI_Minimax_Simple : AIBehaviorBase
{
    private CardSlot _chosenSlot;
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        var available = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (available.Count == 0) return null;
        if (ManagerGame.Instance == null) return available[0];
        CardSlot[] board = ManagerGame.Instance.GetBoard();
        int myId = available[0].cardView.ownerId;
        int enemyId = (myId == 0) ? 1 : 0;
        CardButton bestCard = null;
        CardSlot bestSlot = null;
        int bestNetScore = int.MinValue;
        foreach (var myCard in available)
        {
            foreach (var mySlot in board)
            {
                if (mySlot.IsOccupied) continue;
                int myCaptures = CountCaptures(myCard.GetCardData(), mySlot, board, myId);
                int maxEnemyRecovery = 0;
                foreach (var enemySlot in board)
                {
                    if (enemySlot.IsOccupied || enemySlot == mySlot) continue;
                    SOCardData virtualEnemyCard = CreateVirtualCard(6);
                    int enemyCaptures = CountCaptures(virtualEnemyCard, enemySlot, board, enemyId);
                    if (enemyCaptures > maxEnemyRecovery)
                    maxEnemyRecovery = enemyCaptures;
                }
                int netScore = myCaptures - maxEnemyRecovery;
                if (netScore > bestNetScore)
                {
                    bestNetScore = netScore;
                    bestCard = myCard;
                    bestSlot = mySlot;
                }
            }
        }
        _chosenSlot = bestSlot;
        return bestCard ?? available[0];
    }
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        if (_chosenSlot != null && !_chosenSlot.IsOccupied)
        {
            var s = _chosenSlot;
            _chosenSlot = null;
            return s;
        }
        var empty = new List<CardSlot>();
        foreach (var s in board) if (!s.IsOccupied) empty.Add(s);
        return empty.Count > 0 ? empty[0] : null;
    }
    private int CountCaptures(SOCardData card, CardSlot origin, CardSlot[] board, int ownerId)
    {
        int caps = 0;
        Check(origin.gridPosition.x, origin.gridPosition.y + 1, card.top, s => s.currentCardView.cardData.bottom, board, ownerId, ref caps);
        Check(origin.gridPosition.x + 1, origin.gridPosition.y, card.right, s => s.currentCardView.cardData.left, board, ownerId, ref caps);
        Check(origin.gridPosition.x, origin.gridPosition.y - 1, card.bottom, s => s.currentCardView.cardData.top, board, ownerId, ref caps);
        Check(origin.gridPosition.x - 1, origin.gridPosition.y, card.left, s => s.currentCardView.cardData.right, board, ownerId, ref caps);
        return caps;
    }
    private void Check(int x, int y, int myVal, System.Func<CardSlot, int> getEnemyVal, CardSlot[] board, int myId, ref int caps)
    {
        CardSlot neigh = GetSlot(board, x, y);
        if (neigh != null && neigh.IsOccupied && neigh.currentCardView.ownerId != myId)
        {
            if (myVal > getEnemyVal(neigh)) caps++;
        }
    }
    private CardSlot GetSlot(CardSlot[] board, int x, int y)
    {
        if (x < 0 || x > 2 || y < 0 || y > 2) return null;
        foreach (var s in board) if (s.gridPosition.x == x && s.gridPosition.y == y) return s;
        return null;
    }
    private SOCardData CreateVirtualCard(int val)
    {
        var c = CreateInstance<SOCardData>();
        c.top = c.bottom = c.left = c.right = val;
        return c;
    }
}

