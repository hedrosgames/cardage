using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Victory/WinAll")]
public class RuleWinAll : SOVictoryRule
{
    public override MatchResult CalculateVictory(ManagerBoard board, int playerScore, int opponentScore)
    {
        if (board != null)
        {
            CardSlot[] slots = board.GetAllSlots();
            bool playerHasAll = true;
            bool opponentHasAll = true;
            foreach (var slot in slots)
            {
                if (slot != null && slot.IsOccupied)
                {
                    SOCardData card = slot.currentCardView.cardData;
                    if (card != null && card.rarity == CardRarity.Special)
                    continue;
                    if (slot.currentCardView.ownerId != ManagerGame.ID_PLAYER)
                    playerHasAll = false;
                    if (slot.currentCardView.ownerId != ManagerGame.ID_OPPONENT)
                    opponentHasAll = false;
                }
            }
            if (playerHasAll) return MatchResult.PlayerWin;
            if (opponentHasAll) return MatchResult.OpponentWin;
        }
        if (playerScore > opponentScore) return MatchResult.PlayerWin;
        if (opponentScore > playerScore) return MatchResult.OpponentWin;
        return MatchResult.Draw;
    }
}

