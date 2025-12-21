using UnityEngine;
[CreateAssetMenu(menuName = "Tutorial/OpenPhone")]
public class SOTutorialConditionOpenPhone : SOTutorialCondition
{
    bool bankChecked;
    public override void OnStart(ManagerTutorial manager)
    {
        bankChecked = false;
        // Remove o evento antes de adicionar novamente (evita múltiplas inscrições)
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
        // Remove o evento assim que a condição é atendida
        GameEvents.OnBankChecked -= OnBankChecked;
    }
    void OnDestroy()
    {
        // Fallback para garantir limpeza
        GameEvents.OnBankChecked -= OnBankChecked;
    }
}

