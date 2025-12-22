using UnityEngine;
using UnityEngine.Events;
public class InteractableSimple : Interactable, INotificationProvider
{
    public SODialogueSequence dialogue;
    [Header("Evento Extra")]
    public UnityEvent onInteract;
    [Header("Configuração")]
    [Tooltip("Se verdadeiro, o código executa apenas uma vez e depois desativa.")]
    public bool runOnce = false;
    private bool hasTriggered = false;
    private SpriteRenderer _cachedAlertRef;
    protected override void Awake()
    {
        base.Awake();
        if (interactionIcon != null && interactionIcon.name == "imgAlert")
        {
            interactionIcon = null;
        }
    }
    private void Start()
    {
        if (ManagerNotification.Instance != null)
        {
            ManagerNotification.Instance.RegisterInteractable(this);
        }
    }
    public override void OnInteract()
    {
        if (runOnce && hasTriggered) return;
        
        // Executa o evento e o save centralizado
        TriggerSave();

        if (dialogue != null)
        {
            GameEvents.OnRequestDialogue?.Invoke(dialogue);
        }

        if (runOnce)
        {
            hasTriggered = true;
            OnBlur();
        }
    }
    public override void OnFocus()
    {
        if (runOnce && hasTriggered) return;
        base.OnFocus();
    }
    public NotificationType GetNotificationType()
    {
        return NotificationType.None;
    }
    public SpriteRenderer GetImgAlert()
    {
        if (_cachedAlertRef != null) return _cachedAlertRef;
        Transform t = transform.Find("imgAlert");
        if (t == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "imgAlert")
                {
                    t = child;
                    break;
                }
            }
        }
        if (t != null)
        {
            _cachedAlertRef = t.GetComponent<SpriteRenderer>();
            return _cachedAlertRef;
        }
        return null;
    }
}

