using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Game/AI/Random")]
public class AIRandomBehavior : AIBehaviorBase
{
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        var available = hand.FindAll(c => c != null && c.gameObject.activeSelf);
        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        var empty = new System.Collections.Generic.List<CardSlot>();
        foreach (var slot in board)
        {
            if (!slot.IsOccupied) empty.Add(slot);
        }
        if (empty.Count == 0) return null;
        return empty[Random.Range(0, empty.Count)];
    }
}

