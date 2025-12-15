using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    public TMP_Text turnTxt;
    public Image turnIndicator;
    public TMP_Text playerNameTxt;
    public TMP_Text opponentNameTxt;
    public TMP_Text playerScoreTxt;
    public TMP_Text opponentScoreTxt;
    public Color playerColor;
    public Color opponentColor;
    private void OnEnable()
    {
        GameEvents.OnTurnChanged += UpdateTurnText;
        GameEvents.OnTurnOwnerChanged += UpdateTurnOwner;
        GameEvents.OnScoreChanged += UpdateScore;
        GameEvents.OnPlayerNameChanged += SetPlayerName;
        GameEvents.OnOpponentNameChanged += SetOpponentName;
        ManagerLocalization.OnLanguageChanged += RefreshTurnText;
    }
    private void OnDisable()
    {
        GameEvents.OnTurnChanged -= UpdateTurnText;
        GameEvents.OnTurnOwnerChanged -= UpdateTurnOwner;
        GameEvents.OnScoreChanged -= UpdateScore;
        GameEvents.OnPlayerNameChanged -= SetPlayerName;
        GameEvents.OnOpponentNameChanged -= SetOpponentName;
        ManagerLocalization.OnLanguageChanged -= RefreshTurnText;
    }
    private void UpdateTurnText(int turn)
    {
        if (turnIndicator != null)
        turnIndicator.gameObject.SetActive(true);
        if (turnTxt != null)
        {
            string turnPrefix = "TURN ";
            if (ManagerLocalization.Instance != null)
            {
                turnPrefix = ManagerLocalization.Instance.GetText("UI_TURN_PREFIX") + " ";
            }
            turnTxt.text = turnPrefix + turn;
        }
        if (turnIndicator != null)
        {
            turnIndicator.transform.SetAsLastSibling();
            turnIndicator.canvasRenderer.SetAlpha(1f);
            Color c = turnIndicator.color;
            c.a = 1f;
            turnIndicator.color = c;
        }
    }
    private void RefreshTurnText()
    {
        UpdateTurnText(1);
    }
    private void UpdateTurnOwner(bool isPlayerTurn)
    {
        if (turnIndicator == null)
        return;
        Color c = isPlayerTurn ? playerColor : opponentColor;
        c.a = 1f;
        turnIndicator.color = c;
    }
    private void UpdateScore(int player, int opponent)
    {
        if (playerScoreTxt != null)
        playerScoreTxt.text = player.ToString();
        if (opponentScoreTxt != null)
        opponentScoreTxt.text = opponent.ToString();
    }
    private void SetPlayerName(string name)
    {
        if (playerNameTxt != null)
        playerNameTxt.text = name;
    }
    private void SetOpponentName(string name)
    {
        if (opponentNameTxt != null)
        opponentNameTxt.text = name;
    }
}

