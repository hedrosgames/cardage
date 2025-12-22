using UnityEngine;
using System.Collections.Generic;
public class ManagerGame : MonoBehaviour
{
    [Header("Configuração da Partida")]
    public SOGameSetup setup;
    public ManagerBoard boardManager;
    public ManagerTurn turnManager;
    public GameObject boardCardPrefab;
    [Header("UI & Interação")]
    public List<CardButton> playerHandButtons;
    public List<CardButton> opponentHandButtons;
    public Canvas canvas;
    public Transform dragLayer;
    [Header("Cores")]
    public Color playerColor = Color.blue;
    public Color opponentColor = Color.red;
    public static ManagerGame Instance { get; private set; }
    public SODeckData playerDeck { get; private set; }
    public SODeckData opponentDeck { get; private set; }
    public const int ID_PLAYER = 0;
    public const int ID_OPPONENT = 1;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (boardManager == null) boardManager = FindFirstObjectByType<ManagerBoard>();
        if (turnManager == null) turnManager = FindFirstObjectByType<ManagerTurn>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        CardInstanceManager.EnsureExists();
    }
    private void OnEnable()
    {
        GameEvents.OnTurnOwnerChanged += HandleTurnChanged;
    }
    private void OnDisable()
    {
        GameEvents.OnTurnOwnerChanged -= HandleTurnChanged;
    }
    private void Start()
    {
        if (setup == null)
        {
            var challengeManager = Object.FindFirstObjectByType<ManagerCardGameChallenge>();
            if (challengeManager != null && challengeManager.pendingGameSetup != null)
            {
                setup = challengeManager.pendingGameSetup;
                challengeManager.ClearGameSetup();
            }
        }
        if (setup == null || setup.opponent == null) return;
        playerDeck = setup.playerDeck;
        opponentDeck = setup.opponent.deck;
        if (CardInstanceManager.Instance != null)
        {
            CardInstanceManager.Instance.PreCloneDecks(playerDeck, opponentDeck);
        }
        if (ManagerLocalization.Instance != null) {
            GameEvents.OnPlayerNameChanged?.Invoke(ManagerLocalization.Instance.GetText("PLAYER_NAME"));
            GameEvents.OnOpponentNameChanged?.Invoke(ManagerLocalization.Instance.GetText(setup.opponent.opponentNameKey));
        }
        UIScoreService.Reset();
        InitHands();
        CheckHandVisibility();
        if (turnManager != null) turnManager.StartGame();
    }
    void HandleTurnChanged(bool isPlayerTurn)
    {
        SetPlayerHandInteractable(isPlayerTurn);
    }
    public void SetPlayerHandInteractable(bool interactable)
    {
        foreach (var btn in playerHandButtons)
        {
            if (btn != null && btn.gameObject.activeSelf)
            {
                btn.SetInteractable(interactable);
            }
        }
    }
    public void CheckHandVisibility()
    {
        bool show = false;
        if (ManagerCapture.Instance != null) {
            foreach (var rule in ManagerCapture.Instance.activeRules) {
                if (rule != null && rule is RuleOpen) {
                    show = true; break;
                }
            }
        }
        if (!show && setup != null && setup.opponent != null && setup.opponent.behavior != null) {
            if (setup.opponent.behavior is AI_Manual) show = true;
        }
        GameEvents.OnHandVisibilityChanged?.Invoke(show);
    }
    private void InitHands()
    {
        int playerCount = Mathf.Min(playerHandButtons.Count, playerDeck.cards.Length);
        for (int i = 0; i < playerCount; i++) {
            SOCardData cardInstance = GetCardInstance(playerDeck.cards[i]);
            playerHandButtons[i].cardView.isOpponentHand = false;
            playerHandButtons[i].cardView.isHandCard = true;
            playerHandButtons[i].Setup(cardInstance, this, ID_PLAYER, true);
        }
        int opponentCount = Mathf.Min(opponentHandButtons.Count, opponentDeck.cards.Length);
        for (int i = 0; i < opponentCount; i++) {
            SOCardData cardInstance = GetCardInstance(opponentDeck.cards[i]);
            opponentHandButtons[i].cardView.isOpponentHand = true;
            opponentHandButtons[i].cardView.isHandCard = true;
            opponentHandButtons[i].Setup(cardInstance, this, ID_OPPONENT, false);
        }
        RebuildPlayerHandLayout();
    }
    public Color GetColorById(int id)
    {
        if (id == ID_PLAYER) return playerColor;
        if (id == ID_OPPONENT) return opponentColor;
        return Color.white;
    }
    private void RebuildPlayerHandLayout()
    {
        Transform root = null;
        for(int i=0; i<playerHandButtons.Count; i++) {
            if(playerHandButtons[i]!=null) { root = playerHandButtons[i].transform.parent; break; }
        }
        if(root == null) return;
        CardHandLayout layout = root.GetComponent<CardHandLayout>();
        if(layout != null) layout.Rebuild();
    }
    public List<CardButton> GetOpponentHand() => opponentHandButtons;
    public CardSlot[] GetBoard() => boardManager != null ? boardManager.GetAllSlots() : new CardSlot[0];
    private SOCardData GetCardInstance(SOCardData originalCard)
    {
        if (CardInstanceManager.Instance != null)
        {
            return CardInstanceManager.Instance.GetCardInstance(originalCard);
        }
        return originalCard;
    }
    public void SetOpponentHandInteractable(bool interactable)
    {
        foreach (var btn in opponentHandButtons)
        {
            if (btn != null && btn.gameObject.activeSelf)
            {
                btn.SetInteractable(interactable);
            }
        }
    }
}

