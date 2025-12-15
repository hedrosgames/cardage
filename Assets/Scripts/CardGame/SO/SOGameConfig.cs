using UnityEngine;
[CreateAssetMenu(menuName = "Config/Game")]
public class SOGameConfig : ScriptableObject
{
    public int cardsPerPlayer = 5;
    public bool enableAdvancedRules = false;
}

