using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteract : MonoBehaviour
{
    PlayerMove move;
    Animator anim;
    Transform interactionPoint;
    Interactable target;
    int mask;
    InputAction interactAction;
    void Awake()
    {
        move = GetComponent<PlayerMove>();
        anim = GetComponentInChildren<Animator>();
        interactionPoint = transform.Find("InteractionPoint");
        mask = LayerMask.GetMask("Interactable");
        interactAction = new InputAction("Interact", type: InputActionType.Button, binding: "<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonWest");
    }
    void OnEnable()
    {
        interactAction.performed += OnInteractPerformed;
        interactAction.Enable();
    }
    void OnDisable()
    {
        interactAction.performed -= OnInteractPerformed;
        interactAction.Disable();
        if (target != null)
        {
            target.OnBlur();
            target = null;
        }
    }
    void Update()
    {
        DetectInteractable();
    }
    void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (target != null && !target.autoInteract && Time.timeScale > 0)
        Interact();
    }
    void DetectInteractable()
    {
        Collider2D col = Physics2D.OverlapCircle(interactionPoint.position, 0.4f, mask);
        Interactable newTarget = null;
        if (col != null)
        newTarget = col.GetComponent<Interactable>();
        if (newTarget != target)
        {
            if (target != null) target.OnBlur();
            target = newTarget;
            if (target != null)
            {
                target.OnFocus();
                if (target.autoInteract && Time.timeScale > 0)
                {
                    Interact();
                }
            }
        }
    }
    void Interact()
    {
        Vector2 dir = move.LastDirection;
        if (target != null && !target.autoInteract)
        {
            anim.ResetTrigger("Interact");
            anim.SetTrigger("Interact");
            anim.SetFloat("directionX", dir.x);
            anim.SetFloat("directionY", dir.y);
        }
        if (target != null)
        target.OnInteract();
    }
}

