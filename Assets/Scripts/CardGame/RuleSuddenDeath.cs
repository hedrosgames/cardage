using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Victory/SuddenDeath")]
public class RuleSuddenDeath : SOVictoryRule
{
    private List<CardSlot> capturedCards = new List<CardSlot>();
    public override void OnMatchStart()
    {
        capturedCards.Clear();
        GameEvents.OnCardsCaptured += TrackCapturedCards;
    }
    private void TrackCapturedCards(List<CardSlot> captured, int ownerId)
    {
        foreach (var slot in captured)
        {
            if (!capturedCards.Contains(slot))
            capturedCards.Add(slot);
        }
    }
    public override MatchResult CalculateVictory(ManagerBoard board, int playerScore, int opponentScore)
    {
        if (playerScore == opponentScore)
        {
            GameEvents.OnMatchReset?.Invoke();
            return MatchResult.Draw;
        }
        if (playerScore > opponentScore) return MatchResult.PlayerWin;
        return MatchResult.OpponentWin;
    }
}

