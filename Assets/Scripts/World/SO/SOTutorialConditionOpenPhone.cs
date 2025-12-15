using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu(menuName = "Tutorial/OpenPhone")]
public class SOTutorialConditionOpenPhone : SOTutorialCondition
{
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        if (Keyboard.current == null) return false;
        bool opened = Keyboard.current.tabKey.wasPressedThisFrame ||
        Keyboard.current.pKey.wasPressedThisFrame ||
        Keyboard.current.mKey.wasPressedThisFrame;
        return opened;
    }
}

