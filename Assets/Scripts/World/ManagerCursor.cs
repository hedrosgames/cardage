using UnityEngine;
using UnityEngine.InputSystem;
public class ManagerCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = new Vector2(0f, 0f);
    public CursorMode cursorMode = CursorMode.Auto;
    public float idleTimeToHide = 3f;
    public float moveThreshold = 0.5f;
    Vector2 lastMousePosition;
    float lastActivityTime;
    bool isHidden;
    void Awake()
    {
        ApplyCursor();
        lastMousePosition = GetMousePosition();
        lastActivityTime = Time.unscaledTime;
        Cursor.visible = true;
        isHidden = false;
    }
    void OnEnable()
    {
        ApplyCursor();
    }
    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
        Cursor.visible = true;
        isHidden = false;
    }
    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        Vector2 currentPos = GetMousePosition();
        bool moved = (currentPos - lastMousePosition).sqrMagnitude > moveThreshold * moveThreshold;
        bool clicked = mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.middleButton.isPressed;
        if (moved || clicked)
        {
            lastActivityTime = Time.unscaledTime;
            lastMousePosition = currentPos;
            if (isHidden)
            {
                Cursor.visible = true;
                isHidden = false;
            }
        }
        if (!isHidden && Time.unscaledTime - lastActivityTime >= idleTimeToHide)
        {
            Cursor.visible = false;
            isHidden = true;
        }
    }
    void ApplyCursor()
    {
        if (cursorTexture == null) return;
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
    }
    Vector2 GetMousePosition()
    {
        var mouse = Mouse.current;
        if (mouse == null) return lastMousePosition;
        return mouse.position.ReadValue();
    }
}

