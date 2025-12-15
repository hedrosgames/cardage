using UnityEngine;
using TMPro;
using System.Collections;
public class PhoneApp_Drive : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI statusText;
    [Header("Configuração - Primeira Vez")]
    public SOZoneFlag flagIntroDone;
    public SODialogueSequence introDialogue;
    private SaveClientZone _saveZone;
    private ManagerDialogue _managerDialogue;
    private SaveClientZone SaveZone => _saveZone ??= FindFirstObjectByType<SaveClientZone>();
    private ManagerDialogue DialogueManager => _managerDialogue ??= FindFirstObjectByType<ManagerDialogue>();
    public void OnAppOpen()
    {
        if (statusText != null) statusText.text = "Aguardando...";
        StopAllCoroutines();
        StartCoroutine(CheckSequenceRoutine());
    }
    IEnumerator CheckSequenceRoutine()
    {
        yield return null;
        bool isFirstTime = false;
        if (SaveZone != null && flagIntroDone != null)
        {
            if (!SaveZone.HasFlag(flagIntroDone)) isFirstTime = true;
        }
        if (isFirstTime)
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
                PerformSave();
            }
        }
        else
        {
            PerformSave();
        }
    }
    void OnDialogueFinished(SODialogueSequence seq)
    {
        if (seq == introDialogue)
        {
            if (DialogueManager != null)
            GameEvents.OnDialogueFinished += OnDialogueFinished;
            PerformSave();
        }
    }
    void PerformSave()
    {
        StartCoroutine(SaveProcessRoutine());
    }
    IEnumerator SaveProcessRoutine()
    {
        if (statusText != null) statusText.text = "Sincronizando...";
        yield return new WaitForSeconds(0.5f);
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.SaveAll();
        }
        if (statusText != null) statusText.text = "Backup Concluído.";
        GameEvents.OnDriveSaved?.Invoke();
    }
}

