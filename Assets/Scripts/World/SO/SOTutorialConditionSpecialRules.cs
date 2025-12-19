using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/SpecialRules")]
public class SOTutorialConditionSpecialRules : SOTutorialCondition
{
    bool sameTriggered;
    bool plusTriggered;
    bool comboTriggered;
    public override void OnStart(ManagerTutorial manager)
    {
        sameTriggered = false;
        plusTriggered = false;
        comboTriggered = false;
        GameEvents.OnSameTriggered += OnSameTriggered;
        GameEvents.OnPlusTriggered += OnPlusTriggered;
        GameEvents.OnComboTriggered += OnComboTriggered;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return sameTriggered || plusTriggered || comboTriggered;
    }
    void OnSameTriggered(System.Collections.Generic.List<CardSlot> slots)
    {
        sameTriggered = true;
    }
    void OnPlusTriggered(System.Collections.Generic.List<CardSlot> slots)
    {
        plusTriggered = true;
    }
    void OnComboTriggered()
    {
        comboTriggered = true;
    }
    void OnDestroy()
    {
        GameEvents.OnSameTriggered -= OnSameTriggered;
        GameEvents.OnPlusTriggered -= OnPlusTriggered;
        GameEvents.OnComboTriggered -= OnComboTriggered;
    }
}

