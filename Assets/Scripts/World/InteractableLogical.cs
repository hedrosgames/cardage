using UnityEngine;
using UnityEngine.Events;
using System.Collections;
public class InteractableLogical : Interactable, INotificationProvider
{
    public SOZoneFlag requiredFlag;
    [HideInInspector] public bool invertCondition;
    [HideInInspector] public SOZoneFlag onLoadCheckFlag;
    [HideInInspector] public bool disableIfFlagExists = true;
    [HideInInspector] public SOZoneFlag notificationItemFlag;
    [HideInInspector] public NotificationType notificationTypeWhenItemActive = NotificationType.None;
    [Header("[SE TIVER A FLAG OU FOR VAZIO]")]
    public SODialogueSequence dialogueOnSuccess;
    public SOZoneFlag setFlagOnSuccess;
    public GameObject disableObjectOnFinished;
    public UnityEvent onInteractSuccess;
    [Header("[SE NÃO TIVER A FLAG]")]
    public SODialogueSequence dialogueOnFail;
    public UnityEvent onInteractFail;
    private SaveClientZone saveZone;
    private bool isWaitingForDialogue = false;
    private float interactionCooldown = 0f;
    private SODialogueSequence activeDialogue;
    protected override void Awake()
    {
        base.Awake();
        if (saveZone == null) saveZone = FindFirstObjectByType<SaveClientZone>();
    }
    void OnEnable() => GameEvents.OnDialogueFinished += OnGlobalDialogueFinished;
    void OnDisable() => GameEvents.OnDialogueFinished -= OnGlobalDialogueFinished;
    private void Start()
    {
        if (saveZone != null)
        {
            UpdateVisualState();
            saveZone.OnLoadComplete += UpdateVisualState;
        }
    }
    void OnDestroy()
    {
        if (saveZone != null) saveZone.OnLoadComplete -= UpdateVisualState;
    }
    public void UpdateVisualState()
    {
        if (disableObjectOnFinished == null || onLoadCheckFlag == null || saveZone == null) return;
        bool hasFlag = saveZone.HasFlag(onLoadCheckFlag);
        bool shouldBeActive = disableIfFlagExists ? !hasFlag : hasFlag;
        if (disableObjectOnFinished.activeSelf != shouldBeActive)
        {
            disableObjectOnFinished.SetActive(shouldBeActive);
            if (!shouldBeActive) OnBlur();
        }
    }
    public override void OnInteract()
    {
        if (isWaitingForDialogue || Time.time < interactionCooldown) return;
        
        // Executa o evento e o save centralizado
        TriggerSave();

        if (saveZone == null) saveZone = FindFirstObjectByType<SaveClientZone>();
        bool conditionMet = CheckCondition();
        if (conditionMet)
        HandleInteraction(dialogueOnSuccess, true);
        else
        HandleInteraction(dialogueOnFail, false);
    }
    private void HandleInteraction(SODialogueSequence dialogue, bool isSuccess)
    {
        if (dialogue != null)
        {
            isWaitingForDialogue = true;
            activeDialogue = dialogue;
            GameEvents.OnRequestDialogue?.Invoke(dialogue);
            StartCoroutine(WaitForDialogueRoutine(isSuccess));
        }
        else
        {
            ExecuteLogic(isSuccess);
        }
    }
    private void OnGlobalDialogueFinished(SODialogueSequence seq)
    {
        if (isWaitingForDialogue && seq == activeDialogue)
        {
            isWaitingForDialogue = false;
            activeDialogue = null;
            interactionCooldown = Time.time + 0.5f;
        }
    }
    private IEnumerator WaitForDialogueRoutine(bool isSuccess)
    {
        while (isWaitingForDialogue) yield return null;
        yield return new WaitForEndOfFrame();
        ExecuteLogic(isSuccess);
    }
    private void ExecuteLogic(bool isSuccess)
    {
        if (isSuccess)
        {
            if (setFlagOnSuccess != null && saveZone != null)
            saveZone.SetFlag(setFlagOnSuccess, 1);
            onInteractSuccess?.Invoke();
            if (disableObjectOnFinished != null)
            UpdateVisualState();
        }
        else
        {
            onInteractFail?.Invoke();
        }
    }
    bool CheckCondition()
    {
        if (requiredFlag == null) return true;
        if (saveZone == null) return false;
        bool hasFlag = saveZone.HasFlag(requiredFlag);
        return invertCondition ? !hasFlag : hasFlag;
    }
    public NotificationType GetNotificationType()
    {
        if (notificationItemFlag != null && notificationTypeWhenItemActive != NotificationType.None)
        {
            if (ManagerNotification.Instance != null && ManagerNotification.Instance.HasItemFlag(notificationItemFlag))
            {
                return notificationTypeWhenItemActive;
            }
        }
        return NotificationType.None;
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

