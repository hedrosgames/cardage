using UnityEngine;
[CreateAssetMenu(menuName = "Config/RuleSet")]
public class SORuleSet : ScriptableObject
{
    public SOCapture[] activeRules;
    public SOVictoryRule victoryRule;
}

