using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
public class ManagerMainMenu : MonoBehaviour
{
    public static ManagerMainMenu Instance { get; private set; }
    [Header("UI Panels")]
    public RectTransform panelMainMenu;
    public RectTransform panelSettings;
    public RectTransform panelAchievements;
    public RectTransform panelQuitConfirm;
    [Header("Canvases (Referências para Editor)")]
    public GameObject canvasMainMenu;
    public GameObject canvasSettings;
    public GameObject canvasAchievements;
    public GameObject canvasQuit;
    [Header("Config")]
    public float transitionDuration = 0.35f;
    public string worldSceneName = "World";
    public SaveClientMenu saveMenu;
    Vector2 centerPos, leftPos, rightPos, upPos, downPos;
    bool isTransitioning;
    bool CheckSaveFileExists()
    {
        return ManagerSave.Instance != null && File.Exists(Path.Combine(Application.persistentDataPath, "save.json"));
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if(canvasMainMenu) canvasMainMenu.SetActive(true);
        if(canvasSettings) canvasSettings.SetActive(true);
        if(canvasAchievements) canvasAchievements.SetActive(true);
        if(canvasQuit) canvasQuit.SetActive(true);
        RectTransform parent = panelMainMenu.parent as RectTransform;
        float w = parent.rect.width;
        float h = parent.rect.height;
        centerPos = Vector2.zero;
        leftPos = new Vector2(-w, 0f);
        rightPos = new Vector2(w, 0f);
        upPos = new Vector2(0f, h);
        downPos = new Vector2(0f, -h);
        SetupPanel(panelMainMenu, centerPos, true);
        SetupPanel(panelSettings, rightPos, false);
        SetupPanel(panelAchievements, leftPos, false);
        SetupPanel(panelQuitConfirm, downPos, false);
    }
    void Start()
    {
        if (saveMenu != null)
        {
            saveMenu.SetSaveState(CheckSaveFileExists());
        }
    }
    public void OnPlayButtonClicked()
    {
        if (isTransitioning) return;
        bool hasSave = saveMenu != null && saveMenu.hasSaveFile;
        if (hasSave)
        {
            LoadWorldScene();
        }
        else
        {
            if (saveMenu != null)
            {
                saveMenu.InitializeNewGame();
            }
            ManagerSave.Instance.SaveAll();
            LoadWorldScene();
        }
    }
    void LoadWorldScene()
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
    void SetupPanel(RectTransform panel, Vector2 startPos, bool active)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(active);
        panel.anchoredPosition = startPos;
        CanvasGroup cg = GetOrAddCanvasGroup(panel);
        cg.alpha = active ? 1f : 0f;
        cg.blocksRaycasts = active;
        cg.interactable = active;
    }
    CanvasGroup GetOrAddCanvasGroup(RectTransform panel)
    {
        if (panel == null) return null;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }
    public void OpenSettings() { if(!isTransitioning) StartCoroutine(TransitionPanels(panelMainMenu, centerPos, leftPos, panelSettings, rightPos, centerPos)); }
    public void BackFromSettings() { if(!isTransitioning) StartCoroutine(BackSettingsRoutine()); }
    IEnumerator BackSettingsRoutine() { yield return TransitionPanelsRoutine(panelSettings, centerPos, rightPos, panelMainMenu, leftPos, centerPos); ManagerSave.Instance.SaveAll(); }
    public void OpenAchievements() { if(!isTransitioning) StartCoroutine(TransitionPanels(panelMainMenu, centerPos, rightPos, panelAchievements, leftPos, centerPos)); }
    public void BackFromAchievements() { if(!isTransitioning) StartCoroutine(BackAchievementsRoutine()); }
    IEnumerator BackAchievementsRoutine() { yield return TransitionPanelsRoutine(panelAchievements, centerPos, leftPos, panelMainMenu, rightPos, centerPos); ManagerSave.Instance.SaveAll(); }
    public void OpenQuitConfirm() { if(!isTransitioning) StartCoroutine(TransitionPanels(panelMainMenu, centerPos, upPos, panelQuitConfirm, downPos, centerPos)); }
    public void CancelQuit()
    {
        if(!isTransitioning)
        {
            ManagerQuitLogic quitLogic = FindFirstObjectByType<ManagerQuitLogic>();
            if (quitLogic != null)
            {
                quitLogic.ResetState();
            }
            StartCoroutine(TransitionPanels(panelQuitConfirm, centerPos, downPos, panelMainMenu, upPos, centerPos));
        }
    }
    public void QuitGame() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    IEnumerator TransitionPanels(RectTransform outPanel, Vector2 outStart, Vector2 outEnd, RectTransform inPanel, Vector2 inStart, Vector2 inEnd)
    {
        isTransitioning = true;
        yield return TransitionPanelsRoutine(outPanel, outStart, outEnd, inPanel, inStart, inEnd);
        isTransitioning = false;
    }
    IEnumerator TransitionPanelsRoutine(RectTransform outPanel, Vector2 outStart, Vector2 outEnd, RectTransform inPanel, Vector2 inStart, Vector2 inEnd)
    {
        CanvasGroup outCg = GetOrAddCanvasGroup(outPanel);
        CanvasGroup inCg = GetOrAddCanvasGroup(inPanel);
        if (outPanel != null) { outPanel.gameObject.SetActive(true); outPanel.anchoredPosition = outStart; if (outCg != null) { outCg.alpha = 1f; outCg.blocksRaycasts = false; outCg.interactable = false; } }
        if (inPanel != null) { inPanel.gameObject.SetActive(true); inPanel.anchoredPosition = inStart; if (inCg != null) { inCg.alpha = 0f; inCg.blocksRaycasts = false; inCg.interactable = false; } }
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(transitionDuration, 0.0001f);
            float n = Mathf.Clamp01(t);
            float posT = EaseOutBack(n);
            if (outPanel != null) { outPanel.anchoredPosition = Vector2.LerpUnclamped(outStart, outEnd, posT); if (outCg != null) outCg.alpha = Mathf.Lerp(1f, 0f, n); }
            if (inPanel != null) { inPanel.anchoredPosition = Vector2.LerpUnclamped(inStart, inEnd, posT); if (inCg != null) inCg.alpha = Mathf.Lerp(0f, 1f, n); }
            yield return null;
        }
        if (outPanel != null) { outPanel.gameObject.SetActive(false); outPanel.anchoredPosition = outStart; if (outCg != null) { outCg.alpha = 0f; outCg.blocksRaycasts = false; outCg.interactable = false; } }
        if (inPanel != null) { inPanel.anchoredPosition = inEnd; if (inCg != null) { inCg.alpha = 1f; inCg.blocksRaycasts = true; inCg.interactable = true; } }
    }
    float EaseOutBack(float t) { float c1 = 1.70158f; float c3 = c1 + 1f; float x = t - 1f; return 1f + c3 * x * x * x + c1 * x * x; }
}

