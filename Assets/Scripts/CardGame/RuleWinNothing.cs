using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Victory/WinNothing")]
public class RuleWinNothing : SOVictoryRule
{
    public override MatchResult CalculateVictory(ManagerBoard board, int playerScore, int opponentScore)
    {
        return MatchResult.Draw;
    }
}

