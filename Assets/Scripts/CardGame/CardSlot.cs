using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class CardSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2Int gridPosition;
    public CardView currentCardView;
    public Image background;
    private Color baseColor;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.25f);
    public bool IsOccupied => currentCardView != null;
    void Awake()
    {
        if (background == null) background = GetComponent<Image>();
        if (background != null) baseColor = background.color;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsOccupied && background != null) background.color = highlightColor;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsOccupied && background != null) background.color = baseColor;
    }
    public void PlaceCard(SOCardData data, int ownerId)
    {
        if (ManagerGame.Instance == null || ManagerGame.Instance.boardCardPrefab == null) return;
        SOCardData cardInstance = GetCardInstance(data);
        GameObject obj = Instantiate(ManagerGame.Instance.boardCardPrefab, transform);
        currentCardView = obj.GetComponent<CardView>();
        if (currentCardView != null)
        {
            currentCardView.isHandCard = false;
            currentCardView.isOpponentHand = false;
            currentCardView.Setup(cardInstance, ownerId);
        }
        if (background != null) background.color = baseColor;
    }
    private SOCardData GetCardInstance(SOCardData originalCard)
    {
        if (CardInstanceManager.Instance != null)
        {
            return CardInstanceManager.Instance.GetCardInstance(originalCard);
        }
        return originalCard;
    }
}

