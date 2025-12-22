using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Random")]
public class RuleRandom : SOCapture
{
    public override void OnMatchStart()
    {
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
}

