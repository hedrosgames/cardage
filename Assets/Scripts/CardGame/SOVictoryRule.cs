using UnityEngine;
using System.Collections.Generic;
public abstract class SOVictoryRule : ScriptableObject
{
    public virtual void OnMatchStart() { }
    public abstract MatchResult CalculateVictory(ManagerBoard board, int playerScore, int opponentScore);
}

