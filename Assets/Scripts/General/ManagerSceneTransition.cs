using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
public class ManagerSceneTransition : MonoBehaviour
{
    public static ManagerSceneTransition Instance { get; private set; }
    [Header("ConfiguraÃ§Ã£o")]
    public GameObject transitionPrefab;
    public bool fadeOnGameStart = true;
    public float startDelay = 1f;
    private ScreenTransition currentTransitionInstance;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (fadeOnGameStart)
        {
            ScreenTransition tr = GetTransitionInstance();
            if (tr != null)
            {
                tr.SetInstantAlpha(1f);
                StartCoroutine(StartupSequence(tr));
            }
        }
    }
    IEnumerator StartupSequence(ScreenTransition tr)
    {
        yield return new WaitForSecondsRealtime(startDelay);
        if (tr != null)
        yield return StartCoroutine(tr.PlayOpenRoutine());
    }
    private ScreenTransition GetTransitionInstance()
    {
        if (currentTransitionInstance != null)
        return currentTransitionInstance;
        if (transitionPrefab == null)
        {
            return null;
        }
        GameObject obj = Instantiate(transitionPrefab, transform);
        obj.name = "ScreenTransition_Instance";
        currentTransitionInstance = obj.GetComponent<ScreenTransition>();
        return currentTransitionInstance;
    }
    public void PerformTransition(Action midAction)
    {
        StartCoroutine(TransitionOnlyRoutine(midAction));
    }
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName, LoadSceneMode.Single));
    }
    public void LoadSceneAdditive(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName, LoadSceneMode.Additive));
    }
    IEnumerator TransitionOnlyRoutine(Action midAction)
    {
        ScreenTransition transition = GetTransitionInstance();
        if (transition != null)
        yield return StartCoroutine(transition.PlayCloseRoutine());
        midAction?.Invoke();
        yield return null;
        if (transition != null)
        yield return StartCoroutine(transition.PlayOpenRoutine());
    }
    IEnumerator LoadRoutine(string sceneName, LoadSceneMode mode)
    {
        ScreenTransition transition = GetTransitionInstance();
        if (transition != null)
        yield return StartCoroutine(transition.PlayCloseRoutine());
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
        op.allowSceneActivation = false;
        while (!op.isDone)
        {
            if (op.progress >= 0.9f) op.allowSceneActivation = true;
            yield return null;
        }
        yield return null;
        if (mode == LoadSceneMode.Additive)
        {
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
            }
        }
        if (transition != null)
        yield return StartCoroutine(transition.PlayOpenRoutine());
    }
}

