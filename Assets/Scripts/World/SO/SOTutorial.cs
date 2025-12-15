using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/TutorialConfig")]
public class SOTutorial : ScriptableObject
{
    [TextArea] public string message;
    public Sprite icon;
    public SOTutorialCondition condition;
    public bool runOnce = true;
    [Header("Restrições")]
    [Tooltip("Se marcado, impede o jogador de andar enquanto o tutorial estiver na tela.")]
    public bool blockMovement;
    [Tooltip("Se marcado, impede o jogador de interagir enquanto o tutorial estiver na tela.")]
    public bool blockInteraction;
}

