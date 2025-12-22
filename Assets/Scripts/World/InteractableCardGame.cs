using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events; // Necessário para UnityEvent

public enum SightDirection
{
    Up,
    Down,
    Left,
    Right
}

public class InteractableCardGame : Interactable, INotificationProvider
{
    [Header("Configuração do Card Game")]
    [Tooltip("Setup do jogo de cartas (oponente, deck, etc)")]
    public SOGameSetup gameSetup;

    // --- NOVO CAMPO ADICIONADO ---
    [Header("Eventos")]
    public UnityEvent OnDialogueStart;
    // ----------------------------

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

    [Header("Notificações")]
    [Tooltip("Flag de item necessária para mostrar notificações especiais")]
    public SOZoneFlag notificationItemFlag;
    [Tooltip("Este NPC tem carta rara no deck")]
    public bool hasRareCard = false;
    [Tooltip("Este NPC está participando do torneio")]
    public bool isInTournament = false;
    [Tooltip("Este NPC tem carta recuperável")]
    public bool hasRecoverableCard = false;
    [Tooltip("Ícone de duelo disponível (mostra quando item ativo)")]
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
        Debug.Log($"[InteractableCardGame] OnFocus disparado para NPC: {npcId}. AutoInteract: {autoInteract}");
    }

    public override void OnInteract()
    {
        Debug.Log($"[InteractableCardGame] OnInteract chamado para NPC: {npcId}. AutoInteract: {autoInteract}");
        if (autoInteract) return;
        Debug.Log($"[InteractableCardGame] Executando lógica de interação manual.");
        
        // Aciona o evento de início de interação/diálogo
        if (OnDialogueStart != null && OnDialogueStart.GetPersistentEventCount() > 0)
        {
            Debug.Log($"[InteractableCardGame] Invocando UnityEvent OnDialogueStart.");
            OnDialogueStart.Invoke();
        }
        else
        {
            Debug.Log("[InteractableCardGame] OnDialogueStart vazio ou sem ouvintes. Chamando SaveWorld via Fallback Estático.");
            SaveHelper.SaveWorld();
        }

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
        Debug.Log($"[InteractableCardGame] Iniciando sequência de diálogo para NPC: {npcId}");
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
            Debug.Log($"[InteractableCardGame] Interação automática (Line of Sight) detectada para NPC: {npcId}");
            MarkAsChallenged();

            // Aciona o evento de início de interação/diálogo também no modo automático
            if (OnDialogueStart != null && OnDialogueStart.GetPersistentEventCount() > 0)
            {
                Debug.Log($"[InteractableCardGame] Invocando UnityEvent OnDialogueStart (Modo Automático).");
                OnDialogueStart.Invoke();
            }
            else
            {
                Debug.Log("[InteractableCardGame] OnDialogueStart vazio no modo automático. Chamando SaveWorld via Fallback Estático.");
                SaveHelper.SaveWorld();
            }

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

        // --- INVOCAÇÃO DO EVENTO (DEBUG) ---
        Debug.Log($"[InteractableCardGame] StartCardGame executado para NPC: {npcId}. O evento OnDialogueStart deve ter sido disparado.");
        // ----------------------------

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
        // Aguarda um pouco mais para garantir que a gravação do arquivo de save terminou
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