using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Victory/WinDiff")]
public class RuleWinDiff : SOVictoryRule
{
    public override MatchResult CalculateVictory(ManagerBoard board, int playerScore, int opponentScore)
    {
        int playerCards = 0;
        int opponentCards = 0;
        if (board != null)
        {
            CardSlot[] slots = board.GetAllSlots();
            foreach (var slot in slots)
            {
                if (slot != null && slot.IsOccupied)
                {
                    SOCardData card = slot.currentCardView.cardData;
                    if (card != null && card.rarity == CardRarity.Special)
                    {
                        continue;
                    }
                    if (slot.currentCardView.ownerId == ManagerGame.ID_PLAYER)
                    playerCards++;
                    else if (slot.currentCardView.ownerId == ManagerGame.ID_OPPONENT)
                    opponentCards++;
                }
            }
        }
        if (playerCards > opponentCards) return MatchResult.PlayerWin;
        if (opponentCards > playerCards) return MatchResult.OpponentWin;
        return MatchResult.Draw;
    }
}

