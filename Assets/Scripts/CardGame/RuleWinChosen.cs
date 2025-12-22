using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Victory/WinChosen")]
public class RuleWinChosen : SOVictoryRule
{
    public override MatchResult CalculateVictory(ManagerBoard board, int playerScore, int opponentScore)
    {
        if (playerScore > opponentScore) return MatchResult.PlayerWin;
        if (opponentScore > playerScore) return MatchResult.OpponentWin;
        return MatchResult.Draw;
    }
}

