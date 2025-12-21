using UnityEngine;
using UnityEngine.InputSystem;
using Game.World;
public class PlayerInteract : MonoBehaviour
{
    PlayerMove move;
    Animator anim;
    Transform interactionPoint;
    Collider2D playerCollider;
    Interactable target;
    int mask;
    InputAction interactAction;
    [Header("Cooldown de Interação")]
    [Tooltip("Tempo em segundos para ignorar interações após um diálogo terminar (evita conflito com input E)")]
    public float interactionCooldownAfterDialogue = 0.3f;
    private float canInteractTime = 0f;
    
    // Otimização: Cache para reduzir chamadas de Physics
    private float detectionCheckInterval = 0.1f; // Verificar a cada 0.1s ao invés de cada frame
    private float lastDetectionTime = 0f;
    
    void Awake()
    {
        move = GetComponent<PlayerMove>();
        anim = GetComponentInChildren<Animator>();
        interactionPoint = transform.Find("InteractionPoint");
        if (interactionPoint == null)
        {
            Debug.LogError("InteractionPoint não encontrado no PlayerInteract!");
        }
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        playerCollider = GetComponentInChildren<Collider2D>();
        mask = LayerMask.GetMask("Interactable");
        interactAction = new InputAction("Interact", type: InputActionType.Button, binding: "<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonWest");
    }
    void OnEnable()
    {
        interactAction.performed += OnInteractPerformed;
        interactAction.Enable();
        GameEvents.OnDialogueFinished += OnDialogueFinished;
    }
    void OnDisable()
    {
        interactAction.performed -= OnInteractPerformed;
        interactAction.Disable();
        GameEvents.OnDialogueFinished -= OnDialogueFinished;
        if (target != null)
        {
            target.OnBlur();
            target = null;
        }
    }
    void OnDialogueFinished(SODialogueSequence sequence)
    {
        canInteractTime = Time.time + interactionCooldownAfterDialogue;
    }
    void Update()
    {
        // Otimização: Verificar detecção apenas a intervalos, não a cada frame
        if (Time.time - lastDetectionTime >= detectionCheckInterval)
        {
            DetectInteractable();
            lastDetectionTime = Time.time;
        }
    }
    void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (Time.time < canInteractTime) return;
        if (target != null && !target.autoInteract && Time.timeScale > 0)
        Interact();
    }
    void DetectInteractable()
    {
        // Otimização: Removido transform.Find do Update (já verificado no Awake)
        if (interactionPoint == null) return;
        
        Vector2 detectionPoint = interactionPoint.position;
        float detectionRadius = 0.3f;
        Collider2D col = Physics2D.OverlapCircle(detectionPoint, detectionRadius, mask);
        Interactable newTarget = null;
        if (col != null)
        {
            newTarget = col.GetComponent<Interactable>();
            if (newTarget != null && newTarget.gameObject != null && newTarget.gameObject.activeInHierarchy)
            {
            }
            else
            {
                newTarget = null;
            }
        }
        if (newTarget != target)
        {
            if (target != null && target.gameObject != null)
            {
                target.OnBlur();
            }
            target = newTarget;
            if (target != null && target.gameObject != null)
            {
                target.OnFocus();
                if (target.autoInteract && Time.timeScale > 0 && Time.time >= canInteractTime)
                {
                    Interact();
                }
            }
        }
    }
    void Interact()
    {
        if (target == null || move == null) return;
        Vector2 dir = move.LastDirection;
        if (!target.autoInteract && anim != null)
        {
            anim.ResetTrigger("Interact");
            anim.SetTrigger("Interact");
            anim.SetFloat("directionX", dir.x);
            anim.SetFloat("directionY", dir.y);
        }
        target.OnInteract();
    }
}

