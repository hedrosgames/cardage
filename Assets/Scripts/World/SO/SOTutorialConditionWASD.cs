using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu(menuName = "Tutorial/WASD")]
public class SOTutorialConditionWASD : SOTutorialCondition
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
        bool moved =
        Keyboard.current.wKey.wasPressedThisFrame ||
        Keyboard.current.aKey.wasPressedThisFrame ||
        Keyboard.current.sKey.wasPressedThisFrame ||
        Keyboard.current.dKey.wasPressedThisFrame;
        if (!moved) return false;
        alreadyTriggered = true;
        return true;
    }
}

