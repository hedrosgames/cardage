using UnityEngine;
using TMPro;
using System.Collections;
public class PhoneApp_Bank : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI txtBalance;
    public string initialBalance = "R$ 4,20";
    [Header("Configuração - Primeira Vez")]
    public SOGameFlowFlag flagIntroDone;
    public SODialogueSequence introDialogue;
    private SaveClientGameFlow _saveGameFlow;
    private ManagerDialogue _managerDialogue;
    private SaveClientGameFlow SaveGameFlow => _saveGameFlow ??= FindFirstObjectByType<SaveClientGameFlow>();
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
        if (flagIntroDone == null || SaveGameFlow == null) yield break;
        if (!SaveGameFlow.HasFlag(flagIntroDone))
        {
            SaveGameFlow.SetFlag(flagIntroDone, 1);
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

