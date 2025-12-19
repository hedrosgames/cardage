using UnityEngine;
using UnityEngine.Events;
public class InteractableTutorial : Interactable, INotificationProvider
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
        bool tutorialSeen = false;
        if (tutorialManager != null && tutorial != null)
        {
            tutorialSeen = tutorialManager.IsCompleted(tutorial.name);
        }
        if (!tutorialSeen && tutorial != null)
        {
            isWaitingForTutorial = true;
            GameEvents.OnRequestTutorialByAsset?.Invoke(tutorial);
            if (tutorialManager != null)
            {
                tutorialManager.OnTutorialClosed += OnTutorialClosed;
            }
        }
        else
        {
            ShowDialogue();
        }
    }
    void OnTutorialClosed(SOTutorial completedTutorial)
    {
        if (completedTutorial == tutorial && isWaitingForTutorial)
        {
            isWaitingForTutorial = false;
            if (tutorialManager != null)
            {
                tutorialManager.OnTutorialClosed -= OnTutorialClosed;
            }
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
    public NotificationType GetNotificationType()
    {
        if (tutorial == null) return NotificationType.None;
        if (tutorialManager == null)
        {
            tutorialManager = FindFirstObjectByType<ManagerTutorial>();
        }
        if (tutorialManager != null)
        {
            bool isCompleted = tutorialManager.IsCompleted(tutorial.name);
            if (isCompleted)
            {
                return NotificationType.None;
            }
            return NotificationType.Tutorial;
        }
        return NotificationType.Tutorial;
    }
    public SpriteRenderer GetImgAlert()
    {
        Transform alertTransform = transform.Find("imgAlert");
        if (alertTransform != null)
        {
            return alertTransform.GetComponent<SpriteRenderer>();
        }
        return null;
    }
}

