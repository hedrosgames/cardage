using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class ManagerNotification : MonoBehaviour
{
    public static ManagerNotification Instance { get; private set; }
    [Header("Configuração")]
    public SONotificationIcons notificationIcons;
    [Header("Flags de Itens (para notificações condicionais)")]
    [Tooltip("Lista de flags que indicam itens ativos. Interactables verificam essas flags.")]
    public List<SOZoneFlag> itemFlags = new List<SOZoneFlag>();
    private HashSet<Interactable> registeredInteractables = new HashSet<Interactable>();
    private Interactable focusedInteractable;
    private SaveClientZone saveZone;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void OnEnable()
    {
        GameEvents.OnInteractableFocused += OnInteractableFocused;
        GameEvents.OnInteractableBlurred += OnInteractableBlurred;
    }
    void OnDisable()
    {
        GameEvents.OnInteractableFocused -= OnInteractableFocused;
        GameEvents.OnInteractableBlurred -= OnInteractableBlurred;
    }
    void Start()
    {
        saveZone = FindFirstObjectByType<SaveClientZone>();
        if (saveZone != null)
        {
            saveZone.OnFlagChanged += OnFlagChanged;
        }
        StartCoroutine(DelayedInitialSetup());
    }
    void OnDestroy()
    {
        if (saveZone != null)
        {
            saveZone.OnFlagChanged -= OnFlagChanged;
        }
    }
    void OnFlagChanged(SOZoneFlag flag)
    {
        if (flag != null && IsItemFlag(flag))
        {
            RefreshAllNotifications();
        }
    }
    bool IsItemFlag(SOZoneFlag flag)
    {
        if (flag == null || itemFlags == null) return false;
        foreach (var itemFlag in itemFlags)
        {
            if (itemFlag != null && itemFlag.id == flag.id)
            {
                return true;
            }
        }
        return false;
    }
    IEnumerator DelayedInitialSetup()
    {
        yield return null;
        yield return null;
        yield return null;
        RegisterAllInteractables();
        RefreshAllNotifications();
    }
    void RegisterAllInteractables()
    {
        Interactable[] allInteractables = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
        foreach (var interactable in allInteractables)
        {
            if (interactable != null)
            {
                RegisterInteractable(interactable);
            }
        }
    }
    public void RegisterInteractable(Interactable interactable)
    {
        if (interactable == null) return;
        if (registeredInteractables.Contains(interactable))
        {
            return;
        }
        registeredInteractables.Add(interactable);
        SpriteRenderer imgAlert = FindImgAlert(interactable);
        if (imgAlert != null)
        {
            imgAlert.gameObject.SetActive(false);
        }
    }
    public void UnregisterInteractable(Interactable interactable)
    {
        if (interactable != null)
        {
            registeredInteractables.Remove(interactable);
        }
    }
    void OnInteractableFocused(Interactable interactable)
    {
        if (focusedInteractable != null && focusedInteractable != interactable)
        {
            UpdateNotificationForInteractable(focusedInteractable);
        }
        focusedInteractable = interactable;
        UpdateNotificationForInteractable(interactable);
    }
    void OnInteractableBlurred(Interactable interactable)
    {
        if (focusedInteractable == interactable)
        {
            focusedInteractable = null;
            UpdateNotificationForInteractable(interactable);
        }
    }
    void RefreshAllNotifications()
    {
        foreach (var interactable in registeredInteractables.ToList())
        {
            if (interactable != null && interactable.gameObject != null)
            {
                UpdateNotificationForInteractable(interactable);
            }
        }
    }
    void UpdateNotificationForInteractable(Interactable interactable)
    {
        if (interactable == null) return;
        NotificationType type = DetermineNotificationType(interactable);
        SpriteRenderer imgAlert = FindImgAlert(interactable);
        if (imgAlert == null)
        {
            return;
        }
        UpdateImgAlert(imgAlert, type);
    }
    void UpdateImgAlert(SpriteRenderer imgAlert, NotificationType type)
    {
        if (imgAlert == null || notificationIcons == null)
        {
            if (imgAlert != null)
            {
                imgAlert.gameObject.SetActive(false);
            }
            return;
        }
        Sprite targetSprite = GetSpriteForNotification(type);
        if (targetSprite == null)
        {
            imgAlert.gameObject.SetActive(false);
        }
        else
        {
            imgAlert.sprite = targetSprite;
            imgAlert.gameObject.SetActive(true);
        }
    }
    Sprite GetSpriteForNotification(NotificationType type)
    {
        if (notificationIcons == null) return null;
        switch (type)
        {
            case NotificationType.Alert:
            return notificationIcons.iconAlert;
            case NotificationType.Tournament:
            return notificationIcons.iconTournament;
            case NotificationType.RareCard:
            return notificationIcons.iconRareCard;
            case NotificationType.LostCard:
            return notificationIcons.iconLostCard;
            case NotificationType.Duel:
            return notificationIcons.iconDuel;
            case NotificationType.Tutorial:
            return notificationIcons.iconTutorial;
            default:
            return null;
        }
    }
    SpriteRenderer FindImgAlert(Interactable interactable)
    {
        if (interactable == null) return null;
        INotificationProvider provider = interactable as INotificationProvider;
        if (provider != null)
        {
            return provider.GetImgAlert();
        }
        Transform alertTransform = interactable.transform.Find("imgAlert");
        if (alertTransform != null)
        {
            return alertTransform.GetComponent<SpriteRenderer>();
        }
        SpriteRenderer[] renderers = interactable.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer.gameObject.name == "imgAlert")
            {
                return renderer;
            }
        }
        return null;
    }
    NotificationType DetermineNotificationType(Interactable interactable)
    {
        if (interactable is InteractableTutorial)
        {
            InteractableTutorial tutorialInteractable = interactable as InteractableTutorial;
            if (tutorialInteractable.tutorial != null)
            {
                ManagerTutorial tutorialManager = FindFirstObjectByType<ManagerTutorial>();
                if (tutorialManager != null)
                {
                    bool isCompleted = tutorialManager.IsCompleted(tutorialInteractable.tutorial.name);
                    if (!isCompleted)
                    {
                        return NotificationType.Tutorial;
                    }
                }
                else
                {
                    return NotificationType.Tutorial;
                }
            }
        }
        INotificationProvider provider = interactable as INotificationProvider;
        if (provider != null)
        {
            NotificationType providerType = provider.GetNotificationType();
            if (providerType != NotificationType.None)
            {
                return providerType;
            }
        }
        bool hasActiveItem = HasActiveItem();
        if (hasActiveItem)
        {
            if (HasLostCard(interactable))
            {
                return NotificationType.LostCard;
            }
            if (HasRareCard(interactable))
            {
                return NotificationType.RareCard;
            }
            if (CanDuel(interactable))
            {
                return NotificationType.Duel;
            }
        }
        if (IsInTournament(interactable))
        {
            return NotificationType.Tournament;
        }
        if (focusedInteractable == interactable)
        {
            return NotificationType.Alert;
        }
        return NotificationType.None;
    }
    bool HasLostCard(Interactable interactable)
    {
        return false;
    }
    bool HasRareCard(Interactable interactable)
    {
        InteractableCardGame cardGameInteractable = interactable.GetComponent<InteractableCardGame>();
        if (cardGameInteractable == null)
        {
            cardGameInteractable = interactable.GetComponentInChildren<InteractableCardGame>();
        }
        if (cardGameInteractable != null && cardGameInteractable.gameSetup != null)
        {
            if (cardGameInteractable.gameSetup.opponent != null &&
            cardGameInteractable.gameSetup.opponent.deck != null)
            {
                foreach (var card in cardGameInteractable.gameSetup.opponent.deck.cards)
                {
                    if (card != null && card.rarity == CardRarity.Rare)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    bool CanDuel(Interactable interactable)
    {
        InteractableCardGame cardGameInteractable = interactable.GetComponent<InteractableCardGame>();
        if (cardGameInteractable == null)
        {
            cardGameInteractable = interactable.GetComponentInChildren<InteractableCardGame>();
        }
        return cardGameInteractable != null;
    }
    bool IsInTournament(Interactable interactable)
    {
        return false;
    }
    public bool HasActiveItem()
    {
        if (itemFlags == null || itemFlags.Count == 0) return false;
        if (saveZone == null)
        {
            saveZone = FindFirstObjectByType<SaveClientZone>();
        }
        if (saveZone != null)
        {
            foreach (var flag in itemFlags)
            {
                if (flag != null && saveZone.HasFlag(flag))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasItemFlag(SOZoneFlag flag)
    {
        if (flag == null) return false;
        if (saveZone == null)
        {
            saveZone = FindFirstObjectByType<SaveClientZone>();
        }
        if (saveZone != null)
        {
            return saveZone.HasFlag(flag);
        }
        return false;
    }
    public void RefreshNotifications()
    {
        RefreshAllNotifications();
    }
    [ContextMenu("Debug: Listar Todas as Notificações")]
    public void DebugListAllNotifications()
    {
        int count = 0;
        foreach (var interactable in registeredInteractables)
        {
            if (interactable == null || interactable.gameObject == null) continue;
            count++;
            NotificationType currentType = DetermineNotificationType(interactable);
            bool isFocused = focusedInteractable == interactable;
            SpriteRenderer imgAlert = FindImgAlert(interactable);
            if (interactable is InteractableTutorial)
            {
                InteractableTutorial tut = interactable as InteractableTutorial;
            }
        }
    }
    [ContextMenu("Debug: Forçar Atualização")]
    public void DebugForceRefresh()
    {
        RefreshAllNotifications();
    }
}

