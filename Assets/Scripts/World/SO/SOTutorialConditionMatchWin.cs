using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/MatchWin")]
public class SOTutorialConditionMatchWin : SOTutorialCondition
{
    bool matchFinished;
    public override void OnStart(ManagerTutorial manager)
    {
        matchFinished = false;
        GameEvents.OnMatchFinished += OnMatchFinished;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return matchFinished;
    }
    void OnMatchFinished(MatchResult result, int playerScore, int opponentScore)
    {
        matchFinished = true;
        GameEvents.OnMatchFinished -= OnMatchFinished;
    }
    void OnDestroy()
    {
        GameEvents.OnMatchFinished -= OnMatchFinished;
    }
}
