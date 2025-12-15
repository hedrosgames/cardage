using UnityEngine;
[CreateAssetMenu(menuName = "Game/Deck")]
public class SODeckData : ScriptableObject
{
    public SOCardData[] cards = new SOCardData[5];
}

