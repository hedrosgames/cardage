using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Open")]
public class RuleOpen : SOCapture
{
    public override void OnMatchStart()
    {
        GameEvents.OnHandVisibilityChanged?.Invoke(true);
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
}

