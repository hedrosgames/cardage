using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/Betting")]
public class SOTutorialConditionBetting : SOTutorialCondition
{
    bool matchWithBettingFinished;
    public override void OnStart(ManagerTutorial manager)
    {
        matchWithBettingFinished = false;
        GameEvents.OnMatchFinished += OnMatchFinished;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return matchWithBettingFinished;
    }
    void OnMatchFinished(MatchResult result, int playerScore, int opponentScore)
    {
        matchWithBettingFinished = true;
        GameEvents.OnMatchFinished -= OnMatchFinished;
    }
    void OnDestroy()
    {
        GameEvents.OnMatchFinished -= OnMatchFinished;
    }
}

