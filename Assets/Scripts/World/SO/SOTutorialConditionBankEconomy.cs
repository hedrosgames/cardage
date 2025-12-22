using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/BankEconomy")]
public class SOTutorialConditionBankEconomy : SOTutorialCondition
{
    bool bankOpened;
    bool purchaseMade;
    public override void OnStart(ManagerTutorial manager)
    {
        bankOpened = false;
        purchaseMade = false;
        GameEvents.OnBankChecked += OnBankChecked;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return bankOpened && purchaseMade;
    }
    void OnBankChecked()
    {
        bankOpened = true;
    }
    public void RegisterPurchase()
    {
        purchaseMade = true;
    }
    void OnDestroy()
    {
        GameEvents.OnBankChecked -= OnBankChecked;
    }
}

