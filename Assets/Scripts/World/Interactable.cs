using UnityEngine;
using UnityEngine.Events;
public class Interactable : MonoBehaviour
{
    public bool autoInteract = false;
    [Header("Salvamento Automático")]
    [Tooltip("Tipo de salvamento que será executado ao interagir. Escolha 'None' para não salvar.")]
    public SaveId saveOnInteract = SaveId.None;
    [Header("Eventos de Interação")]
    [Tooltip("Evento disparado no momento da interação.")]
    public UnityEvent OnInteractionStart;
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
    protected void TriggerSave()
    {
        if (OnInteractionStart != null && OnInteractionStart.GetPersistentEventCount() > 0)
        {
            OnInteractionStart.Invoke();
        }
        if (saveOnInteract != SaveId.None)
        {
            SaveHelper.SaveByEnum(saveOnInteract);
        }
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

