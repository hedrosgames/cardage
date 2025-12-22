using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
public enum SightDirection
{
    Up,
    Down,
    Left,
    Right
}
public class InteractableCardGame : Interactable, INotificationProvider
{
    [Header("ConfiguraÃ§Ã£o do Card Game")]
    public SOGameSetup gameSetup;
    [Header("IdentificaÃ§Ã£o do NPC")]
    public string npcId;
    public SOZoneFlag challengedNpcsFlag;
    [Header("Modo: InteraÃ§Ã£o Normal (autoInteract = false)")]
    public SODialogueSequence dialogueBeforeGame;
    [Header("Modo: Desafio AutomÃ¡tico (autoInteract = true)")]
    public SightDirection sightDirection = SightDirection.Right;
    public float sightDistance = 5f;
    [Header("NotificaÃ§Ãµes")]
    public SOZoneFlag notificationItemFlag;
    public bool hasRareCard = false;
    public bool isInTournament = false;
    public bool hasRecoverableCard = false;
    public bool canDuel = false;
    private bool isWaitingForDialogue = false;
    private SODialogueSequence activeDialogue;
    private Transform playerTransform;
    private ManagerCardGameChallenge challengeManager;
    private SaveClientZone saveZone;
    private int playerLayer;
    private bool originalAutoInteract;
    protected override void Awake()
    {
        base.Awake();
        FindOrCreateChallengeManager();
        saveZone = FindFirstObjectByType<SaveClientZone>();
        playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer == -1) playerLayer = 0;
        originalAutoInteract = autoInteract;
    }
    void FindOrCreateChallengeManager()
    {
        challengeManager = Object.FindFirstObjectByType<ManagerCardGameChallenge>();
        if (challengeManager == null)
        {
            GameObject go = new GameObject("ManagerCardGameChallenge");
            challengeManager = go.AddComponent<ManagerCardGameChallenge>();
        }
    }
    void OnEnable()
    {
        GameEvents.OnDialogueFinished += OnDialogueFinished;
        if (saveZone != null)
        {
            saveZone.OnLoadComplete += OnSaveLoaded;
        }
    }
    void OnDisable()
    {
        GameEvents.OnDialogueFinished -= OnDialogueFinished;
        if (saveZone != null)
        {
            saveZone.OnLoadComplete -= OnSaveLoaded;
        }
    }
    void Start()
    {
        CheckIfAlreadyChallenged();
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }
    void OnSaveLoaded()
    {
        CheckIfAlreadyChallenged();
    }
    void CheckIfAlreadyChallenged()
    {
        if (string.IsNullOrEmpty(npcId) || challengedNpcsFlag == null || saveZone == null) return;
        if (saveZone.HasIdInFlag(challengedNpcsFlag, npcId))
        {
            autoInteract = false;
        }
        else
        {
            autoInteract = originalAutoInteract;
        }
    }
    void Update()
    {
        if (autoInteract)
        {
            if (Time.frameCount % 5 == 0)
            {
                CheckLineOfSight();
            }
        }
    }
    public override void OnFocus()
    {
        base.OnFocus();
    }
    public override void OnInteract()
    {
        if (autoInteract) return;
        TriggerSave();
        if (dialogueBeforeGame != null)
        {
            StartDialogueSequence();
        }
        else
        {
            StartCardGame();
        }
    }
    void StartDialogueSequence()
    {
        isWaitingForDialogue = true;
        activeDialogue = dialogueBeforeGame;
        GameEvents.OnRequestDialogue?.Invoke(dialogueBeforeGame);
    }
    void OnDialogueFinished(SODialogueSequence sequence)
    {
        if (isWaitingForDialogue && sequence == activeDialogue)
        {
            isWaitingForDialogue = false;
            activeDialogue = null;
            StartCardGame();
        }
    }
    void CheckLineOfSight()
    {
        if (isWaitingForDialogue) return;
        if (playerTransform == null) return;
        Vector3 npcPos = transform.position;
        Vector3 playerPos = playerTransform.position;
        Vector3 direction = GetSightDirectionVector();
        Vector3 toPlayer = playerPos - npcPos;
        float sqrDistance = toPlayer.sqrMagnitude;
        if (sqrDistance > sightDistance * sightDistance) return;
        Vector3 normalizedToPlayer = toPlayer.normalized;
        Vector3 normalizedDirection = direction.normalized;
        float dot = Vector3.Dot(normalizedDirection, normalizedToPlayer);
        if (dot < 0.999f) return;
        float distanceAlongLine = Vector3.Dot(toPlayer, normalizedDirection);
        if (distanceAlongLine <= 0 || distanceAlongLine > sightDistance) return;
        Vector3 projectedPoint = npcPos + normalizedDirection * distanceAlongLine;
        float distanceFromLine = Vector3.Distance(playerPos, projectedPoint);
        if (distanceFromLine > 0.1f) return;
        Vector2 rayOrigin = npcPos;
        Vector2 rayDirection = (playerPos - npcPos).normalized;
        float distance = Mathf.Sqrt(sqrDistance);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, distance, 1 << playerLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            MarkAsChallenged();
            TriggerSave();
            if (dialogueBeforeGame != null)
            {
                StartDialogueSequence();
            }
            else
            {
                StartCardGame();
            }
        }
    }
    Vector3 GetSightDirectionVector()
    {
        switch (sightDirection)
        {
            case SightDirection.Up: return Vector3.up;
            case SightDirection.Down: return Vector3.down;
            case SightDirection.Left: return Vector3.left;
            case SightDirection.Right: return Vector3.right;
            default: return Vector3.right;
        }
    }
    void MarkAsChallenged()
    {
        if (string.IsNullOrEmpty(npcId) || challengedNpcsFlag == null || saveZone == null) return;
        saveZone.AddIdToFlag(challengedNpcsFlag, npcId);
        autoInteract = false;
    }
    void StartCardGame()
    {
        if (gameSetup == null) return;
        if (!string.IsNullOrEmpty(npcId) && challengedNpcsFlag != null && saveZone != null)
        {
            if (!saveZone.HasIdInFlag(challengedNpcsFlag, npcId))
            {
                MarkAsChallenged();
            }
        }
        if (challengeManager != null)
        {
            challengeManager.SetGameSetup(gameSetup);
        }
        StartCoroutine(LoadCardGameAfterSave());
    }
    System.Collections.IEnumerator LoadCardGameAfterSave()
    {
        yield return new WaitForSeconds(0.1f);
        string sceneName = "CardGame";
        if (ManagerSceneTransition.Instance != null)
        {
            ManagerSceneTransition.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    void OnDrawGizmosSelected()
    {
        if (!autoInteract) return;
        Vector3 npcPos = transform.position;
        Vector3 direction = GetSightDirectionVector();
        Vector3 endPoint = npcPos + direction.normalized * sightDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(npcPos, endPoint);
        Gizmos.DrawWireSphere(npcPos, 0.15f);
        Gizmos.DrawWireSphere(endPoint, 0.2f);
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        for (int i = 0; i < 5; i++)
        {
            float offset = (i - 2) * 0.05f;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
            Vector3 startOffset = npcPos + perpendicular * offset;
            Vector3 endOffset = endPoint + perpendicular * offset;
            Gizmos.DrawLine(startOffset, endOffset);
        }
    }
    public NotificationType GetNotificationType()
    {
        bool hasItemFlag = false;
        if (notificationItemFlag != null && ManagerNotification.Instance != null)
        {
            hasItemFlag = ManagerNotification.Instance.HasItemFlag(notificationItemFlag);
        }
        if (!hasItemFlag) return NotificationType.None;
        if (hasRecoverableCard) return NotificationType.LostCard;
        if (hasRareCard) return NotificationType.RareCard;
        if (isInTournament) return NotificationType.Tournament;
        if (canDuel) return NotificationType.Duel;
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

