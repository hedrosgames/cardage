using UnityEngine;
using Game.World;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
public class ManagerGameFlow : MonoBehaviour
{
    [Header("Sistemas")]
    public ManagerDialogue dialogueManager;
    public PlayerControl playerControl;
    public PlayerCutsceneAnimator playerCutscene;
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
    [Header("Controle de Momentos")]
    [Tooltip("Referências aos GameObjects World_City_X (pais das cidades). Ordem: City1, City2, City3, City4, City5, City6, City7")]
    public GameObject[] cityParents = new GameObject[7];
    [Header("Debug - Estado Atual")]
    [SerializeField, Tooltip("Cidade atual detectada (somente leitura)")]
    private int _currentCityNumber = 0;
    [SerializeField, Tooltip("Momento atual do jogo (somente leitura)")]
    private int _currentMomentNumber = 0;
    private SaveClientZone _saveZone;
    private SaveClientMoment _saveMoment;
    private bool _hasShownCityName = false;
    void Awake()
    {
        if (playerControl != null) playerControl.SetControl(false);
        _saveZone = FindFirstObjectByType<SaveClientZone>();
        _saveMoment = FindFirstObjectByType<SaveClientMoment>();
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
        if (_saveMoment != null)
        {
            _saveMoment.OnMomentChanged += OnMomentChanged;
        }
    }
    void OnDisable()
    {
        GameEvents.OnDialogueFinished -= OnGlobalDialogueFinished;
        GameEvents.OnBankChecked -= HandleBankChecked;
        GameEvents.OnPlayerTeleport -= HandlePlayerTeleport;
        if (_saveMoment != null)
        {
            _saveMoment.OnMomentChanged -= OnMomentChanged;
        }
    }
    void HandlePlayerTeleport(Vector3 pos, WorldAreaId areaId)
    {
        if (IsCityExternalArea(areaId) && !_hasShownCityName)
        {
            _hasShownCityName = true;
            ShowCityName(areaId);
        }
        UpdateCurrentCity(areaId);
        RefreshMomentObjects();
    }
    void OnMomentChanged(int newMoment)
    {
        _currentMomentNumber = newMoment;
        RefreshMomentObjects();
    }
    bool IsCityExternalArea(WorldAreaId areaId)
    {
        string areaName = areaId.ToString();
        return areaName.StartsWith("City") && areaName.Contains("_ext_");
    }
    void ShowCityName(WorldAreaId areaId)
    {
        if (cityNameCanvasGroup == null || cityNameText == null) return;
        UpdateCityNameText(areaId);
        StartCoroutine(CityNameDisplayRoutine());
    }
    public void UpdateCityNameText(WorldAreaId areaId)
    {
        if (cityNameText == null) return;
        string localizationKey = GetCityNameKey(areaId);
        if (!string.IsNullOrEmpty(localizationKey))
        {
            if (ManagerLocalization.Instance != null)
            {
                cityNameText.text = ManagerLocalization.Instance.GetText(localizationKey);
            }
            else
            {
                cityNameText.text = $"[{localizationKey}]";
            }
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
        if (cityNameCanvasGroup == null || cityNameText == null) yield break;
        RectTransform textRect = cityNameText.GetComponent<RectTransform>();
        Vector2 startPosition = textRect.anchoredPosition;
        Vector2 startPositionAbove = startPosition + Vector2.up * cityNameFloatDownDistance;
        if (textRect != null)
        {
            textRect.anchoredPosition = startPositionAbove;
        }
        cityNameCanvasGroup.blocksRaycasts = false;
        cityNameCanvasGroup.interactable = false;
        float timer = 0f;
        while (timer < cityNameFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / cityNameFadeDuration);
            cityNameCanvasGroup.alpha = progress;
            if (textRect != null)
            {
                float floatProgress = 1f - progress;
                textRect.anchoredPosition = Vector2.Lerp(startPosition, startPositionAbove, floatProgress);
            }
            yield return null;
        }
        cityNameCanvasGroup.alpha = 1f;
        if (textRect != null)
        {
            textRect.anchoredPosition = startPosition;
        }
        yield return new WaitForSecondsRealtime(cityNameDisplayDuration);
        timer = 0f;
        while (timer < cityNameFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / cityNameFadeDuration);
            cityNameCanvasGroup.alpha = 1f - progress;
            if (textRect != null)
            {
                float floatProgress = progress;
                textRect.anchoredPosition = startPosition + Vector2.up * (cityNameFloatDistance * floatProgress);
            }
            yield return null;
        }
        cityNameCanvasGroup.alpha = 0f;
        cityNameCanvasGroup.blocksRaycasts = false;
        if (textRect != null)
        {
            textRect.anchoredPosition = startPosition;
        }
    }
    void HandleBankChecked() => UnlockZone1Exit();
    void UnlockZone1Exit()
    {
        if (_saveZone != null && flagUnlockDoor != null)
        {
            _saveZone.SetFlag(flagUnlockDoor, 1);
            SaveEvents.RaiseSave();
        }
        if (fakeDoorObject != null) fakeDoorObject.SetActive(false);
    }
    void Start()
    {
        CheckInitialState();
        CheckInitialCity();
        if (_saveMoment == null) _saveMoment = FindFirstObjectByType<SaveClientMoment>();
        UpdateDebugInfo();
        RefreshMomentObjects();
        if (playerCutscene != null)
        playerCutscene.PlayCutsceneNoControl("Wake", 0.3f, "Interact", StartIntroDialogue);
        else
        StartIntroDialogue();
    }
    void CheckInitialCity()
    {
        var managerCamera = FindFirstObjectByType<ManagerCamera>();
        if (managerCamera != null && managerCamera.startAreaId != WorldAreaId.None)
        {
            WorldAreaId initialArea = managerCamera.startAreaId;
            UpdateCurrentCity(initialArea);
            RefreshMomentObjects();
            if (IsCityExternalArea(initialArea) && !_hasShownCityName)
            {
                _hasShownCityName = true;
                StartCoroutine(DelayedShowCityName(initialArea, 1f));
            }
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
                SaveEvents.RaiseSave();
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
            UpdateDebugInfo();
        }
    }
    void UpdateDebugInfo()
    {
        if (_saveMoment != null)
        {
            _currentMomentNumber = _saveMoment.GetMoment();
        }
    }
    void RefreshMomentObjects()
    {
        if (_currentCityNumber < 1 || _currentCityNumber > 7) return;
        if (_saveMoment == null) return;
        int activeMoment = _saveMoment.GetMoment();
        _currentMomentNumber = activeMoment;
        GameObject cityParent = null;
        int cityIndex = _currentCityNumber - 1;
        if (cityIndex >= 0 && cityIndex < cityParents.Length && cityParents[cityIndex] != null)
        {
            cityParent = cityParents[cityIndex];
        }
        if (cityParent == null)
        {
            cityParent = GameObject.Find($"World_City_{_currentCityNumber}");
        }
        if (cityParent == null) return;
        ProcessMomentObjectsInCity(cityParent.transform, activeMoment);
    }
    void ProcessMomentObjectsInCity(Transform parent, int activeMoment)
    {
        if (parent == null) return;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == null) continue;
            GameObject obj = child.gameObject;
            if (obj == null) continue;
            bool hasMomentTag = false;
            for (int moment = 1; moment <= 5; moment++)
            {
                string momentTag = $"Moment_{moment}";
                if (obj.CompareTag(momentTag))
                {
                    hasMomentTag = true;
                    bool shouldBeActive = (moment == activeMoment);
                    obj.SetActive(shouldBeActive);
                    break;
                }
            }
            if (!hasMomentTag)
            {
                obj.SetActive(true);
            }
            ProcessMomentObjectsInCity(child, activeMoment);
        }
    }
    public void ForceRefreshMoments()
    {
        var managerCamera = FindFirstObjectByType<ManagerCamera>();
        if (managerCamera != null && managerCamera.CurrentArea != null)
        {
            UpdateCurrentCity(managerCamera.CurrentArea.id);
        }
        RefreshMomentObjects();
    }
}

