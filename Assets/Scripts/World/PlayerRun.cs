using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerRun : MonoBehaviour
{
    public float bonusSpeed = 2f;
    public float animMultiplier = 1.5f;
    PlayerMove move;
    Animator anim;
    float normalSpeed;
    float normalAnimSpeed = 1f;
    void Awake()
    {
        move = GetComponent<PlayerMove>();
        anim = GetComponentInChildren<Animator>();
        if (move != null && move.data != null)
        normalSpeed = move.data.moveSpeed;
    }
    void OnDisable()
    {
        if (move != null && move.data != null)
        move.data.moveSpeed = normalSpeed;
        if (anim != null)
        anim.speed = normalAnimSpeed;
    }
    void Update()
    {
        bool running = Keyboard.current.leftShiftKey.isPressed ||
        Keyboard.current.rightShiftKey.isPressed;
        if (running)
        {
            move.data.moveSpeed = normalSpeed + bonusSpeed;
            anim.speed = normalAnimSpeed * animMultiplier;
        }
        else
        {
            move.data.moveSpeed = normalSpeed;
            anim.speed = normalAnimSpeed;
        }
    }
}

