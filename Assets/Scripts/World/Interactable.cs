using UnityEngine;
public class Interactable : MonoBehaviour
{
    public bool autoInteract = false;
    [HideInInspector]public GameObject interactionIcon;
    protected virtual void Awake()
    {
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer != -1)
        gameObject.layer = layer;
        if (interactionIcon == null)
        {
            FindIconAutomatically();
        }
        if (interactionIcon != null)
        interactionIcon.SetActive(false);
    }
    protected virtual void Reset()
    {
        FindIconAutomatically();
    }
    void FindIconAutomatically()
    {
        Transform t = transform.Find("Icon");
        if (t == null)
        {
            var feel = GetComponentInChildren<UIFeel>(true);
            if (feel != null) t = feel.transform;
        }
        if (t == null)
        {
            var sprite = GetComponentInChildren<SpriteRenderer>(true);
            if (sprite != null && sprite.gameObject != this.gameObject)
            t = sprite.transform;
        }
        if (t != null) interactionIcon = t.gameObject;
    }
    public virtual void OnInteract() { }
    public virtual void OnFocus()
    {
        if (this == null || gameObject == null) return;
        if (!autoInteract && interactionIcon != null && interactionIcon.activeSelf != true)
        {
            interactionIcon.SetActive(true);
        }
        GameEvents.OnInteractableFocused?.Invoke(this);
    }
    public virtual void OnBlur()
    {
        if (interactionIcon != null)
        interactionIcon.SetActive(false);
        GameEvents.OnInteractableBlurred?.Invoke(this);
    }
}

