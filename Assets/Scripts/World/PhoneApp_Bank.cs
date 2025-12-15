using UnityEngine;
using TMPro;
using System.Collections;
public class PhoneApp_Bank : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI txtBalance;
    public string initialBalance = "R$ 4,20";
    [Header("Configuração - Primeira Vez")]
    public SOZoneFlag flagIntroDone;
    public SODialogueSequence introDialogue;
    private SaveClientZone _saveZone;
    private ManagerDialogue _managerDialogue;
    private SaveClientZone SaveZone => _saveZone ??= FindFirstObjectByType<SaveClientZone>();
    private ManagerDialogue DialogueManager => _managerDialogue ??= FindFirstObjectByType<ManagerDialogue>();
    public void OnAppOpen()
    {
        if (txtBalance != null) txtBalance.text = initialBalance;
        StopAllCoroutines();
        StartCoroutine(CheckSequenceRoutine());
    }
    IEnumerator CheckSequenceRoutine()
    {
        yield return null;
        if (flagIntroDone == null || SaveZone == null) yield break;
        if (!SaveZone.HasFlag(flagIntroDone))
        {
            SaveZone.SetFlag(flagIntroDone, 1);
            SaveEvents.RaiseSave();
            if (DialogueManager != null && introDialogue != null)
            {
                GameEvents.OnRequestDialogue?.Invoke(introDialogue);
                GameEvents.OnDialogueFinished += OnDialogueFinished;
            }
            else
            {
                NotifyTaskComplete();
            }
        }
    }
    void OnDialogueFinished(SODialogueSequence seq)
    {
        if (seq == introDialogue)
        {
            if (DialogueManager != null)
            GameEvents.OnDialogueFinished += OnDialogueFinished;
            NotifyTaskComplete();
        }
    }
    void NotifyTaskComplete()
    {
        GameEvents.OnBankChecked?.Invoke();
    }
}

