using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMove : MonoBehaviour
{
    public SOPlayerData data;
    Vector3 offsetUp = new Vector3(0, 1.4f, 0);
    Vector3 offsetDown = new Vector3(0, -0.12f, 0);
    Vector3 offsetSide = new Vector3(0.5f, 0.5f, 0);
    Vector2 input;
    public Vector2 LastDirection { get; private set; } = Vector2.down;
    Animator anim;
    SpriteRenderer sr;
    Transform interactionPoint;
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        interactionPoint = transform.Find("InteractionPoint");
    }
    void OnDisable()
    {
        if (anim != null)
        anim.SetBool("isMoving", false);
        input = Vector2.zero;
    }
    void Update()
    {
        ReadInput();
        UpdateDirection();
        UpdateAnimation();
        UpdateFlip();
        MoveCharacter();
        UpdateInteractionPoint();
    }
    void ReadInput()
    {
        input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y = 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y = -1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x = -1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x = 1;
        if (input.x != 0 && input.y != 0)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) input.y = 0;
            else input.x = 0;
        }
    }
    void UpdateDirection()
    {
        if (input != Vector2.zero)
        LastDirection = input;
    }
    void UpdateAnimation()
    {
        bool moving = input != Vector2.zero;
        anim.SetBool("isMoving", moving);
        anim.SetFloat("directionX", LastDirection.x);
        anim.SetFloat("directionY", LastDirection.y);
    }
    void UpdateFlip()
    {
        sr.flipX = LastDirection.x < 0;
    }
    void MoveCharacter()
    {
        transform.position += (Vector3)input * data.moveSpeed * Time.deltaTime;
    }
    void UpdateInteractionPoint()
    {
        if (LastDirection.y > 0)
        interactionPoint.localPosition = offsetUp;
        else if (LastDirection.y < 0)
        interactionPoint.localPosition = offsetDown;
        else
        interactionPoint.localPosition = new Vector3(LastDirection.x * offsetSide.x, offsetSide.y, 0);
    }
}

