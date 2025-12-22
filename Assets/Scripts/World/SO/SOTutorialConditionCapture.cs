using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/Capture")]
public class SOTutorialConditionCapture : SOTutorialCondition
{
    bool captureOccurred;
    public override void OnStart(ManagerTutorial manager)
    {
        captureOccurred = false;
        GameEvents.OnCardsCaptured += OnCardsCaptured;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return captureOccurred;
    }
    void OnCardsCaptured(System.Collections.Generic.List<CardSlot> captured, int ownerId)
    {
        if (ownerId == ManagerGame.ID_PLAYER && captured != null && captured.Count > 0)
        {
            captureOccurred = true;
            GameEvents.OnCardsCaptured -= OnCardsCaptured;
        }
    }
    void OnDestroy()
    {
        GameEvents.OnCardsCaptured -= OnCardsCaptured;
    }
}

