using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class ManagerNotification : MonoBehaviour
{
    public static ManagerNotification Instance { get; private set; }
    [Header("ConfiguraÃ§Ã£o")]
    public SONotificationIcons notificationIcons;
    [Header("Flags de Itens")]
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
        focusedInteractable = null;
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
        return itemFlags.Any(f => f != null && f.id == flag.id);
    }
    IEnumerator DelayedInitialSetup()
    {
        yield return null;
        yield return null;
        focusedInteractable = null;
        RegisterAllInteractables();
        RefreshAllNotifications();
    }
    void RegisterAllInteractables()
    {
        Interactable[] all = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
        foreach (var interactable in all)
        {
            if (interactable != null) RegisterInteractable(interactable);
        }
    }
    public void RegisterInteractable(Interactable interactable)
    {
        if (interactable == null || registeredInteractables.Contains(interactable)) return;
        registeredInteractables.Add(interactable);
        SpriteRenderer imgAlert = FindImgAlert(interactable);
        if (imgAlert != null)
        {
            imgAlert.sprite = null;
            imgAlert.gameObject.SetActive(false);
        }
    }
    public void UnregisterInteractable(Interactable interactable)
    {
        if (interactable != null) registeredInteractables.Remove(interactable);
    }
    void OnInteractableFocused(Interactable interactable)
    {
        if (focusedInteractable != null && focusedInteractable != interactable)
        {
            Interactable old = focusedInteractable;
            focusedInteractable = null;
            UpdateNotificationForInteractable(old);
        }
        focusedInteractable = interactable;
        UpdateNotificationForInteractable(interactable);
    }
    void OnInteractableBlurred(Interactable interactable)
    {
        if (focusedInteractable == interactable)
        {
            focusedInteractable = null;
        }
        UpdateNotificationForInteractable(interactable);
    }
    public void RefreshAllNotifications()
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
        if (imgAlert != null)
        {
            UpdateImgAlert(imgAlert, type);
        }
    }
    void UpdateImgAlert(SpriteRenderer imgAlert, NotificationType type)
    {
        if (imgAlert == null) return;
        if (type == NotificationType.None)
        {
            if (imgAlert.gameObject.activeSelf || imgAlert.sprite != null)
            {
                imgAlert.sprite = null;
                imgAlert.gameObject.SetActive(false);
            }
            return;
        }
        Sprite targetSprite = GetSpriteForNotification(type);
        if (targetSprite == null)
        {
            imgAlert.sprite = null;
            imgAlert.gameObject.SetActive(false);
        }
        else
        {
            if (imgAlert.sprite != targetSprite || !imgAlert.gameObject.activeSelf)
            {
                imgAlert.sprite = targetSprite;
                imgAlert.gameObject.SetActive(true);
            }
        }
    }
    Sprite GetSpriteForNotification(NotificationType type)
    {
        if (notificationIcons == null) return null;
        switch (type)
        {
            case NotificationType.Alert: return notificationIcons.iconAlert;
            case NotificationType.Tournament: return notificationIcons.iconTournament;
            case NotificationType.RareCard: return notificationIcons.iconRareCard;
            case NotificationType.LostCard: return notificationIcons.iconLostCard;
            case NotificationType.Duel: return notificationIcons.iconDuel;
            default: return null;
        }
    }
    SpriteRenderer FindImgAlert(Interactable interactable)
    {
        if (interactable == null) return null;
        var provider = interactable as INotificationProvider;
        if (provider != null) return provider.GetImgAlert();
        Transform t = interactable.transform.Find("imgAlert");
        return t != null ? t.GetComponent<SpriteRenderer>() : null;
    }
    NotificationType DetermineNotificationType(Interactable interactable)
    {
        bool isFocused = (focusedInteractable == interactable);
        var provider = interactable as INotificationProvider;
        if (provider != null)
        {
            NotificationType pType = provider.GetNotificationType();
            if (pType != NotificationType.None) return pType;
        }
        if (HasActiveItem())
        {
            if (HasRareCard(interactable)) return NotificationType.RareCard;
            if (CanDuel(interactable)) return NotificationType.Duel;
        }
        if (isFocused) return NotificationType.Alert;
        return NotificationType.None;
    }
    bool HasActiveItem()
    {
        if (itemFlags == null || itemFlags.Count == 0) return false;
        if (saveZone == null) saveZone = FindFirstObjectByType<SaveClientZone>();
        return saveZone != null && itemFlags.Any(f => f != null && saveZone.HasFlag(f));
    }
    bool HasRareCard(Interactable interactable)
    {
        var icg = interactable.GetComponent<InteractableCardGame>() ?? interactable.GetComponentInChildren<InteractableCardGame>();
        if (icg != null && icg.gameSetup?.opponent?.deck != null)
        {
            return icg.gameSetup.opponent.deck.cards.Any(c => c != null && c.rarity == CardRarity.Rare);
        }
        return false;
    }
    bool CanDuel(Interactable interactable)
    {
        return (interactable.GetComponent<InteractableCardGame>() ?? interactable.GetComponentInChildren<InteractableCardGame>()) != null;
    }
    public bool HasItemFlag(SOZoneFlag flag)
    {
        if (flag == null) return false;
        if (saveZone == null) saveZone = FindFirstObjectByType<SaveClientZone>();
        return saveZone != null && saveZone.HasFlag(flag);
    }
    public void RefreshNotifications() => RefreshAllNotifications();
}

