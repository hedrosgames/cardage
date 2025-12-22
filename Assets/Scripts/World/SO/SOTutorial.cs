using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/TutorialConfig")]
public class SOTutorial : ScriptableObject
{
    [TextArea] public string message;
    public Sprite icon;
    public SOTutorialCondition condition;
    public bool runOnce = true;
    [Header("RestriÃ§Ãµes")]
    public bool blockMovement;
    public bool blockInteraction;
}

