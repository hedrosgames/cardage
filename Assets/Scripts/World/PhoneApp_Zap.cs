using UnityEngine;
using System.Collections;
public class PhoneApp_Zap : MonoBehaviour
{
    [Header("Configuração - Primeira Vez")]
    public SOGameFlowFlag flagIntroDone;
    public SODialogueSequence introDialogue;
    [Header("UI")]
    public GameObject contactsListContainer;
    private SaveClientGameFlow _saveGameFlow;
    private ManagerDialogue _managerDialogue;
    private SaveClientGameFlow SaveGameFlow => _saveGameFlow ??= FindFirstObjectByType<SaveClientGameFlow>();
    private ManagerDialogue DialogueManager => _managerDialogue ??= FindFirstObjectByType<ManagerDialogue>();
    public void OnAppOpen()
    {
        StopAllCoroutines();
        StartCoroutine(CheckSequenceRoutine());
    }
    IEnumerator CheckSequenceRoutine()
    {
        yield return null;
        bool isFirstTime = false;
        if (SaveGameFlow != null && flagIntroDone != null && !SaveGameFlow.HasFlag(flagIntroDone))
        {
            isFirstTime = true;
        }
        if (isFirstTime)
        {
            if (contactsListContainer != null) contactsListContainer.SetActive(false);
            SaveGameFlow.SetFlag(flagIntroDone, 1);
            if (DialogueManager != null && introDialogue != null)
            {
                GameEvents.OnRequestDialogue?.Invoke(introDialogue);
                GameEvents.OnDialogueFinished += OnDialogueFinished;
            }
            else
            {
                FinishZapLogic();
            }
        }
        else
        {
            FinishZapLogic();
        }
    }
    void OnDialogueFinished(SODialogueSequence seq)
    {
        if (seq == introDialogue)
        {
            if (DialogueManager != null)
            GameEvents.OnDialogueFinished += OnDialogueFinished;
            FinishZapLogic();
        }
    }
    void FinishZapLogic()
    {
        if (contactsListContainer != null)
        contactsListContainer.SetActive(true);
        GameEvents.OnZapChecked?.Invoke();
    }
}

