using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/Triad")]
public class RuleTriad : SOCapture
{
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
        TryCaptureTriad(origin, card.top, card.triad, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y + 1), ownerId, captured, s => s.currentCardView.cardData.bottom, s => s.currentCardView.cardData.triad);
        TryCaptureTriad(origin, card.right, card.triad, board.GetSlot(origin.gridPosition.x + 1, origin.gridPosition.y), ownerId, captured, s => s.currentCardView.cardData.left, s => s.currentCardView.cardData.triad);
        TryCaptureTriad(origin, card.bottom, card.triad, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y - 1), ownerId, captured, s => s.currentCardView.cardData.top, s => s.currentCardView.cardData.triad);
        TryCaptureTriad(origin, card.left, card.triad, board.GetSlot(origin.gridPosition.x - 1, origin.gridPosition.y), ownerId, captured, s => s.currentCardView.cardData.right, s => s.currentCardView.cardData.triad);
    }
    private void TryCaptureTriad(CardSlot origin, int placedVal, TriadType placedTriad, CardSlot neigh, int ownerId,
    List<CardSlot> captured, System.Func<CardSlot, int> getNeighVal, System.Func<CardSlot, TriadType> getNeighTriad)
    {
        if (neigh == null || !neigh.IsOccupied) return;
        if (neigh.currentCardView.ownerId == ownerId) return;
        bool wasPlayer = neigh.currentCardView.isPlayerOwner;
        bool isAttackerPlayer = (ownerId == ManagerGame.ID_PLAYER);
        int neighVal = getNeighVal(neigh);
        TriadType neighTriad = getNeighTriad(neigh);
        bool shouldCapture = false;
        if (DoesTriadWin(placedTriad, neighTriad))
        {
            shouldCapture = true;
        }
        else if (DoesTriadLose(placedTriad, neighTriad))
        {
            shouldCapture = false;
        }
        else
        {
            shouldCapture = placedVal > neighVal;
        }
        if (shouldCapture)
        {
            neigh.currentCardView.SetOwnerId(ownerId);
            if (isAttackerPlayer && !wasPlayer)
            UIScoreService.AddPointPlayer();
            else if (!isAttackerPlayer && wasPlayer)
            UIScoreService.AddPointOpponent();
            if (!captured.Contains(neigh))
            captured.Add(neigh);
        }
    }
    private bool DoesTriadWin(TriadType attacker, TriadType defender)
    {
        if (attacker == TriadType.Power && defender == TriadType.Agility)
        return true;
        if (attacker == TriadType.Agility && defender == TriadType.Magic)
        return true;
        if (attacker == TriadType.Magic && defender == TriadType.Power)
        return true;
        return false;
    }
    private bool DoesTriadLose(TriadType attacker, TriadType defender)
    {
        if (attacker == TriadType.Power && defender == TriadType.Magic)
        return true;
        if (attacker == TriadType.Agility && defender == TriadType.Power)
        return true;
        if (attacker == TriadType.Magic && defender == TriadType.Agility)
        return true;
        return false;
    }
}

