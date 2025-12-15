using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/Compra")]
public class SOTutorialConditionCompra : SOTutorialCondition
{
    bool purchased;
    public override void OnStart(ManagerTutorial manager)
    {
        purchased = false;
    }
    public override bool CheckCompleted(ManagerTutorial manager)
    {
        return purchased;
    }
    public void RegisterPurchase()
    {
        if (purchased) return;
        purchased = true;
    }
}

