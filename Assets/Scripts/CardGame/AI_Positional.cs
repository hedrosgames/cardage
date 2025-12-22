using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Game/AI/Positional")]
public class AI_Positional : AIBehaviorBase
{
    private int[,] priorityMap = new int[3, 3] {
        { 3, 2, 3 },
        { 2, 1, 2 },
        { 3, 2, 3 }
    };
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        var available = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (available.Count == 0) return null;
        available.Sort((a, b) => {
            int pA = a.GetCardData().top + a.GetCardData().bottom + a.GetCardData().left + a.GetCardData().right;
            int pB = b.GetCardData().top + b.GetCardData().bottom + b.GetCardData().left + b.GetCardData().right;
            return pB.CompareTo(pA);
        });
        return available[0];
    }
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        List<CardSlot> bestSlots = new List<CardSlot>();
        int maxPriority = -1;
        foreach (var slot in board)
        {
            if (slot.IsOccupied) continue;
            int p = priorityMap[slot.gridPosition.y, slot.gridPosition.x];
            if (p > maxPriority)
            {
                maxPriority = p;
                bestSlots.Clear();
                bestSlots.Add(slot);
            }
            else if (p == maxPriority)
            {
                bestSlots.Add(slot);
            }
        }
        if (bestSlots.Count == 0) return null;
        return bestSlots[Random.Range(0, bestSlots.Count)];
    }
}

