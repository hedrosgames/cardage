using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class ManagerNewGame : MonoBehaviour
{
    public static ManagerNewGame Instance { get; private set; }
    [Header("PainÃ©is")]
    public RectTransform pnlStartGame;
    public RectTransform pnlCutscene;
    [Header("CustomizaÃ§Ã£o - Start Game")]
    public Button btnMale;
    public Button btnFemale;
    public Button btnColor1;
    public Button btnColor2;
    public Button btnColor3;
    public Button btnColor4;
    public TMP_InputField inputPlayerName;
    public Button btnConfirm;
    [Header("Cutscene")]
    public Image frame1;
    public Image frame2;
    public Image frame3;
    [Header("Config")]
    public float fadeDuration = 0.5f;
    public float frameDisplayDuration = 2f;
    public float delayAfterLastFrame = 1f;
    public string worldSceneName = "World";
    [Header("Dados")]
    public SOPlayerData playerData;
    private PlayerGender selectedGender = PlayerGender.Male;
    private Color selectedColor = Color.white;
    private Color[] colorOptions = new Color[]
    {
        Color.white,
        new Color(1f, 0.8f, 0.6f),
        new Color(0.8f, 0.6f, 0.4f),
        new Color(0.6f, 0.4f, 0.2f)
    };
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        InitializeUI();
    }
    void InitializeUI()
    {
        if (btnMale != null) btnMale.onClick.AddListener(() => SetGender(PlayerGender.Male));
        if (btnFemale != null) btnFemale.onClick.AddListener(() => SetGender(PlayerGender.Female));
        if (btnColor1 != null) btnColor1.onClick.AddListener(() => SetColor(0));
        if (btnColor2 != null) btnColor2.onClick.AddListener(() => SetColor(1));
        if (btnColor3 != null) btnColor3.onClick.AddListener(() => SetColor(2));
        if (btnColor4 != null) btnColor4.onClick.AddListener(() => SetColor(3));
        if (btnConfirm != null) btnConfirm.onClick.AddListener(OnConfirmClicked);
        selectedGender = PlayerGender.Male;
        selectedColor = colorOptions[0];
        UpdateGenderButtons();
        UpdateColorButtons();
        if (pnlStartGame != null)
        {
            pnlStartGame.gameObject.SetActive(true);
            CanvasGroup cg = GetOrAddCanvasGroup(pnlStartGame);
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
        if (pnlCutscene != null)
        {
            pnlCutscene.gameObject.SetActive(false);
            CanvasGroup cg = GetOrAddCanvasGroup(pnlCutscene);
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
        if (frame1 != null) SetImageAlpha(frame1, 0f);
        if (frame2 != null) SetImageAlpha(frame2, 0f);
        if (frame3 != null) SetImageAlpha(frame3, 0f);
    }
    void SetGender(PlayerGender gender)
    {
        selectedGender = gender;
        UpdateGenderButtons();
    }
    void UpdateGenderButtons()
    {
        if (btnMale != null)
        {
            var colors = btnMale.colors;
            colors.normalColor = selectedGender == PlayerGender.Male ? Color.green : Color.white;
            btnMale.colors = colors;
        }
        if (btnFemale != null)
        {
            var colors = btnFemale.colors;
            colors.normalColor = selectedGender == PlayerGender.Female ? Color.green : Color.white;
            btnFemale.colors = colors;
        }
    }
    void SetColor(int index)
    {
        if (index >= 0 && index < colorOptions.Length)
        {
            selectedColor = colorOptions[index];
            UpdateColorButtons();
        }
    }
    void UpdateColorButtons()
    {
        Button[] colorButtons = { btnColor1, btnColor2, btnColor3, btnColor4 };
        for (int i = 0; i < colorButtons.Length && i < colorOptions.Length; i++)
        {
            if (colorButtons[i] != null)
            {
                var colors = colorButtons[i].colors;
                colors.normalColor = (selectedColor == colorOptions[i]) ? Color.green : colorOptions[i];
                colorButtons[i].colors = colors;
            }
        }
    }
    void OnConfirmClicked()
    {
        if (playerData != null)
        {
            playerData.playerName = inputPlayerName != null && !string.IsNullOrEmpty(inputPlayerName.text)
            ? inputPlayerName.text
            : "Player";
            playerData.gender = selectedGender;
            playerData.characterColor = selectedColor;
        }
        if (ManagerMainMenu.Instance != null && ManagerMainMenu.Instance.saveMenu != null)
        {
            ManagerMainMenu.Instance.saveMenu.InitializeNewGame();
        }
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.SaveAll();
        }
        StartCoroutine(TransitionToCutscene());
    }
    IEnumerator TransitionToCutscene()
    {
        if (pnlStartGame != null)
        {
            CanvasGroup cg = GetOrAddCanvasGroup(pnlStartGame);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeDuration;
                cg.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
            pnlStartGame.gameObject.SetActive(false);
        }
        if (pnlCutscene != null)
        {
            pnlCutscene.gameObject.SetActive(true);
            CanvasGroup cg = GetOrAddCanvasGroup(pnlCutscene);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeDuration;
                cg.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            cg.alpha = 1f;
        }
        yield return StartCoroutine(PlayCutsceneSequence());
    }
    IEnumerator PlayCutsceneSequence()
    {
        if (frame1 != null)
        {
            yield return StartCoroutine(FadeInImage(frame1));
            yield return new WaitForSeconds(frameDisplayDuration);
        }
        if (frame2 != null)
        {
            yield return StartCoroutine(FadeInImage(frame2));
            yield return new WaitForSeconds(frameDisplayDuration);
        }
        if (frame3 != null)
        {
            yield return StartCoroutine(FadeInImage(frame3));
            yield return new WaitForSeconds(frameDisplayDuration);
        }
        yield return new WaitForSeconds(delayAfterLastFrame);
        LoadWorldScene();
    }
    IEnumerator FadeInImage(Image img)
    {
        if (img == null) yield break;
        float t = 0f;
        Color startColor = img.color;
        startColor.a = 0f;
        Color endColor = img.color;
        endColor.a = 1f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            img.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        img.color = endColor;
    }
    void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
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
            UnityEngine.SceneManagement.SceneManager.LoadScene(worldSceneName);
        }
    }
    CanvasGroup GetOrAddCanvasGroup(RectTransform panel)
    {
        if (panel == null) return null;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }
}

