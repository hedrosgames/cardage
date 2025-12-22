using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/DeckBuilder")]
public class SOTutorialConditionDeckBuilder : SOTutorialCondition
{
    bool deckCompleted;
    private const int REQUIRED_CARDS = 5;
    public override void OnStart(ManagerTutorial manager)
    {
        deckCompleted = false;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return deckCompleted;
    }
    public void RegisterDeckCompleted(int cardCount)
    {
        if (cardCount >= REQUIRED_CARDS)
        {
            deckCompleted = true;
        }
    }
}

