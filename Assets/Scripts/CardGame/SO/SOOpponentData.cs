using UnityEngine;
[CreateAssetMenu(menuName = "Game/Opponent")]
public class SOOpponentData : ScriptableObject
{
    public string opponentName;
    public string opponentNameKey;
    public SODeckData deck;
    public AIBehaviorBase behavior;
}

