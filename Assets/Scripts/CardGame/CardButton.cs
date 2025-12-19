using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class CardButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CardView cardView;
    public float hoverScale = 1.12f;
    public float hoverLift = 20f;
    public float hoverSmooth = 10f;
    public float rotationSmooth = 10f;
    public RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private SOCardData cardData;
    private ManagerGame gameManager;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private int originalSiblingIndex;
    private bool interactable;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool dragging;
    private Vector2 targetAnchoredPosition;
    private Vector3 targetScale;
    private Quaternion targetRotation;
    private int ownerId;
    public bool IsDragging() => dragging;
    public void Setup(SOCardData data, ManagerGame gm, int id, bool isInteractable)
    {
        cardData = data;
        gameManager = gm;
        interactable = isInteractable;
        ownerId = id;
        if (cardView != null)
        cardView.Setup(data, id);
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
            originalRotation = rectTransform.localRotation;
            originalAnchoredPosition = rectTransform.anchoredPosition;
            if (float.IsNaN(originalRotation.x) || float.IsNaN(originalRotation.y) ||
            float.IsNaN(originalRotation.z) || float.IsNaN(originalRotation.w))
            {
                originalRotation = Quaternion.identity;
            }
            targetScale = originalScale;
            targetRotation = originalRotation;
            targetAnchoredPosition = originalAnchoredPosition;
        }
        if (canvasGroup == null) {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        Button btn = GetComponent<Button>();
        if (btn != null) btn.interactable = isInteractable;
        if (!isInteractable) canvasGroup.blocksRaycasts = false;
    }
    public void UpdateInitialStateFromLayout()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
            targetAnchoredPosition = originalAnchoredPosition;
            originalScale = rectTransform.localScale;
            targetScale = originalScale;
            originalRotation = rectTransform.localRotation;
            if (float.IsNaN(originalRotation.x) || float.IsNaN(originalRotation.y) ||
            float.IsNaN(originalRotation.z) || float.IsNaN(originalRotation.w))
            {
                originalRotation = Quaternion.identity;
            }
            targetRotation = originalRotation;
        }
    }
    void Update()
    {
        if (rectTransform == null) return;
        if (float.IsNaN(targetRotation.x) || float.IsNaN(targetRotation.y) ||
        float.IsNaN(targetRotation.z) || float.IsNaN(targetRotation.w))
        {
            targetRotation = originalRotation;
        }
        if (float.IsNaN(rectTransform.localRotation.x) || float.IsNaN(rectTransform.localRotation.y) ||
        float.IsNaN(rectTransform.localRotation.z) || float.IsNaN(rectTransform.localRotation.w))
        {
            rectTransform.localRotation = originalRotation;
        }
        if (!dragging && !cardView.isOpponentHand)
        {
            if (cardView.handLayout != null && cardView.handLayout.isAnimatingOpen) return;
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetAnchoredPosition, Time.deltaTime * hoverSmooth);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * hoverSmooth);
        }
        float lerpFactor = Mathf.Clamp01(Time.deltaTime * rotationSmooth);
        if (lerpFactor > 0 && !float.IsNaN(lerpFactor))
        {
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, targetRotation, lerpFactor);
        }
    }
    public SOCardData GetCardData()=> cardData;
    public void Disable(){interactable=false;gameObject.SetActive(false);}
    public void OnPointerEnter(PointerEventData eventData){if(!interactable)return;if(cardView.isOpponentHand)return;targetScale=originalScale*hoverScale;targetAnchoredPosition=originalAnchoredPosition+new Vector2(0,hoverLift);}
    public void OnPointerExit(PointerEventData eventData){if(cardView.isOpponentHand)return;targetScale=originalScale;targetAnchoredPosition=originalAnchoredPosition;}
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gameManager != null && gameManager.turnManager != null)
        {
            if (!gameManager.turnManager.IsPlayerTurn()) return;
        }
        if (!interactable || gameManager == null || gameManager.dragLayer == null) return;
        dragging = true;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        if (rectTransform != null)
        {
            originalRotation = rectTransform.localRotation;
            if (float.IsNaN(originalRotation.x) || float.IsNaN(originalRotation.y) ||
            float.IsNaN(originalRotation.z) || float.IsNaN(originalRotation.w))
            {
                originalRotation = Quaternion.identity;
            }
        }
        transform.SetParent(gameManager.dragLayer, true);
        if (rectTransform != null) rectTransform.SetAsLastSibling();
        targetRotation = Quaternion.identity;
        targetScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.9f;
        }
    }
    public void OnDrag(PointerEventData eventData){if(!interactable||gameManager==null||gameManager.canvas==null)return;rectTransform.anchoredPosition+=eventData.delta/gameManager.canvas.scaleFactor;}
    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        var target = eventData.pointerCurrentRaycast.gameObject;
        if (target != null)
        {
            CardSlot slot = target.GetComponent<CardSlot>();
            if (slot != null && !slot.IsOccupied)
            {
                slot.PlaceCard(cardData, ownerId);
                Disable();
                GameEvents.OnCardPlayed?.Invoke(slot, cardData, ownerId);
                CardHandLayout layout = originalParent != null ? originalParent.GetComponent<CardHandLayout>() : null;
                if (layout != null) layout.Rebuild();
                if (ManagerGame.Instance != null && ManagerGame.Instance.turnManager != null)
                ManagerGame.Instance.turnManager.EndTurn();
                return;
            }
        }
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        targetRotation = originalRotation;
        targetScale = originalScale;
        targetAnchoredPosition = originalAnchoredPosition;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
    private void OnEnable(){GameEvents.OnHandVisibilityChanged += UpdateHandVisibility;}
    private void OnDisable(){GameEvents.OnHandVisibilityChanged -= UpdateHandVisibility;}
    private void UpdateHandVisibility(bool visible){if(!cardView.isOpponentHand)return;if(cardView.artImage!=null)cardView.artImage.enabled=visible;if(cardView.backImage!=null)cardView.backImage.enabled=!visible;}
    public void SetInteractable(bool value)
    {
        interactable = value;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = value;
            if (cardView != null && cardView.isOpponentHand)
            {
                canvasGroup.alpha = 1f;
            }
            else
            {
                canvasGroup.alpha = value ? 1f : 0.6f;
            }
        }
    }
}

