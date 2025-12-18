using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/CardBasics")]
public class SOTutorialConditionCardBasics : SOTutorialCondition
{
    bool cardPlaced;
    public override void OnStart(ManagerTutorial manager)
    {
        cardPlaced = false;
        GameEvents.OnCardPlayed += OnCardPlayed;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return cardPlaced;
    }
    void OnCardPlayed(CardSlot slot, SOCardData card, int ownerId)
    {
        if (ownerId == ManagerGame.ID_PLAYER)
        {
            cardPlaced = true;
            GameEvents.OnCardPlayed -= OnCardPlayed;
        }
    }
    void OnDestroy()
    {
        GameEvents.OnCardPlayed -= OnCardPlayed;
    }
}
