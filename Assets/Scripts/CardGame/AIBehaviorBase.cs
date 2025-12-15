using System.Collections.Generic;
using UnityEngine;
public abstract class AIBehaviorBase : ScriptableObject
{
    [Header("Documentação")]
    [Tooltip("Espaço livre para anotações sobre como essa IA se comporta.")]
    [TextArea(5, 10)]
    public string description;
    public abstract CardButton ChooseCard(List<CardButton> hand);
    public abstract CardSlot ChooseSlot(CardSlot[] board);
}

