using UnityEngine;
using UnityEngine.SceneManagement;
public class ManagerCardGameChallenge : MonoBehaviour
{
    public static ManagerCardGameChallenge Instance { get; private set; }
    [Header("ConfiguraÃ§Ã£o Atual")]
    public SOGameSetup pendingGameSetup;
    private string worldSceneName = "World";
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnEnable()
    {
        GameEvents.OnMatchFinished += OnMatchFinished;
    }
    void OnDisable()
    {
        GameEvents.OnMatchFinished -= OnMatchFinished;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.OnMatchFinished -= OnMatchFinished;
    }
    void OnMatchFinished(MatchResult result, int playerScore, int opponentScore)
    {
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
    public void SetGameSetup(SOGameSetup setup)
    {
        pendingGameSetup = setup;
    }
    public void ClearGameSetup()
    {
        pendingGameSetup = null;
    }
    public void ReturnToWorld()
    {
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

