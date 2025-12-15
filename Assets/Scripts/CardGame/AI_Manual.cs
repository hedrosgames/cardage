using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Game/AI/Manual (Human)")]
public class AI_Manual : AIBehaviorBase
{
    public override CardButton ChooseCard(List<CardButton> hand)
    {
        return null;
    }
    public override CardSlot ChooseSlot(CardSlot[] board)
    {
        return null;
    }
}

