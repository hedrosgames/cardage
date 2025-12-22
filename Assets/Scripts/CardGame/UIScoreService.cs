using UnityEngine;
using TMPro;
public class UIScoreService : MonoBehaviour
{
    public static UIScoreService Instance { get; private set; }
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI opponentScoreText;
    static int playerScore = 5;
    static int opponentScore = 5;
    public static int PlayerScore => playerScore;
    public static int OpponentScore => opponentScore;
    void Awake()
    {
        Instance = this;
        RefreshUI();
    }
    public static void Reset()
    {
        playerScore = 5;
        opponentScore = 5;
        if (Instance != null)
        Instance.RefreshUI();
        GameEvents.OnScoreChanged?.Invoke(playerScore, opponentScore);
    }
    public static void AddPointPlayer()
    {
        playerScore++;
        opponentScore--;
        if (Instance != null)
        Instance.RefreshUI();
        GameEvents.OnScoreChanged?.Invoke(playerScore, opponentScore);
    }
    public static void AddPointOpponent()
    {
        opponentScore++;
        playerScore--;
        if (Instance != null)
        Instance.RefreshUI();
        GameEvents.OnScoreChanged?.Invoke(playerScore, opponentScore);
    }
    void RefreshUI()
    {
        if (playerScoreText != null)
        playerScoreText.text = playerScore.ToString();
        if (opponentScoreText != null)
        opponentScoreText.text = opponentScore.ToString();
    }
}

