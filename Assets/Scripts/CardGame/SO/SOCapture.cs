using UnityEngine;
using System.Collections.Generic;
public abstract class SOCapture : ScriptableObject
{
    public virtual void OnMatchStart(){}
    public virtual void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board){}
    public abstract void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured);
}

