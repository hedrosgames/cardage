using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(menuName = "Game/AI/Novice")]
public class AI_Novice : AIBehaviorBase
{
    [Header("Nível de Erro")]
    public float errorRate = 0.4f;
    private CardSlot _chosenSlot;
    private struct Move
    {
        public CardButton card;
        public CardSlot slot;
        public int score;
    }
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        var available = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (available.Count == 0) return null;
        if (ManagerGame.Instance == null) return available[0];
        int myId = available[0].cardView.ownerId;
        CardSlot[] board = ManagerGame.Instance.GetBoard();
        List<Move> possibleMoves = new List<Move>();
        foreach (var card in available)
        {
            SOCardData data = card.GetCardData();
            foreach (var slot in board)
            {
                if (slot.IsOccupied) continue;
                int captures = CountCaptures(data, slot, board, myId);
                possibleMoves.Add(new Move { card = card, slot = slot, score = captures });
            }
        }
        possibleMoves.Sort((a, b) => b.score.CompareTo(a.score));
        Move finalMove;
        if (possibleMoves.Count > 0)
        {
            bool willMakeMistake = Random.value < errorRate;
            if (willMakeMistake && possibleMoves.Count > 1)
            {
                int mistakeIndex = Random.Range(1, Mathf.Min(3, possibleMoves.Count));
                finalMove = possibleMoves[mistakeIndex];
            }
            else
            {
                finalMove = possibleMoves[0];
            }
        }
        else
        {
            return available[0];
        }
        _chosenSlot = finalMove.slot;
        return finalMove.card;
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
}

