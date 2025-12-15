using UnityEngine;
using System.Collections;
public class PhoneApp_Zap : MonoBehaviour
{
    [Header("Configuração - Primeira Vez")]
    public SOZoneFlag flagIntroDone;
    public SODialogueSequence introDialogue;
    [Header("UI")]
    public GameObject contactsListContainer;
    private SaveClientZone _saveZone;
    private ManagerDialogue _managerDialogue;
    private SaveClientZone SaveZone => _saveZone ??= FindFirstObjectByType<SaveClientZone>();
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
        if (SaveZone != null && flagIntroDone != null && !SaveZone.HasFlag(flagIntroDone))
        {
            isFirstTime = true;
        }
        if (isFirstTime)
        {
            if (contactsListContainer != null) contactsListContainer.SetActive(false);
            SaveZone.SetFlag(flagIntroDone, 1);
            SaveEvents.RaiseSave();
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

