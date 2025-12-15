using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UISelectedIcon : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Graphic targetGraphic;
    public float fadeSpeed = 10f;
    public float visibleAlpha = 1f;
    public float hiddenAlpha = 0f;
    bool active;
    float currentAlpha;
    void Awake()
    {
        if (targetGraphic == null) return;
        Color c = targetGraphic.color;
        currentAlpha = hiddenAlpha;
        c.a = hiddenAlpha;
        targetGraphic.color = c;
    }
    void Update()
    {
        if (targetGraphic == null) return;
        float target = active ? visibleAlpha : hiddenAlpha;
        currentAlpha = Mathf.Lerp(currentAlpha, target, Time.unscaledDeltaTime * fadeSpeed);
        Color c = targetGraphic.color;
        c.a = currentAlpha;
        targetGraphic.color = c;
    }
    public void OnSelect(BaseEventData eventData)
    {
        active = true;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        active = false;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        active = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        active = false;
    }
}

