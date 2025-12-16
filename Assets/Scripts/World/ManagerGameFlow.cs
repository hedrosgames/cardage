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
    [Tooltip("Tempo em segundos que o nome da cidade fica visível")]
    public float cityNameDisplayDuration = 3f;
    [Tooltip("Tempo em segundos para o fade in/out")]
    public float cityNameFadeDuration = 0.5f;
    [Tooltip("Distância em pixels que o texto sobe durante o fade out")]
    public float cityNameFloatDistance = 30f;
    [Tooltip("Distância em pixels que o texto desce durante o fade in (começa acima)")]
    public float cityNameFloatDownDistance = 20f;
    private SaveClientZone _saveZone;
    private bool _hasShownCityName = false;
    void Awake()
    {
        if (playerControl != null) playerControl.SetControl(false);
        _saveZone = FindFirstObjectByType<SaveClientZone>();
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
    }
    void OnDisable()
    {
        GameEvents.OnDialogueFinished -= OnGlobalDialogueFinished;
        GameEvents.OnBankChecked -= HandleBankChecked;
        GameEvents.OnPlayerTeleport -= HandlePlayerTeleport;
    }
    void HandlePlayerTeleport(Vector3 pos, WorldAreaId areaId)
    {
        if (IsCityExternalArea(areaId) && !_hasShownCityName)
        {
            _hasShownCityName = true;
            ShowCityName(areaId);
        }
    }
    bool IsCityExternalArea(WorldAreaId areaId)
    {
        string areaName = areaId.ToString();
        return areaName.StartsWith("City") && areaName.Contains("_ext_");
    }
    void ShowCityName(WorldAreaId areaId)
    {
        if (cityNameCanvasGroup == null || cityNameText == null) return;
        StartCoroutine(CityNameDisplayRoutine());
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
}

