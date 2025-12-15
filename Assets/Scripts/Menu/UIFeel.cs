using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
[System.Flags]
public enum UIFeelEffect
{
    None = 0,
    HoverScale = 1 << 0,
    PressScale = 1 << 1,
    HoverPunch = 1 << 2,
    HoverLift = 1 << 3,
    SlideHorizontal = 1 << 4,
    SlideVertical = 1 << 5,
    Rotate = 1 << 6,
    WiggleRotate = 1 << 7,
    WiggleScale = 1 << 8,
    FadeOnHover = 1 << 9,
    ColorTint = 1 << 10,
    Shake = 1 << 11,
    Tilt3D = 1 << 12,
    FloatLoop = 1 << 13
}
public class UIFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public UIFeelEffect effects = UIFeelEffect.HoverScale;
    public float transitionSpeed = 12f;
    [Header("Scale")]
    public float hoverScaleMultiplier = 1.08f;
    public float pressScaleMultiplier = 0.9f;
    public float punchScaleMultiplier = 1.12f;
    public float wiggleScaleAmount = 0.05f;
    public float wiggleScaleFrequency = 12f;
    [Header("Move / Slide / Shake")]
    public float liftOffsetY = 8f;
    public float slideHorizontalOffset = 8f;
    public float slideVerticalOffset = 8f;
    public float shakeAmount = 3f;
    [Header("Float Loop (Auto)")]
    public float floatSpeed = 2f;
    public float floatAmplitude = 5f;
    [Header("Rotate / Tilt")]
    public float rotateAngleZ = 5f;
    public float wiggleRotateAngle = 5f;
    public float wiggleRotateFrequency = 12f;
    public float tiltMaxAngle = 8f;
    [Header("Fade / Color")]
    public float fadeMinAlpha = 0.6f;
    public Color tintColor = Color.yellow;
    RectTransform rect;
    Vector3 baseScale;
    Vector3 baseLocalPos;
    Quaternion baseRotation;
    Graphic graphic;
    TMP_Text tmpText;
    Color baseColor;
    float baseAlpha;
    bool hovering;
    bool pressing;
    float hoverTime;
    Vector2 lastPointerPos;
    Camera lastEventCamera;
    void Awake()
    {
        rect = transform as RectTransform;
        baseScale = transform.localScale;
        baseLocalPos = rect != null ? rect.anchoredPosition3D : transform.localPosition;
        baseRotation = transform.localRotation;
        graphic = GetComponent<Graphic>();
        tmpText = GetComponent<TMP_Text>();
        if (graphic != null)
        {
            baseColor = graphic.color;
            baseAlpha = baseColor.a;
        }
        else if (tmpText != null)
        {
            baseColor = tmpText.color;
            baseAlpha = baseColor.a;
        }
        else
        {
            baseColor = Color.white;
            baseAlpha = 1f;
        }
    }
    void Update()
    {
        bool hasHover = hovering || pressing;
        if (hovering) hoverTime += Time.unscaledDeltaTime;
        else hoverTime = 0f;
        Vector3 targetScale = baseScale;
        Vector3 offsetPos = Vector3.zero;
        float zRot = 0f;
        float tiltX = 0f;
        float tiltY = 0f;
        Color targetColor = baseColor;
        float targetAlpha = baseAlpha;
        if ((effects & UIFeelEffect.HoverScale) != 0)
        {
            if (hovering) targetScale *= hoverScaleMultiplier;
        }
        if ((effects & UIFeelEffect.PressScale) != 0)
        {
            if (pressing) targetScale *= pressScaleMultiplier;
        }
        if ((effects & UIFeelEffect.HoverPunch) != 0 && hasHover)
        {
            float p = 1f + Mathf.Sin(hoverTime * wiggleScaleFrequency) * (punchScaleMultiplier - 1f);
            targetScale *= p;
        }
        if ((effects & UIFeelEffect.HoverLift) != 0 && hasHover)
        {
            offsetPos += Vector3.up * liftOffsetY;
        }
        if ((effects & UIFeelEffect.SlideHorizontal) != 0 && hasHover)
        {
            offsetPos += Vector3.right * slideHorizontalOffset;
        }
        if ((effects & UIFeelEffect.SlideVertical) != 0 && hasHover)
        {
            offsetPos += Vector3.up * slideVerticalOffset;
        }
        if ((effects & UIFeelEffect.FloatLoop) != 0)
        {
            float floatY = Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmplitude;
            offsetPos += Vector3.up * floatY;
        }
        if ((effects & UIFeelEffect.Rotate) != 0 && hasHover)
        {
            zRot += rotateAngleZ;
        }
        if ((effects & UIFeelEffect.WiggleRotate) != 0 && hasHover)
        {
            float a = Mathf.Sin(Time.unscaledTime * wiggleRotateFrequency) * wiggleRotateAngle;
            zRot += a;
        }
        if ((effects & UIFeelEffect.WiggleScale) != 0 && hasHover)
        {
            float s = 1f + Mathf.Sin(Time.unscaledTime * wiggleScaleFrequency) * wiggleScaleAmount;
            targetScale *= s;
        }
        if ((effects & UIFeelEffect.Shake) != 0 && hasHover)
        {
            Vector2 shake = Random.insideUnitCircle * shakeAmount;
            offsetPos += new Vector3(shake.x, shake.y, 0f);
        }
        if ((effects & UIFeelEffect.Tilt3D) != 0 && hovering && rect != null)
        {
            Camera cam = lastEventCamera;
            if (cam == null) cam = Camera.main;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, lastPointerPos, cam, out localPoint))
            {
                Vector2 size = rect.rect.size * 0.5f;
                if (size.x > 0.0001f && size.y > 0.0001f)
                {
                    float nx = Mathf.Clamp(localPoint.x / size.x, -1f, 1f);
                    float ny = Mathf.Clamp(localPoint.y / size.y, -1f, 1f);
                    tiltX = -ny * tiltMaxAngle;
                    tiltY = nx * tiltMaxAngle;
                }
            }
        }
        if ((effects & UIFeelEffect.ColorTint) != 0)
        {
            float t = hasHover ? 1f : 0f;
            targetColor = Color.Lerp(baseColor, tintColor, t);
            targetColor.a = baseAlpha;
        }
        if ((effects & UIFeelEffect.FadeOnHover) != 0)
        {
            targetAlpha = hasHover ? baseAlpha : fadeMinAlpha * baseAlpha;
        }
        float dt = Time.unscaledDeltaTime * transitionSpeed;
        Vector3 finalScale = Vector3.Lerp(transform.localScale, targetScale, dt);
        Vector3 finalPosBase;
        if (rect != null)
        {
            finalPosBase = Vector3.Lerp(rect.anchoredPosition3D, baseLocalPos + offsetPos, dt);
        }
        else
        {
            finalPosBase = Vector3.Lerp(transform.localPosition, baseLocalPos + offsetPos, dt);
        }
        Quaternion targetRot = Quaternion.Euler(tiltX, tiltY, zRot);
        Quaternion finalRot = Quaternion.Lerp(transform.localRotation, baseRotation * targetRot, dt);
        transform.localScale = finalScale;
        if (rect != null)
        {
            rect.anchoredPosition3D = finalPosBase;
        }
        else
        {
            transform.localPosition = finalPosBase;
        }
        transform.localRotation = finalRot;
        if ((effects & (UIFeelEffect.FadeOnHover | UIFeelEffect.ColorTint)) != 0)
        {
            Color c = baseColor;
            if ((effects & UIFeelEffect.ColorTint) != 0)
            {
                c = targetColor;
            }
            if ((effects & UIFeelEffect.FadeOnHover) != 0)
            {
                c.a = Mathf.Lerp(GetCurrentAlpha(), targetAlpha, dt);
            }
            if (graphic != null) graphic.color = c;
            if (tmpText != null) tmpText.color = c;
        }
    }
    float GetCurrentAlpha()
    {
        if (graphic != null) return graphic.color.a;
        if (tmpText != null) return tmpText.color.a;
        return baseAlpha;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        lastPointerPos = eventData.position;
        lastEventCamera = eventData.enterEventCamera;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        pressing = false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        pressing = true;
        lastPointerPos = eventData.position;
        lastEventCamera = eventData.pressEventCamera ?? eventData.enterEventCamera;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        pressing = false;
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        lastPointerPos = eventData.position;
        lastEventCamera = eventData.enterEventCamera;
    }
}

