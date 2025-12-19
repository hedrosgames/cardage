using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Closed")]
public class RuleClosed : SOCapture
{
    public override void OnMatchStart()
    {
        GameEvents.OnHandVisibilityChanged?.Invoke(false);
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
}

