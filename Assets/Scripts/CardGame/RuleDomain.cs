using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RuleDomain")]
public class RuleDomain : SOCapture
{
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
}

