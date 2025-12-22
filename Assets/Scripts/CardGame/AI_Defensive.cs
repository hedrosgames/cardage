using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Game/AI/Defensive")]
public class AI_Defensive : AIBehaviorBase
{
    private CardSlot _decision;
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        var available = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (available.Count == 0) return null;
        if (ManagerGame.Instance == null) return available[0];
        CardSlot[] board = ManagerGame.Instance.GetBoard();
        CardButton bestCard = null;
        CardSlot bestSlot = null;
        int bestDefenseScore = -1;
        foreach (var card in available)
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
                if (defense > bestDefenseScore)
                {
                    bestDefenseScore = defense;
                    bestCard = card;
                    bestSlot = slot;
                }
            }
        }
        _decision = bestSlot;
        return bestCard ?? available[0];
    }
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        if (_decision != null && !_decision.IsOccupied)
        {
            var s = _decision;
            _decision = null;
            return s;
        }
        List<CardSlot> empty = new List<CardSlot>();
        foreach(var s in board) if(!s.IsOccupied) empty.Add(s);
        return empty.Count > 0 ? empty[Random.Range(0, empty.Count)] : null;
    }
    private bool IsExposed(int x, int y, CardSlot[] board)
    {
        if (x < 0 || x > 2 || y < 0 || y > 2) return true;
        foreach(var s in board)
        {
            if (s.gridPosition.x == x && s.gridPosition.y == y)
            return !s.IsOccupied;
        }
        return true;
    }
}

