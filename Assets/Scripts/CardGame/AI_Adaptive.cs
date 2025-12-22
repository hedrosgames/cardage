using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Game/AI/Adaptive")]
public class AI_Adaptive : AIBehaviorBase
{
    [Header("ConfiguraÃ§Ã£o")]
    public int safetyMargin = 1;
    private CardSlot _chosenSlot;
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        var available = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (available.Count == 0) return null;
        if (ManagerGame.Instance == null) return available[0];
        int myId = available[0].cardView.ownerId;
        int myScore = (myId == ManagerGame.ID_PLAYER) ? UIScoreService.PlayerScore : UIScoreService.OpponentScore;
        int enemyScore = (myId == ManagerGame.ID_PLAYER) ? UIScoreService.OpponentScore : UIScoreService.PlayerScore;
        bool isWinning = (myScore - enemyScore) >= safetyMargin;
        if (isWinning)
        {
            return RunDefensiveLogic(available, ManagerGame.Instance.GetBoard());
        }
        else
        {
            return RunGreedyLogic(available, ManagerGame.Instance.GetBoard(), myId);
        }
    }
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        if (_chosenSlot != null && !_chosenSlot.IsOccupied)
        {
            var s = _chosenSlot;
            _chosenSlot = null;
            return s;
        }
        List<CardSlot> empty = new List<CardSlot>();
        foreach (var s in board) if (!s.IsOccupied) empty.Add(s);
        return empty.Count > 0 ? empty[Random.Range(0, empty.Count)] : null;
    }
    private CardButton RunGreedyLogic(List<CardButton> hand, CardSlot[] board, int myId)
    {
        CardButton bestCard = null;
        CardSlot bestSlot = null;
        int bestScore = -1;
        int bestPower = -1;
        foreach (var card in hand)
        {
            SOCardData data = card.GetCardData();
            int power = data.top + data.bottom + data.left + data.right;
            foreach (var slot in board)
            {
                if (slot.IsOccupied) continue;
                int score = CountCaptures(data, slot, board, myId);
                bool isBetter = false;
                if (score > bestScore) isBetter = true;
                else if (score == bestScore && power > bestPower) isBetter = true;
                if (isBetter)
                {
                    bestScore = score;
                    bestPower = power;
                    bestCard = card;
                    bestSlot = slot;
                }
            }
        }
        _chosenSlot = bestSlot;
        return bestCard ?? hand[0];
    }
    private CardButton RunDefensiveLogic(List<CardButton> hand, CardSlot[] board)
    {
        CardButton bestCard = null;
        CardSlot bestSlot = null;
        int bestDefense = -1;
        foreach (var card in hand)
        {
            SOCardData data = card.GetCardData();
            foreach (var slot in board)
            {
                if (slot.IsOccupied) continue;
                int defense = 0;
                if (IsExposed(slot.gridPosition.x, slot.gridPosition.y + 1, board)) defense += data.top;
                if (IsExposed(slot.gridPosition.x + 1, slot.gridPosition.y, board)) defense += data.right;
                if (IsExposed(slot.gridPosition.x, slot.gridPosition.y - 1, board)) defense += data.bottom;
                if (IsExposed(slot.gridPosition.x - 1, slot.gridPosition.y, board)) defense += data.left;
                if (defense > bestDefense)
                {
                    bestDefense = defense;
                    bestCard = card;
                    bestSlot = slot;
                }
            }
        }
        _chosenSlot = bestSlot;
        return bestCard ?? hand[0];
    }
    private int CountCaptures(SOCardData card, CardSlot origin, CardSlot[] board, int myId)
    {
        int caps = 0;
        Check(origin.gridPosition.x, origin.gridPosition.y + 1, card.top, s => s.currentCardView.cardData.bottom, board, myId, ref caps);
        Check(origin.gridPosition.x + 1, origin.gridPosition.y, card.right, s => s.currentCardView.cardData.left, board, myId, ref caps);
        Check(origin.gridPosition.x, origin.gridPosition.y - 1, card.bottom, s => s.currentCardView.cardData.top, board, myId, ref caps);
        Check(origin.gridPosition.x - 1, origin.gridPosition.y, card.left, s => s.currentCardView.cardData.right, board, myId, ref caps);
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
    private bool IsExposed(int x, int y, CardSlot[] board)
    {
        if (x < 0 || x > 2 || y < 0 || y > 2) return true;
        foreach (var s in board) if (s.gridPosition.x == x && s.gridPosition.y == y) return !s.IsOccupied;
        return true;
    }
}

