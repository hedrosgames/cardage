using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(menuName = "Game/AI/Saver")]
public class AI_Saver : AIBehaviorBase
{
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        var available = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (available.Count == 0) return null;
        var sortedHand = available.OrderBy(c =>
        c.GetCardData().top + c.GetCardData().bottom + c.GetCardData().left + c.GetCardData().right
        ).ToList();
        int emptyCount = 0;
        if (ManagerGame.Instance != null)
        {
            foreach(var s in ManagerGame.Instance.GetBoard()) if (!s.IsOccupied) emptyCount++;
        }
        if (emptyCount > 5)
        {
            return sortedHand[0];
        }
        else
        {
            return sortedHand[sortedHand.Count - 1];
        }
    }
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        var empty = new List<CardSlot>();
        foreach (var s in board) if (!s.IsOccupied) empty.Add(s);
        return empty.Count > 0 ? empty[Random.Range(0, empty.Count)] : null;
    }
}

