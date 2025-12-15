using UnityEngine;
using UnityEngine.Events;
public class InteractableSimple : Interactable
{
    public SODialogueSequence dialogue;
    [Header("Evento Extra")]
    public UnityEvent onInteract;
    public override void OnInteract()
    {
        if (dialogue != null)
        {
            GameEvents.OnRequestDialogue?.Invoke(dialogue);
        }
        onInteract?.Invoke();
    }
}

