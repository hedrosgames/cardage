using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class UIMatchResult : MonoBehaviour
{
    public CanvasGroup panel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI scoreText;
    public Button rematchButton;
    public Button menuButton;
    public string mainMenuSceneName;
    void Awake()
    {
        HidePanel();
    }
    void OnEnable()
    {
        GameEvents.OnMatchFinished += OnMatchFinished;
        if (rematchButton != null)
        rematchButton.onClick.AddListener(OnRematchClicked);
        if (menuButton != null)
        menuButton.onClick.AddListener(OnMenuClicked);
    }
    void OnDisable()
    {
        GameEvents.OnMatchFinished -= OnMatchFinished;
        if (rematchButton != null)
        rematchButton.onClick.RemoveListener(OnRematchClicked);
        if (menuButton != null)
        menuButton.onClick.RemoveListener(OnMenuClicked);
    }
    void OnMatchFinished(MatchResult result, int playerScore, int opponentScore)
    {
        ShowPanel();
        if (resultText != null)
        {
            string key = "";
            if (result == MatchResult.PlayerWin)
            key = "VENCEU";
            else if (result == MatchResult.OpponentWin)
            key = "PERDEU";
            else
            key = "EMPATOU";
            if (ManagerLocalization.Instance != null)
            {
                resultText.text = ManagerLocalization.Instance.GetText(key);
            }
            else
            {
                resultText.text = key;
            }
        }
        if (scoreText != null)
        {
            string scoreFormat = "{0} - {1}";
            if (ManagerLocalization.Instance != null)
            {
                scoreFormat = ManagerLocalization.Instance.GetText("UI_SCORE_FORMAT");
            }
            scoreText.text = string.Format(scoreFormat, playerScore, opponentScore);
        }
    }
    void OnRematchClicked()
    {
        HidePanel();
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }
    void OnMenuClicked()
    {
        if (ManagerCardGameChallenge.Instance != null)
        {
            ManagerCardGameChallenge.Instance.ReturnToWorld();
        }
        else
        {
            string worldSceneName = "World";
            if (ManagerSceneTransition.Instance != null)
            {
                ManagerSceneTransition.Instance.LoadScene(worldSceneName);
            }
            else
            {
                SceneManager.LoadScene(worldSceneName);
            }
        }
    }
    void ShowPanel()
    {
        if (panel == null)
        return;
        panel.gameObject.SetActive(true);
        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }
    void HidePanel()
    {
        if (panel == null)
        return;
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }
}

