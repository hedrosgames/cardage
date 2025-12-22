using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/SpecialBlock")]
public class RuleSpecialBlock : SOCapture
{
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
        List<CardSlot> toRemove = new List<CardSlot>();
        foreach (var slot in captured)
        {
            if (slot != null && slot.IsOccupied && slot.currentCardView != null)
            {
                SOCardData card = slot.currentCardView.cardData;
                if (card != null && card.rarity == CardRarity.Special)
                {
                    int previousOwner = slot.currentCardView.ownerId == ManagerGame.ID_PLAYER
                    ? ManagerGame.ID_OPPONENT
                    : ManagerGame.ID_PLAYER;
                    slot.currentCardView.SetOwnerId(previousOwner);
                    bool wasPlayer = slot.currentCardView.isPlayerOwner;
                    bool isAttackerPlayer = (ownerId == ManagerGame.ID_PLAYER);
                    if (isAttackerPlayer && !wasPlayer)
                    UIScoreService.AddPointOpponent();
                    else if (!isAttackerPlayer && wasPlayer)
                    UIScoreService.AddPointPlayer();
                    toRemove.Add(slot);
                }
            }
        }
        foreach (var slot in toRemove)
        {
            captured.Remove(slot);
        }
    }
}

