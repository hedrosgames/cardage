using UnityEngine;
public class DialogueTrigger : Interactable
{
    public SODialogueSequence dialogueSequence;
    public bool isOneShot = true;
    private bool triggered = false;
    public override void OnInteract()
    {
        if (isOneShot && triggered)
        return;
        ManagerDialogue manager = Object.FindFirstObjectByType<ManagerDialogue>();
        if (manager != null && dialogueSequence != null)
        {
            manager.StartSequence(dialogueSequence);
            triggered = true;
        }
    }
}

