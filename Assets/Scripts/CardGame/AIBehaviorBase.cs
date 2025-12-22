using System.Collections.Generic;
using UnityEngine;
public abstract class AIBehaviorBase : ScriptableObject
{
    [Header("DocumentaÃ§Ã£o")]
    [TextArea(5, 10)]
    public string description;
    public abstract CardButton ChooseCard(List<CardButton> hand);
    public abstract CardSlot ChooseSlot(CardSlot[] board);
}

