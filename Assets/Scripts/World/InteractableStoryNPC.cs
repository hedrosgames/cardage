using UnityEngine;
using UnityEngine.Events;
using System.Collections;
public class InteractableStoryNPC : Interactable, INotificationProvider
{
    [Header("DiÃ¡logos")]
    public SODialogueSequence dialogue;
    public SODialogueSequence dialogueClosing;
    [Header("FinalizaÃ§Ã£o")]
    public bool destroyOnFinish = true;
    [Header("Linha de VisÃ£o")]
    public SightDirection sightDirection = SightDirection.Right;
    public float sightDistance = 5f;
    [Header("Pontos de Movimento")]
    public Transform pointB;
    public Transform pointC;
    [Header("ConfiguraÃ§Ã£o de Movimento")]
    public float moveSpeed = 1.2f;
    public float arrivalDistance = 0.1f;
    [Header("Delays")]
    public float delayBeforeMoving = 0.3f;
    public float delayBeforeDialogue = 0.3f;
    [Header("Eventos")]
    public UnityEvent onDialogueClosingFinished;
    private SpriteRenderer _cachedAlertRef;
    private Transform playerTransform;
    private PlayerControl playerControl;
    private PlayerMove playerMove;
    private Camera mainCamera;
    private int playerLayer;
    private bool hasTriggered = false;
    private bool isMoving = false;
    private bool isWaitingForDialogue = false;
    private SODialogueSequence activeDialogue;
    private Vector3 initialPosition;
    private bool closingDialogueStarted = false;
    private bool movementToBStarted = false;
    private bool isClosingPhase = false;
    protected override void Awake()
    {
        base.Awake();
        playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer == -1) playerLayer = 0;
        initialPosition = transform.position;
        if (interactionIcon != null && interactionIcon.name == "imgAlert")
        {
            interactionIcon = null;
        }
    }
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerControl = player.GetComponent<PlayerControl>();
            playerMove = player.GetComponent<PlayerMove>();
        }
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = FindFirstObjectByType<Camera>();
        if (ManagerNotification.Instance != null)
        {
            ManagerNotification.Instance.RegisterInteractable(this);
        }
    }
    private void OnEnable() => GameEvents.OnDialogueFinished += OnDialogueFinished;
    private void OnDisable() => GameEvents.OnDialogueFinished -= OnDialogueFinished;
    private void OnDestroy() => ReleasePlayer();
    private void Update()
    {
        if (hasTriggered)
        {
            if (playerControl != null)
            {
                if (playerControl.playerMove != null && playerControl.playerMove.enabled) playerControl.SetMovement(false);
                if (playerControl.playerInteract != null && playerControl.playerInteract.enabled) playerControl.SetInteraction(false);
            }
        }
        if (hasTriggered || isMoving || isWaitingForDialogue) return;
        if (Time.frameCount % 5 == 0) CheckLineOfSight();
    }
    private void CheckLineOfSight()
    {
        if (playerTransform == null || pointB == null) return;
        Vector3 npcPos = transform.position;
        Vector3 playerPos = playerTransform.position;
        float sqrDistance = (playerPos - npcPos).sqrMagnitude;
        if (sqrDistance > sightDistance * sightDistance) return;
        Vector2 rayOrigin = npcPos;
        Vector2 rayDirection = (playerPos - npcPos).normalized;
        float distance = Mathf.Sqrt(sqrDistance);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, distance, 1 << playerLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            TriggerSequence();
        }
    }
    private void TriggerSequence()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        isClosingPhase = false;
        TriggerSave();
        ShowAlert();
        StartCoroutine(StartSequenceAfterNotification());
    }
    private IEnumerator StartSequenceAfterNotification()
    {
        if (ManagerNotification.Instance != null) ManagerNotification.Instance.RefreshNotifications();
        yield return new WaitForSeconds(0.1f);
        LockPlayerCompletely();
        MakePlayerFaceNPC();
        StartCoroutine(DelayedMoveToPointB());
    }
    private IEnumerator DelayedMoveToPointB()
    {
        if (movementToBStarted) yield break;
        movementToBStarted = true;
        yield return new WaitForSeconds(delayBeforeMoving);
        if (pointB != null)
        {
            yield return StartCoroutine(MoveToPoint(pointB.position, OnReachedPointB));
        }
        else
        {
            yield return new WaitForSeconds(delayBeforeDialogue);
            StartDialogue();
        }
    }
    private void OnReachedPointB()
    {
        StartCoroutine(DelayedStartDialogue());
    }
    private IEnumerator DelayedStartDialogue()
    {
        yield return new WaitForSeconds(delayBeforeDialogue);
        StartDialogue();
    }
    private void StartDialogue()
    {
        if (dialogue != null)
        {
            isWaitingForDialogue = true;
            activeDialogue = dialogue;
            isClosingPhase = false;
            GameEvents.OnRequestDialogue?.Invoke(dialogue);
        }
        else
        {
            OnDialogueFinished(null);
        }
    }
    private void OnDialogueFinished(SODialogueSequence sequence)
    {
        if (!isWaitingForDialogue) return;
        if (sequence != null && sequence != activeDialogue && activeDialogue != null) return;
        isWaitingForDialogue = false;
        activeDialogue = null;
        if (isClosingPhase)
        {
            OnDialogueClosingFinished();
        }
        else
        {
            OnDialogueOpeningFinished();
        }
    }
    private void OnDialogueOpeningFinished()
    {
        StartCoroutine(KeepPlayerLockedAfterDialogue());
        if (pointC != null)
        {
            StartCoroutine(MoveToPoint(pointC.position, OnReachedPointC));
        }
        else
        {
            StartClosingDialogue();
        }
    }
    private void OnReachedPointC()
    {
        if (closingDialogueStarted) return;
        StartClosingDialogue();
    }
    private void StartClosingDialogue()
    {
        if (closingDialogueStarted) return;
        closingDialogueStarted = true;
        if (dialogueClosing != null)
        {
            isWaitingForDialogue = true;
            activeDialogue = dialogueClosing;
            isClosingPhase = true;
            GameEvents.OnRequestDialogue?.Invoke(dialogueClosing);
        }
        else
        {
            isClosingPhase = true;
            OnDialogueClosingFinished();
        }
    }
    private void OnDialogueClosingFinished()
    {
        ReleasePlayer();
        StartCoroutine(ExecuteEventsAndDestroy());
    }
    private IEnumerator ExecuteEventsAndDestroy()
    {
        yield return new WaitForSeconds(0.5f);
        try { onDialogueClosingFinished?.Invoke(); }
        catch (System.Exception e) { Debug.LogError($"Erro no UnityEvent: {e.Message}"); }
        if (destroyOnFinish)
        {
            Destroy(gameObject);
        }
    }
    private void LockPlayerCompletely()
    {
        if (playerControl != null) { playerControl.SetControl(false); playerControl.SetMovement(false); }
        if (ManagerPhone.Instance != null) ManagerPhone.Instance.SetTabBlocked(true);
    }
    private void MakePlayerFaceNPC()
    {
        if (playerMove != null && playerTransform != null)
        {
            Vector3 dir = (transform.position - playerTransform.position).normalized;
            playerMove.FaceDirection(new Vector2(dir.x, dir.y));
        }
    }
    private void ReleasePlayer()
    {
        hasTriggered = false;
        if (playerControl != null) playerControl.SetControl(true);
        if (ManagerPhone.Instance != null) ManagerPhone.Instance.SetTabBlocked(false);
    }
    private IEnumerator MoveToPoint(Vector3 targetPosition, System.Action onArrival)
    {
        if (isMoving) yield break;
        isMoving = true;
        Animator npcAnim = GetComponentInChildren<Animator>();
        SpriteRenderer npcSR = GetComponentInChildren<SpriteRenderer>();
        float timeOut = 10f;
        float timer = 0f;
        while (Vector3.Distance(transform.position, targetPosition) > arrivalDistance)
        {
            timer += Time.deltaTime;
            if (timer > timeOut)
            {
                break;
            }
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            if (npcAnim != null)
            {
                npcAnim.SetFloat("directionX", direction.x);
                npcAnim.SetFloat("directionY", direction.y);
                npcAnim.SetBool("isMoving", true);
            }
            if (npcSR != null && direction.x != 0) npcSR.flipX = direction.x < 0;
            yield return null;
        }
        if (npcAnim != null) npcAnim.SetBool("isMoving", false);
        transform.position = targetPosition;
        isMoving = false;
        onArrival?.Invoke();
    }
    private IEnumerator KeepPlayerLockedAfterDialogue()
    {
        yield return new WaitForEndOfFrame();
        if (playerControl != null && activeDialogue != dialogueClosing)
        {
            playerControl.SetMovement(false);
            playerControl.SetInteraction(false);
        }
    }
    private Vector3 GetSightDirectionVector()
    {
        switch (sightDirection)
        {
            case SightDirection.Up: return Vector3.up;
            case SightDirection.Down: return Vector3.down;
            case SightDirection.Left: return Vector3.left;
            default: return Vector3.right;
        }
    }
    public NotificationType GetNotificationType()
    {
        if (playerTransform == null) return NotificationType.None;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist < sightDistance && !hasTriggered) return NotificationType.Alert;
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
                if (child.name == "imgAlert") { t = child; break; }
            }
        }
        if (t != null) _cachedAlertRef = t.GetComponent<SpriteRenderer>();
        return _cachedAlertRef;
    }
    private void ShowAlert()
    {
        if (ManagerNotification.Instance != null) ManagerNotification.Instance.RefreshNotifications();
    }
    public void ForceDestroyNPC()
    {
        Destroy(gameObject);
    }
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (pointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, pointB.position);
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
        }
        if (pointC != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 startC = pointB != null ? pointB.position : transform.position;
            Gizmos.DrawLine(startC, pointC.position);
            Gizmos.DrawWireSphere(pointC.position, 0.2f);
        }
        Vector3 direction = GetSightDirectionVector();
        UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.2f);
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + direction * sightDistance);
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.forward, 0.5f);
        Vector3 endPoint = transform.position + direction * sightDistance;
        UnityEditor.Handles.DrawBezier(transform.position, endPoint, transform.position, endPoint, Color.black, null, 5f);
    }
    #endif
}

