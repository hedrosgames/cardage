using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/OpenPhone")]
public class SOTutorialConditionOpenPhone : SOTutorialCondition
{
    bool bankChecked;
    public override void OnStart(ManagerTutorial manager)
    {
        bankChecked = false;
        GameEvents.OnBankChecked -= OnBankChecked;
        GameEvents.OnBankChecked += OnBankChecked;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return bankChecked;
    }
    void OnBankChecked()
    {
        bankChecked = true;
        GameEvents.OnBankChecked -= OnBankChecked;
    }
    void OnDestroy()
    {
        GameEvents.OnBankChecked -= OnBankChecked;
    }
}

