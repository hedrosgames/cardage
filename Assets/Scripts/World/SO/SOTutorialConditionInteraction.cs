using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu(menuName = "Tutorial/Interaction")]
public class SOTutorialConditionInteraction : SOTutorialCondition
{
    bool alreadyTriggered;
    public override void OnStart(ManagerTutorial manager)
    {
        alreadyTriggered = false;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        if (alreadyTriggered) return true;
        if (Keyboard.current == null) return false;
        bool interacted = Keyboard.current.eKey.wasPressedThisFrame;
        if (!interacted) return false;
        alreadyTriggered = true;
        return true;
    }
}

