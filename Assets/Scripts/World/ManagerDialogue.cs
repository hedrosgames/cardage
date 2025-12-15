using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class ManagerDialogue : MonoBehaviour
{
    public SODialogueSequence currentSequence;
    public int currentIndex { get; private set; }
    bool isRunning;
    private PlayerControl playerControl;
    public event Action<SODialogueSequence> OnSequenceStarted;
    public event Action<SODialogueSequence.DialogueLine> OnLineShown;
    void Awake()
    {
        playerControl = FindFirstObjectByType<PlayerControl>();
    }
    void OnEnable()
    {
        GameEvents.OnRequestDialogue += StartSequence;
    }
    void OnDisable()
    {
        GameEvents.OnRequestDialogue -= StartSequence;
    }
    public void StartSequence(SODialogueSequence sequence)
    {
        if (sequence == null) return;
        if (isRunning)
        {
            return;
        }
        currentSequence = sequence;
        currentIndex = 0;
        isRunning = true;
        
        // Trava o movimento do player quando inicia um diálogo
        if (playerControl == null) playerControl = FindFirstObjectByType<PlayerControl>();
        if (playerControl != null)
        {
            playerControl.SetMovement(false);
        }
        
        ShowCurrentLine();
        OnSequenceStarted?.Invoke(currentSequence);
    }
    void ShowCurrentLine()
    {
        if (currentSequence == null) return;
        if (currentIndex < 0 || currentIndex >= currentSequence.lines.Length)
        {
            EndSequence();
            return;
        }
        var line = currentSequence.lines[currentIndex];
        if (line != null) OnLineShown?.Invoke(line);
    }
    public void NextLine()
    {
        currentIndex++;
        if (currentSequence == null || currentIndex >= currentSequence.lines.Length)
        {
            EndSequence();
            return;
        }
        ShowCurrentLine();
    }
    void EndSequence()
    {
        if (!isRunning) return;
        isRunning = false;
        var finishedSequence = currentSequence;
        currentSequence = null;
        
        // Reabilita o movimento do player quando termina o diálogo
        // (só se o telefone não estiver aberto)
        if (playerControl != null)
        {
            if (ManagerPhone.Instance == null || !ManagerPhone.Instance.IsOpen)
            {
                playerControl.SetMovement(true);
            }
        }
        
        GameEvents.OnDialogueFinished?.Invoke(finishedSequence);
    }
}

