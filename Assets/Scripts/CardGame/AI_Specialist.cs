using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Game/AI/Specialist")]
public class AI_Specialist : AIBehaviorBase
{
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        var available = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (available.Count == 0) return null;
        if (ManagerGame.Instance == null) return available[0];
        int myId = available[0].cardView.ownerId;
        CardSlot[] board = ManagerGame.Instance.GetBoard();
        bool useCombo = IsRuleActive<RuleCombo>();
        bool useSame = IsRuleActive<RuleSame>();
        bool usePlus = IsRuleActive<RulePlus>();
        CardButton bestCard = null;
        CardSlot bestSlot = null;
        int maxCaptures = -1;
        foreach (var card in available)
        {
            foreach (var slot in board)
            {
                if (slot.IsOccupied) continue;
                int score = 0;
                List<CardSlot> initialCaptures = SimulateBasicCaptures(card.GetCardData(), slot, board, myId);
                score += initialCaptures.Count;
                if (useCombo && initialCaptures.Count > 0)
                {
                    score += SimulateChainReaction(initialCaptures, board, myId);
                }
                if (score > maxCaptures)
                {
                    maxCaptures = score;
                    bestCard = card;
                    bestSlot = slot;
                }
            }
        }
        if (bestCard == null) return available[0];
        _cachedSlot = bestSlot;
        return bestCard;
    }
    private CardSlot _cachedSlot;
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        if (_cachedSlot != null && !_cachedSlot.IsOccupied)
        {
            var s = _cachedSlot;
            _cachedSlot = null;
            return s;
        }
        var empty = new List<CardSlot>();
        foreach (var s in board) if (!s.IsOccupied) empty.Add(s);
        return empty.Count > 0 ? empty[Random.Range(0, empty.Count)] : null;
    }
    private bool IsRuleActive<T>() where T : SOCapture
    {
        if (ManagerCapture.Instance == null) return false;
        foreach (var r in ManagerCapture.Instance.activeRules)
        if (r is T) return true;
        return false;
    }
    private List<CardSlot> SimulateBasicCaptures(SOCardData card, CardSlot origin, CardSlot[] board, int myId)
    {
        List<CardSlot> captured = new List<CardSlot>();
        Check(origin.gridPosition.x, origin.gridPosition.y + 1, card.top, s => s.currentCardView.cardData.bottom, board, myId, captured);
        Check(origin.gridPosition.x + 1, origin.gridPosition.y, card.right, s => s.currentCardView.cardData.left, board, myId, captured);
        Check(origin.gridPosition.x, origin.gridPosition.y - 1, card.bottom, s => s.currentCardView.cardData.top, board, myId, captured);
        Check(origin.gridPosition.x - 1, origin.gridPosition.y, card.left, s => s.currentCardView.cardData.right, board, myId, captured);
        return captured;
    }
    private void Check(int x, int y, int myVal, System.Func<CardSlot, int> getEnemyVal, CardSlot[] board, int myId, List<CardSlot> captured)
    {
        CardSlot neigh = GetSlot(board, x, y);
        if (neigh == null || !neigh.IsOccupied || neigh.currentCardView.ownerId == myId) return;
        if (myVal > getEnemyVal(neigh)) captured.Add(neigh);
    }
    private int SimulateChainReaction(List<CardSlot> initialCaptures, CardSlot[] board, int myId)
    {
        HashSet<CardSlot> allCaptured = new HashSet<CardSlot>(initialCaptures);
        Queue<CardSlot> queue = new Queue<CardSlot>(initialCaptures);
        int extraCaptures = 0;
        while (queue.Count > 0)
        {
            CardSlot current = queue.Dequeue();
            SOCardData data = current.currentCardView.cardData;
            List<CardSlot> wave = new List<CardSlot>();
            CheckVirtual(current.gridPosition.x, current.gridPosition.y + 1, data.top, s=>s.currentCardView.cardData.bottom, board, myId, allCaptured, wave);
            CheckVirtual(current.gridPosition.x + 1, current.gridPosition.y, data.right, s=>s.currentCardView.cardData.left, board, myId, allCaptured, wave);
            CheckVirtual(current.gridPosition.x, current.gridPosition.y - 1, data.bottom, s=>s.currentCardView.cardData.top, board, myId, allCaptured, wave);
            CheckVirtual(current.gridPosition.x - 1, current.gridPosition.y, data.left, s=>s.currentCardView.cardData.right, board, myId, allCaptured, wave);
            foreach(var s in wave)
            {
                if (!allCaptured.Contains(s))
                {
                    allCaptured.Add(s);
                    queue.Enqueue(s);
                    extraCaptures++;
                }
            }
        }
        return extraCaptures;
    }
    private void CheckVirtual(int x, int y, int val, System.Func<CardSlot, int> getVal, CardSlot[] board, int myId, HashSet<CardSlot> virtualAllies, List<CardSlot> wave)
    {
        CardSlot neigh = GetSlot(board, x, y);
        if (neigh == null || !neigh.IsOccupied) return;
        bool isAlly = (neigh.currentCardView.ownerId == myId) || virtualAllies.Contains(neigh);
        if (isAlly) return;
        if (val > getVal(neigh)) wave.Add(neigh);
    }
    private CardSlot GetSlot(CardSlot[] board, int x, int y)
    {
        if (x < 0 || x > 2 || y < 0 || y > 2) return null;
        foreach (var s in board) if (s.gridPosition.x == x && s.gridPosition.y == y) return s;
        return null;
    }
}

