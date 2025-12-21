using UnityEngine;
using Game.World;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class GameSetupEvent : UnityEvent<SOGameSetup> { }

public class ManagerGameFlow : MonoBehaviour
{
    [Header("Sistemas")]
    public ManagerDialogue dialogueManager;
    public PlayerControl playerControl;
    public PlayerCutsceneAnimator playerCutscene;
    [Header("Card Game")]
    [Tooltip("Setup padrão do card game (opcional, pode ser definido via parâmetro na função)")]
    public SOGameSetup defaultCardGameSetup;
    [Tooltip("Evento que pode ser chamado via Unity Events passando um SOGameSetup como parâmetro")]
    public GameSetupEvent onStartCardGame;
    [Header("Zone 1 - Setup")]
    public SODialogueSequence introDialogue;
    public SOTutorial tutorialFirstMovement;
    [Header("Zone 1 - Objectives")]
    public GameObject fakeDoorObject;
    public SOZoneFlag flagUnlockDoor;
    [Header("Intro Dialogue Control")]
    public SOZoneFlag flagIntroDone;
    [Header("City Name Display")]
    public CanvasGroup cityNameCanvasGroup;
    public TextMeshProUGUI cityNameText;
    [HideInInspector]
    public float cityNameDisplayDuration = 3f;
    [HideInInspector]
    public float cityNameFadeDuration = 0.5f;
    [HideInInspector]
    public float cityNameFloatDistance = 30f;
    [HideInInspector]
    public float cityNameFloatDownDistance = 20f;
    [Header("City Name Localization Keys")]
    [Tooltip("Keys de tradução para os nomes das 7 cidades. Ordem: City1, City2, City3, City4, City5, City6, City7")]
    public string[] cityNameKeys = new string[7];
    [Header("Debug - Estado Atual")]
    [SerializeField, Tooltip("Cidade atual detectada (somente leitura)")]
    private int _currentCityNumber = 0;
    private SaveClientZone _saveZone;
    private HashSet<int> _citiesShownThisSession = new HashSet<int>();

    public UnityEvent onAwake;
    void Awake()
    {
        onAwake?.Invoke();
        if (onStartCardGame == null)
        {
            onStartCardGame = new GameSetupEvent();
        }
        onStartCardGame.AddListener(StartCardGame);
        if (playerControl != null) playerControl.SetControl(false);
        _saveZone = FindFirstObjectByType<SaveClientZone>();
        _citiesShownThisSession.Clear();
        if (cityNameCanvasGroup == null)
        {
            GameObject canvasObj = GameObject.Find("CanvasCityName");
            if (canvasObj != null)
            {
                cityNameCanvasGroup = canvasObj.GetComponent<CanvasGroup>();
            }
        }
        if (cityNameText == null)
        {
            GameObject textObj = GameObject.Find("txtCityName");
            if (textObj != null)
            {
                cityNameText = textObj.GetComponent<TextMeshProUGUI>();
            }
        }
        if (cityNameCanvasGroup != null)
        {
            cityNameCanvasGroup.alpha = 0f;
            cityNameCanvasGroup.blocksRaycasts = false;
            cityNameCanvasGroup.interactable = false;
        }
    }
    void OnEnable()
    {
        GameEvents.OnDialogueFinished += OnGlobalDialogueFinished;
        GameEvents.OnBankChecked += HandleBankChecked;
        GameEvents.OnPlayerTeleport += HandlePlayerTeleport;
        GameEvents.OnCurtainOpenedAfterTeleport += HandleCurtainOpenedAfterTeleport;
    }
    void OnDisable()
    {
        GameEvents.OnDialogueFinished -= OnGlobalDialogueFinished;
        GameEvents.OnBankChecked -= HandleBankChecked;
        GameEvents.OnPlayerTeleport -= HandlePlayerTeleport;
        GameEvents.OnCurtainOpenedAfterTeleport -= HandleCurtainOpenedAfterTeleport;
    }
    void HandlePlayerTeleport(Vector3 pos, WorldAreaId areaId)
    {
        UpdateCurrentCity(areaId);
        RefreshInteractableLogicalObjects();
    }
    void HandleCurtainOpenedAfterTeleport(WorldAreaId areaId)
    {
        if (areaId == WorldAreaId.None) return;
        if (IsCityExternalArea(areaId))
        {
            int cityNumber = ExtractCityNumber(areaId.ToString());
            if (cityNumber > 0 && !_citiesShownThisSession.Contains(cityNumber))
            {
                _citiesShownThisSession.Add(cityNumber);
                ShowCityName(areaId);
            }
        }
    }
    bool IsCityExternalArea(WorldAreaId areaId)
    {
        string areaName = areaId.ToString();
        return areaName.StartsWith("City") && areaName.Contains("_ext_");
    }
    void ShowCityName(WorldAreaId areaId)
    {
        if (cityNameCanvasGroup == null)
        {
            return;
        }
        if (cityNameText == null)
        {
            return;
        }
        UpdateCityNameText(areaId);
        StartCoroutine(CityNameDisplayRoutine());
    }
    public void UpdateCityNameText(WorldAreaId areaId)
    {
        if (cityNameText == null)
        {
            return;
        }
        string localizationKey = GetCityNameKey(areaId);
        if (!string.IsNullOrEmpty(localizationKey))
        {
            if (ManagerLocalization.Instance != null)
            {
                string localizedText = ManagerLocalization.Instance.GetText(localizationKey);
                cityNameText.text = localizedText;
            }
            else
            {
                cityNameText.text = $"[{localizationKey}]";
            }
        }
        else
        {
        }
    }
    string GetCityNameKey(WorldAreaId areaId)
    {
        string areaName = areaId.ToString();
        if (!areaName.StartsWith("City")) return string.Empty;
        int cityNumber = ExtractCityNumber(areaName);
        if (cityNumber < 1 || cityNumber > 7) return string.Empty;
        int index = cityNumber - 1;
        if (index >= 0 && index < cityNameKeys.Length && !string.IsNullOrEmpty(cityNameKeys[index]))
        {
            return cityNameKeys[index];
        }
        return string.Empty;
    }
    int ExtractCityNumber(string areaName)
    {
        int startIndex = areaName.IndexOf("City");
        if (startIndex == -1) return -1;
        startIndex += 4;
        int endIndex = startIndex;
        while (endIndex < areaName.Length && char.IsDigit(areaName[endIndex]))
        {
            endIndex++;
        }
        if (endIndex > startIndex)
        {
            string numberStr = areaName.Substring(startIndex, endIndex - startIndex);
            if (int.TryParse(numberStr, out int number))
            {
                return number;
            }
        }
        return -1;
    }
    IEnumerator CityNameDisplayRoutine()
    {
        if (cityNameCanvasGroup == null || cityNameText == null)
        {
            yield break;
        }
        RectTransform textRect = cityNameText.GetComponent<RectTransform>();
        if (textRect == null)
        {
            yield break;
        }
        Vector2 startPosition = textRect.anchoredPosition;
        Vector2 startPositionAbove = startPosition + Vector2.up * cityNameFloatDownDistance;
        textRect.anchoredPosition = startPositionAbove;
        cityNameCanvasGroup.blocksRaycasts = false;
        cityNameCanvasGroup.interactable = false;
        float timer = 0f;
        while (timer < cityNameFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / cityNameFadeDuration);
            cityNameCanvasGroup.alpha = progress;
            float floatProgress = 1f - progress;
            textRect.anchoredPosition = Vector2.Lerp(startPosition, startPositionAbove, floatProgress);
            yield return null;
        }
        cityNameCanvasGroup.alpha = 1f;
        textRect.anchoredPosition = startPosition;
        yield return new WaitForSecondsRealtime(cityNameDisplayDuration);
        timer = 0f;
        while (timer < cityNameFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / cityNameFadeDuration);
            cityNameCanvasGroup.alpha = 1f - progress;
            float floatProgress = progress;
            textRect.anchoredPosition = startPosition + Vector2.up * (cityNameFloatDistance * floatProgress);
            yield return null;
        }
        cityNameCanvasGroup.alpha = 0f;
        cityNameCanvasGroup.blocksRaycasts = false;
        textRect.anchoredPosition = startPosition;
    }
    void HandleBankChecked() => UnlockZone1Exit();
    void UnlockZone1Exit()
    {
        if (_saveZone != null && flagUnlockDoor != null)
        {
            _saveZone.SetFlag(flagUnlockDoor, 1);
        }
        if (fakeDoorObject != null) fakeDoorObject.SetActive(false);
    }
    void Start()
    {
        CheckInitialState();
        CheckInitialCity();
        if (playerCutscene != null)
        playerCutscene.PlayCutsceneNoControl("Wake", 0.3f, "Interact", StartIntroDialogue);
        else
        StartIntroDialogue();
    }
    void CheckInitialCity()
    {
        StartCoroutine(CheckInitialCityDelayed());
    }
    IEnumerator CheckInitialCityDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        int maxAttempts = 10;
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            var managerCamera = FindFirstObjectByType<ManagerCamera>();
            if (managerCamera == null)
            {
                yield return null;
                attempts++;
                continue;
            }
            WorldAreaId initialArea = WorldAreaId.None;
            if (managerCamera.CurrentArea != null && managerCamera.CurrentArea.id != WorldAreaId.None)
            {
                initialArea = managerCamera.CurrentArea.id;
            }
            else if (managerCamera.startAreaId != WorldAreaId.None)
            {
                initialArea = managerCamera.startAreaId;
            }
            if (initialArea != WorldAreaId.None)
            {
                UpdateCurrentCity(initialArea);
                if (IsCityExternalArea(initialArea))
                {
                    int cityNumber = ExtractCityNumber(initialArea.ToString());
                    if (cityNumber > 0 && !_citiesShownThisSession.Contains(cityNumber))
                    {
                        _citiesShownThisSession.Add(cityNumber);
                        StartCoroutine(DelayedShowCityName(initialArea, 1.5f));
                    }
                }
                yield break;
            }
            yield return null;
            attempts++;
        }
    }
    IEnumerator DelayedShowCityName(WorldAreaId areaId, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ShowCityName(areaId);
    }
    void CheckInitialState()
    {
        if (_saveZone != null && flagUnlockDoor != null && fakeDoorObject != null)
        {
            if (_saveZone.HasFlag(flagUnlockDoor)) fakeDoorObject.SetActive(false);
        }
    }
    void StartIntroDialogue()
    {
        if (_saveZone != null && flagIntroDone != null && _saveZone.HasFlag(flagIntroDone))
        {
            StartGameplayPhase();
            return;
        }
        if (introDialogue != null)
        {
            GameEvents.OnRequestDialogue?.Invoke(introDialogue);
            if (playerControl != null) playerControl.SetControl(false);
        }
        else
        {
            StartGameplayPhase();
        }
    }
    void OnGlobalDialogueFinished(SODialogueSequence sequence)
    {
        if (ManagerPhone.Instance != null && ManagerPhone.Instance.IsOpen) return;
        if (playerControl != null) playerControl.SetControl(true);
        if (sequence == introDialogue)
        {
            if (_saveZone != null && flagIntroDone != null)
            {
                _saveZone.SetFlag(flagIntroDone, 1);
            }
            StartGameplayPhase();
        }
    }
    void StartGameplayPhase()
    {
        if (playerControl != null) playerControl.SetControl(true);
        GameEvents.OnGameplayStarted?.Invoke();
        if (tutorialFirstMovement != null)
        GameEvents.OnRequestTutorialByAsset?.Invoke(tutorialFirstMovement);
    }
    void UpdateCurrentCity(WorldAreaId areaId)
    {
        string areaName = areaId.ToString();
        if (!areaName.StartsWith("City")) return;
        int cityNumber = ExtractCityNumber(areaName);
        if (cityNumber > 0 && cityNumber <= 7)
        {
            _currentCityNumber = cityNumber;
        }
    }
    void RefreshInteractableLogicalObjects()
    {
        var allInteractables = FindObjectsByType<InteractableLogical>(FindObjectsSortMode.None);
        foreach (var interactable in allInteractables)
        {
            if (interactable != null)
            {
                //interactable.UpdateVisualState();
            }
        }
    }
    /// <summary>
    /// Inicia um card game usando o setup padrão configurado no Inspector.
    /// Pode ser chamado via Unity Events.
    /// </summary>
    public void StartCardGame()
    {
        StartCardGame(defaultCardGameSetup);
    }
    /// <summary>
    /// Inicia um card game com um setup específico.
    /// Pode ser chamado via Unity Events passando o SOGameSetup como parâmetro.
    /// </summary>
    /// <param name="gameSetup">Setup do jogo de cartas a ser iniciado</param>
    public void StartCardGame(SOGameSetup gameSetup)
    {
        if (gameSetup == null)
        {
            Debug.LogWarning("ManagerGameFlow: Tentativa de iniciar card game sem SOGameSetup configurado.");
            return;
        }
        var challengeManager = FindFirstObjectByType<ManagerCardGameChallenge>();
        if (challengeManager == null)
        {
            GameObject go = new GameObject("ManagerCardGameChallenge");
            challengeManager = go.AddComponent<ManagerCardGameChallenge>();
        }
        if (challengeManager != null)
        {
            challengeManager.SetGameSetup(gameSetup);
        }
        StartCoroutine(LoadCardGameAfterSave());
    }
    IEnumerator LoadCardGameAfterSave()
    {
        yield return null;
        string sceneName = "CardGame";
        if (ManagerSceneTransition.Instance != null)
        {
            ManagerSceneTransition.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

