using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CardView : MonoBehaviour
{
    public SOCardVisual visualConfig;
    public SOCardData cardData;
    [Header("UI References")]
    public Image background;
    public Image artImage;
    public Image rarityImage;
    public Image diamondImage;
    public TMP_Text topText;
    public TMP_Text rightText;
    public TMP_Text bottomText;
    public TMP_Text leftText;
    public Image backImage;
    public bool isHandCard = false;
    public bool isOpponentHand = false;
    [Header("Icons")]
    public Image elementIcon;
    public Image collectionIcon;
    public Image specialIcon;
    public Image otherIcon;
    public Image ornamentElement;
    public Image ornamentCollection;
    public Image ornamentSpecial;
    public Image ornamentOther;
    [HideInInspector] public int ownerId;
    public bool isPlayerOwner => (ownerId == ManagerGame.ID_PLAYER);
    public CardHandLayout handLayout;
    private void Awake()
    {
        if (handLayout == null) handLayout = GetComponentInParent<CardHandLayout>();
    }
    private void OnEnable()
    {
        GameEvents.OnHandVisibilityChanged += ApplyVisibility;
    }
    private void OnDisable()
    {
        GameEvents.OnHandVisibilityChanged -= ApplyVisibility;
    }
    private void ApplyVisibility(bool visible)
    {
        if (!isOpponentHand || !isHandCard) return;
        if (artImage != null) artImage.enabled = visible;
        if (backImage != null) backImage.gameObject.SetActive(!visible);
    }
    public void Setup(SOCardData data, int id)
    {
        cardData = data;
        SetOwnerId(id, true);
        if (visualConfig != null)
        {
            Sprite chosenArt = cardData.customArt != null ? cardData.customArt : visualConfig.art;
            if (artImage != null)
            {
                artImage.sprite = chosenArt;
                artImage.enabled = true;
            }
            if (rarityImage != null)
            {
                rarityImage.sprite = visualConfig.rarityBaseSprite;
                rarityImage.color = visualConfig.GetRarityColor(data.rarity);
            }
            if (diamondImage != null)
            {
                diamondImage.sprite = isHandCard ? visualConfig.diamondHand : visualConfig.diamondBoard;
                diamondImage.color = Color.white;
            }
            HandleIconAndOrnament(true, elementIcon, ornamentElement, visualConfig.GetTypeSprite(cardData.type));
            HandleIconAndOrnament(true, collectionIcon, ornamentCollection, visualConfig.GetCollectionSprite(cardData.collection));
            HandleIconAndOrnament(cardData.special != SpecialType.None, specialIcon, ornamentSpecial, visualConfig.GetSpecialSprite(cardData.special));
            HandleIconAndOrnament(true, otherIcon, ornamentOther, visualConfig.GetTriadSprite(cardData.triad));
        }
        if (topText != null) topText.text = cardData.top.ToString();
        if (rightText != null) rightText.text = cardData.right.ToString();
        if (bottomText != null) bottomText.text = cardData.bottom.ToString();
        if (leftText != null) leftText.text = cardData.left.ToString();
        SetupInitialVisibility();
    }
    public void SetOwnerId(int id, bool updateVisuals = false)
    {
        ownerId = id;
        if (updateVisuals)
        {
            if (visualConfig != null)
            {
                Color visualColor = isPlayerOwner ? visualConfig.playerColor : visualConfig.opponentColor;
                if (background != null) background.color = visualColor;
            }
            else if (ManagerGame.Instance != null)
            {
                if (background != null) background.color = ManagerGame.Instance.GetColorById(id);
            }
            if (diamondImage != null)
            {
                diamondImage.gameObject.SetActive(true);
                diamondImage.color = Color.white;
            }
        }
    }
    private void HandleIconAndOrnament(bool hasValue, Image icon, Image ornament, Sprite sprite)
    {
        if (!hasValue) {
            if (icon != null) icon.gameObject.SetActive(false);
            if (ornament != null) ornament.gameObject.SetActive(false);
            return;
        }
        if (icon != null) {
            icon.gameObject.SetActive(true);
            icon.sprite = sprite;
            icon.color = Color.white;
        }
        if (ornament != null) {
            ornament.gameObject.SetActive(true);
            if (visualConfig != null) {
                ornament.sprite = visualConfig.ornamentBaseSprite;
                ornament.color = visualConfig.ornamentColor;
            }
        }
    }
    private void SetupInitialVisibility() {
        if (!isHandCard) {
            if (artImage != null) artImage.enabled = true;
            if (backImage != null) backImage.gameObject.SetActive(false);
            return;
        }
        if (isOpponentHand) {
            if (artImage != null) artImage.enabled = false;
            if (backImage != null) backImage.gameObject.SetActive(true);
        } else {
            if (artImage != null) artImage.enabled = true;
            if (backImage != null) backImage.gameObject.SetActive(false);
        }
    }
    public void Reveal() {
        if (backImage != null) backImage.gameObject.SetActive(false);
        if (artImage != null) artImage.enabled = true;
    }
}

