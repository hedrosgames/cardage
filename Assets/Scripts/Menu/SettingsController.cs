using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
public class SettingsController : MonoBehaviour
{
    [Header("Language UI")]
    public TextMeshProUGUI languageDisplay;
    public Button btnLanguageNext;
    public Button btnLanguagePrev;
    [Header("Resolution UI")]
    public TextMeshProUGUI resolutionDisplay;
    public Button btnResNext;
    public Button btnResPrev;
    [Header("Audio UI")]
    public Slider sliderMusic;
    public TextMeshProUGUI musicPercentText;
    public Slider sliderSFX;
    public TextMeshProUGUI sfxPercentText;
    [Header("Navigation")]
    public Button btnBack;
    private List<string> availableLanguages = new List<string> { "EN", "PT" };
    private int currentLangIndex = 0;
    private Resolution[] filteredResolutions;
    private int currentResIndex = 0;
    private readonly Vector2Int[] allowedResolutions = new Vector2Int[]
    {
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(1280, 800),
        new Vector2Int(1920, 1200),
        new Vector2Int(2560, 1080)
    };
    void Start()
    {
        if (ManagerLocalization.Instance != null && ManagerLocalization.Instance.settingsClient != null)
        {
            InitializeAll();
        }
        else
        {
            InitializeAll();
        }
        SetupListeners();
    }
    void InitializeAll()
    {
        InitializeLanguage();
        InitializeResolution();
        InitializeAudio();
    }
    void SetupListeners()
    {
        if (btnLanguageNext != null) btnLanguageNext.onClick.AddListener(OnNextLanguage);
        if (btnLanguagePrev != null) btnLanguagePrev.onClick.AddListener(OnPrevLanguage);
        if (btnResNext != null) btnResNext.onClick.AddListener(OnNextResolution);
        if (btnResPrev != null) btnResPrev.onClick.AddListener(OnPrevResolution);
        if (sliderMusic != null) sliderMusic.onValueChanged.AddListener(OnMusicChanged);
        if (sliderSFX != null) sliderSFX.onValueChanged.AddListener(OnSFXChanged);
        if (btnBack != null) btnBack.onClick.AddListener(OnBackClicked);
    }
    void OnDestroy()
    {
        if (btnLanguageNext != null) btnLanguageNext.onClick.RemoveListener(OnNextLanguage);
        if (btnLanguagePrev != null) btnLanguagePrev.onClick.RemoveListener(OnPrevLanguage);
        if (btnResNext != null) btnResNext.onClick.RemoveListener(OnNextResolution);
        if (btnResPrev != null) btnResPrev.onClick.RemoveListener(OnPrevResolution);
        if (sliderMusic != null) sliderMusic.onValueChanged.RemoveListener(OnMusicChanged);
        if (sliderSFX != null) sliderSFX.onValueChanged.RemoveListener(OnSFXChanged);
        if (btnBack != null) btnBack.onClick.RemoveListener(OnBackClicked);
    }
    void OnBackClicked()
    {
        SaveEvents.RaiseSave();
        if (ManagerMainMenu.Instance != null)
        ManagerMainMenu.Instance.BackFromSettings();
        else
        {
            var menu = FindFirstObjectByType<ManagerMainMenu>();
            if (menu != null) menu.BackFromSettings();
        }
    }
    void InitializeAudio()
    {
        if (ManagerLocalization.Instance == null || ManagerLocalization.Instance.settingsClient == null) return;
        float mus = ManagerLocalization.Instance.settingsClient.musicVolume;
        float sfx = ManagerLocalization.Instance.settingsClient.sfxVolume;
        if (sliderMusic != null)
        {
            sliderMusic.SetValueWithoutNotify(mus);
            UpdateAudioText(musicPercentText, mus);
        }
        if (sliderSFX != null)
        {
            sliderSFX.SetValueWithoutNotify(sfx);
            UpdateAudioText(sfxPercentText, sfx);
        }
    }
    void OnMusicChanged(float value)
    {
        UpdateAudioText(musicPercentText, value);
        if (ManagerAudio.Instance != null)
        ManagerAudio.Instance.SetMusicVolume(value);
        if (ManagerLocalization.Instance != null && ManagerLocalization.Instance.settingsClient != null)
        {
            ManagerLocalization.Instance.settingsClient.musicVolume = value;
        }
    }
    void OnSFXChanged(float value)
    {
        UpdateAudioText(sfxPercentText, value);
        if (ManagerAudio.Instance != null)
        ManagerAudio.Instance.SetSFXVolume(value);
        if (ManagerLocalization.Instance != null && ManagerLocalization.Instance.settingsClient != null)
        {
            ManagerLocalization.Instance.settingsClient.sfxVolume = value;
        }
    }
    void UpdateAudioText(TextMeshProUGUI label, float value)
    {
        if (label != null) label.text = Mathf.RoundToInt(value * 100f) + "%";
    }
    void InitializeLanguage()
    {
        if (ManagerLocalization.Instance == null || ManagerLocalization.Instance.settingsClient == null) return;
        string current = ManagerLocalization.Instance.settingsClient.language;
        currentLangIndex = availableLanguages.IndexOf(current.ToUpper());
        if (currentLangIndex < 0) currentLangIndex = 0;
        UpdateLanguageDisplay();
    }
    void OnNextLanguage()
    {
        currentLangIndex++;
        if (currentLangIndex >= availableLanguages.Count) currentLangIndex = 0;
        ApplyLanguage();
    }
    void OnPrevLanguage()
    {
        currentLangIndex--;
        if (currentLangIndex < 0) currentLangIndex = availableLanguages.Count - 1;
        ApplyLanguage();
    }
    void ApplyLanguage()
    {
        string selectedLang = availableLanguages[currentLangIndex];
        UpdateLanguageDisplay();
        if (ManagerLocalization.Instance != null)
        {
            ManagerLocalization.Instance.SetLanguage(selectedLang);
            if (ManagerLocalization.Instance.settingsClient != null)
            {
                ManagerLocalization.Instance.settingsClient.language = selectedLang;
                SaveEvents.RaiseSave();
            }
        }
    }
    void UpdateLanguageDisplay()
    {
        if (languageDisplay != null) languageDisplay.text = availableLanguages[currentLangIndex];
    }
    void InitializeResolution()
    {
        Resolution[] all = Screen.resolutions;
        List<Resolution> validList = new List<Resolution>();
        HashSet<string> uniqueRes = new HashSet<string>();
        foreach (var res in all)
        {
            bool isAllowed = false;
            foreach (var allowed in allowedResolutions)
            {
                if (res.width == allowed.x && res.height == allowed.y) { isAllowed = true; break; }
            }
            if (isAllowed)
            {
                string key = $"{res.width}x{res.height}";
                if (!uniqueRes.Contains(key)) { validList.Add(res); uniqueRes.Add(key); }
            }
        }
        filteredResolutions = validList.OrderBy(x => x.width).ThenBy(x => x.height).ToArray();
        if (filteredResolutions.Length == 0) filteredResolutions = new Resolution[] { Screen.currentResolution };
        string savedRes = "";
        if (ManagerLocalization.Instance != null && ManagerLocalization.Instance.settingsClient != null)
        savedRes = ManagerLocalization.Instance.settingsClient.resolution;
        currentResIndex = -1;
        if (!string.IsNullOrEmpty(savedRes))
        {
            for (int i = 0; i < filteredResolutions.Length; i++)
            {
                if (ResToString(filteredResolutions[i]) == savedRes) { currentResIndex = i; break; }
            }
        }
        if (currentResIndex == -1) currentResIndex = filteredResolutions.Length - 1;
        UpdateResolutionDisplay();
    }
    void OnNextResolution()
    {
        currentResIndex++;
        if (currentResIndex >= filteredResolutions.Length) currentResIndex = 0;
        ApplyResolution();
    }
    void OnPrevResolution()
    {
        currentResIndex--;
        if (currentResIndex < 0) currentResIndex = filteredResolutions.Length - 1;
        ApplyResolution();
    }
    void ApplyResolution()
    {
        if (filteredResolutions == null || filteredResolutions.Length == 0) return;
        Resolution r = filteredResolutions[currentResIndex];
        UpdateResolutionDisplay();
        Screen.SetResolution(r.width, r.height, true);
        UpdateEnforcers((float)r.width / r.height);
        if (ManagerLocalization.Instance != null && ManagerLocalization.Instance.settingsClient != null)
        {
            ManagerLocalization.Instance.settingsClient.resolution = ResToString(r);
            SaveEvents.RaiseSave();
        }
    }
    void UpdateEnforcers(float aspect)
    {
        var enforcers = FindObjectsByType<CameraAspectEnforcer>(FindObjectsSortMode.None);
        foreach (var enf in enforcers) enf.SetAspectRatio(aspect);
    }
    void UpdateResolutionDisplay()
    {
        if (resolutionDisplay != null && filteredResolutions.Length > 0)
        {
            Resolution r = filteredResolutions[currentResIndex];
            resolutionDisplay.text = $"{r.width} x {r.height}";
        }
    }
    string ResToString(Resolution r) => $"{r.width}x{r.height}";
}

