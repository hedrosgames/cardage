using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/Combo")]
public class RuleCombo : SOCapture
{
    public override void OnMatchStart()
    {
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> initialCaptured, int ownerId, ManagerBoard board)
    {
        if (initialCaptured == null || initialCaptured.Count == 0)
        return;
        bool comboTriggered = false;
        Queue<CardSlot> queue = new Queue<CardSlot>(initialCaptured);
        HashSet<CardSlot> visited = new HashSet<CardSlot>(initialCaptured);
        while (queue.Count > 0)
        {
            CardSlot origin = queue.Dequeue();
            SOCardData card = origin.currentCardView.cardData;
            List<CardSlot> newlyCaptured = new List<CardSlot>();
            TryCapture(origin, card.top, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y + 1), ownerId, newlyCaptured, s => s.currentCardView.cardData.bottom);
            TryCapture(origin, card.right, board.GetSlot(origin.gridPosition.x + 1, origin.gridPosition.y), ownerId, newlyCaptured, s => s.currentCardView.cardData.left);
            TryCapture(origin, card.bottom, board.GetSlot(origin.gridPosition.x, origin.gridPosition.y - 1), ownerId, newlyCaptured, s => s.currentCardView.cardData.top);
            TryCapture(origin, card.left, board.GetSlot(origin.gridPosition.x - 1, origin.gridPosition.y), ownerId, newlyCaptured, s => s.currentCardView.cardData.right);
            foreach (CardSlot s in newlyCaptured)
            {
                if (!visited.Contains(s))
                {
                    visited.Add(s);
                    queue.Enqueue(s);
                    comboTriggered = true;
                    GameEvents.OnComboStep?.Invoke(s, ownerId);
                }
            }
        }
        if (comboTriggered)
        {
            GameEvents.OnComboTriggered?.Invoke();
        }
    }
    private void TryCapture(CardSlot origin, int placedVal, CardSlot neigh, int ownerId,
    List<CardSlot> captured, System.Func<CardSlot, int> getNeighVal)
    {
        if (neigh == null || !neigh.IsOccupied) return;
        if (neigh.currentCardView.ownerId == ownerId) return;
        bool wasPlayer = neigh.currentCardView.isPlayerOwner;
        bool isAttackerPlayer = (ownerId == ManagerGame.ID_PLAYER);
        int neighVal = getNeighVal(neigh);
        if (placedVal > neighVal)
        {
            neigh.currentCardView.SetOwnerId(ownerId);
            if (ManagerGame.Instance != null)
            {
                if (isAttackerPlayer && !wasPlayer)
                UIScoreService.AddPointPlayer();
                else if (!isAttackerPlayer && wasPlayer)
                UIScoreService.AddPointOpponent();
            }
            if (!captured.Contains(neigh))
            captured.Add(neigh);
        }
    }
}

