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
        // Resolve conflito se a base pegou o ícone errado
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
        // Se for execução única e já tiver rodado, não faz nada
        if (runOnce && hasTriggered) return;

        if (dialogue != null)
        {
            GameEvents.OnRequestDialogue?.Invoke(dialogue);
        }
        onInteract?.Invoke();

        // Marca como executado e esconde o ícone imediatamente
        if (runOnce)
        {
            hasTriggered = true;
            OnBlur(); // Força saída visual
        }
    }

    // Sobrescreve OnFocus para impedir que o ícone apareça novamente se já foi consumido
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