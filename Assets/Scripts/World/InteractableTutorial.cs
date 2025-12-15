using UnityEngine;
using UnityEngine.Events;
public class InteractableTutorial : Interactable
{
    [Header("Tutorial")]
    public SOTutorial tutorial;
    [Header("Diálogo")]
    public SODialogueSequence dialogue;
    [Header("Evento Extra")]
    public UnityEvent onInteract;
    
    private ManagerTutorial tutorialManager;
    private bool isWaitingForTutorial = false;
    
    protected override void Awake()
    {
        base.Awake();
        tutorialManager = FindFirstObjectByType<ManagerTutorial>();
    }
    
    public override void OnInteract()
    {
        if (tutorialManager == null) tutorialManager = FindFirstObjectByType<ManagerTutorial>();
        
        // Verifica se o tutorial já foi completado (usando o sistema runOnce do tutorial)
        bool tutorialSeen = false;
        if (tutorialManager != null && tutorial != null)
        {
            tutorialSeen = tutorialManager.IsCompleted(tutorial.name);
        }
        
        // Se o tutorial não foi visto, mostra o tutorial primeiro
        if (!tutorialSeen && tutorial != null)
        {
            isWaitingForTutorial = true;
            GameEvents.OnRequestTutorialByAsset?.Invoke(tutorial);
            // Escuta quando o tutorial for completado
            if (tutorialManager != null)
            {
                tutorialManager.OnTutorialClosed += OnTutorialClosed;
            }
        }
        else
        {
            // Tutorial já foi visto, dispara o diálogo normalmente
            ShowDialogue();
        }
    }
    
    void OnTutorialClosed(SOTutorial completedTutorial)
    {
        if (completedTutorial == tutorial && isWaitingForTutorial)
        {
            isWaitingForTutorial = false;
            
            // Remove o listener
            if (tutorialManager != null)
            {
                tutorialManager.OnTutorialClosed -= OnTutorialClosed;
            }
            
            // Agora dispara o diálogo
            ShowDialogue();
        }
    }
    
    void ShowDialogue()
    {
        if (dialogue != null)
        {
            GameEvents.OnRequestDialogue?.Invoke(dialogue);
        }
        onInteract?.Invoke();
    }
    
    void OnDestroy()
    {
        if (tutorialManager != null)
        {
            tutorialManager.OnTutorialClosed -= OnTutorialClosed;
        }
    }
}

