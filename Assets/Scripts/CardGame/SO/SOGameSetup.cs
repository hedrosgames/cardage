using UnityEngine;
[CreateAssetMenu(menuName = "Config/GameSetup")]
public class SOGameSetup : ScriptableObject
{
    public SODeckData playerDeck;
    public SOOpponentData opponent;
}

