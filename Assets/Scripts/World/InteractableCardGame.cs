using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public enum SightDirection
{
    Up,
    Down,
    Left,
    Right
}
public class InteractableCardGame : Interactable
{
    [Header("Configuração do Card Game")]
    [Tooltip("Setup do jogo de cartas (oponente, deck, etc)")]
    public SOGameSetup gameSetup;
    [Header("Identificação do NPC")]
    [Tooltip("ID único deste NPC (usado para salvar se já desafiou)")]
    public string npcId;
    [Tooltip("Flag que armazena a lista de IDs de NPCs que já desafiaram")]
    public SOZoneFlag challengedNpcsFlag;
    [Header("Modo: Interação Normal (autoInteract = false)")]
    [Tooltip("Diálogo exibido antes de iniciar o card game")]
    public SODialogueSequence dialogueBeforeGame;
    [Header("Modo: Desafio Automático (autoInteract = true)")]
    [Tooltip("Direção da linha de detecção")]
    public SightDirection sightDirection = SightDirection.Right;
    [Tooltip("Distância máxima da linha de detecção")]
    public float sightDistance = 5f;
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
            CheckLineOfSight();
        }
    }
    public override void OnInteract()
    {
        if (autoInteract) return;
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
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            if (playerTransform == null) return;
        }
        Vector3 npcPos = transform.position;
        Vector3 playerPos = playerTransform.position;
        Vector3 direction = GetSightDirectionVector();
        Vector3 toPlayer = playerPos - npcPos;
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
        float distance = Vector3.Distance(npcPos, playerPos);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, distance, 1 << playerLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            MarkAsChallenged();
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
            case SightDirection.Up:
            return Vector3.up;
            case SightDirection.Down:
            return Vector3.down;
            case SightDirection.Left:
            return Vector3.left;
            case SightDirection.Right:
            return Vector3.right;
            default:
            return Vector3.right;
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
        if (gameSetup == null)
        {
            return;
        }
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
        if (ManagerSave.Instance != null)
        {
            SaveClientWorld saveWorld = FindFirstObjectByType<SaveClientWorld>();
            if (saveWorld != null && saveWorld.saveDefinition != null && saveWorld.gameObject.activeInHierarchy)
            {
                ManagerSave.Instance.RegisterClient(saveWorld.saveDefinition, saveWorld);
            }
        }
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.SaveAll();
        }
        StartCoroutine(LoadCardGameAfterSave());
    }
    System.Collections.IEnumerator LoadCardGameAfterSave()
    {
        yield return null;
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
}

