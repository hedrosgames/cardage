using UnityEngine;
using System.Collections;
public class ManagerTurn : MonoBehaviour
{
    public static ManagerTurn Instance { get; private set; }
    bool playerTurn;
    int turnCounter = 1;
    ManagerGame gameManager;
    AIBehaviorBase AI
    {
        get
        {
            if (gameManager == null || gameManager.setup == null || gameManager.setup.opponent == null)
            return null;
            return gameManager.setup.opponent.behavior;
        }
    }
    void Awake()
    {
        Instance = this;
        if (gameManager == null)
        gameManager = ManagerGame.Instance != null ? ManagerGame.Instance : FindFirstObjectByType<ManagerGame>();
    }
    public void StartGame()
    {
        StartCoroutine(DecideFirstTurn());
    }
    IEnumerator DecideFirstTurn()
    {
        yield return new WaitForSeconds(0.5f);
        playerTurn = Random.value > 0.5f;
        GameEvents.OnTurnChanged?.Invoke(turnCounter);
        GameEvents.OnTurnOwnerChanged?.Invoke(playerTurn);
        if (!playerTurn) StartCoroutine(OpponentPlay());
    }
    public void EndTurn()
    {
        if (ManagerGame.Instance != null)
        ManagerGame.Instance.SetOpponentHandInteractable(false);
        StartCoroutine(EndTurnSequence());
    }
    IEnumerator EndTurnSequence()
    {
        yield return new WaitForEndOfFrame();
        if (ManagerCaptureVisual.Instance != null)
        {
            while (ManagerCaptureVisual.Instance.IsBusy)
            {
                yield return null;
            }
        }
        yield return new WaitForSeconds(0.2f);
        playerTurn = !playerTurn;
        turnCounter++;
        GameEvents.OnTurnChanged?.Invoke(turnCounter);
        GameEvents.OnTurnOwnerChanged?.Invoke(playerTurn);
        if (IsBoardFull())
        {
            FinishMatch();
            yield break;
        }
        if (!playerTurn) StartCoroutine(OpponentPlay());
    }
    public bool IsPlayerTurn() { return playerTurn; }
    IEnumerator OpponentPlay()
    {
        yield return new WaitForSeconds(1f);
        if (gameManager == null || AI == null) yield break;
        var hand = gameManager.GetOpponentHand();
        var board = gameManager.GetBoard();
        CardButton chosenCard = AI.ChooseCard(hand);
        if (chosenCard == null)
        {
            if (gameManager != null) gameManager.CheckHandVisibility();
            gameManager.SetOpponentHandInteractable(true);
            yield break;
        }
        CardSlot chosenSlot = AI.ChooseSlot(board);
        if (chosenCard != null && chosenSlot != null)
        {
            chosenSlot.PlaceCard(chosenCard.GetCardData(), ManagerGame.ID_OPPONENT);
            chosenCard.Disable();
            GameEvents.OnCardPlayed?.Invoke(chosenSlot, chosenCard.GetCardData(), ManagerGame.ID_OPPONENT);
            if (IsBoardFull())
            {
                StartCoroutine(EndTurnSequence());
            }
            else
            {
                EndTurn();
            }
        }
    }
    bool IsBoardFull()
    {
        if (gameManager == null) return false;
        CardSlot[] slots = gameManager.GetBoard();
        if (slots == null || slots.Length == 0) return false;
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i] == null) continue;
            if (!slots[i].IsOccupied) return false;
        }
        return true;
    }
    void FinishMatch()
    {
        int player = UIScoreService.PlayerScore;
        int opponent = UIScoreService.OpponentScore;
        MatchResult result;
        if (player > opponent) result = MatchResult.PlayerWin;
        else if (player < opponent) result = MatchResult.OpponentWin;
        else result = MatchResult.Draw;
        GameEvents.OnMatchFinished?.Invoke(result, player, opponent);
    }
}

