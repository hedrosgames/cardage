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
        
        // Inicializa o canvas do nome da cidade
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
        
        // Garante que o canvas começa escondido
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
        // Verifica se é uma área externa de cidade e se ainda não mostrou o nome nesta sessão
        if (IsCityExternalArea(areaId) && !_hasShownCityName)
        {
            _hasShownCityName = true;
            ShowCityName(areaId);
        }
    }
    
    bool IsCityExternalArea(WorldAreaId areaId)
    {
        string areaName = areaId.ToString();
        // Verifica se é uma área externa (City1_ext_*)
        return areaName.StartsWith("City") && areaName.Contains("_ext_");
    }
    
    void ShowCityName(WorldAreaId areaId)
    {
        if (cityNameCanvasGroup == null || cityNameText == null) return;
        
        // Usa o texto que já está configurado no TextMeshProUGUI
        // Futuramente: usar key de localização aqui
        // Exemplo: cityNameText.text = LocalizationManager.GetText("city." + areaId.ToString());
        
        StartCoroutine(CityNameDisplayRoutine());
    }
    
    IEnumerator CityNameDisplayRoutine()
    {
        if (cityNameCanvasGroup == null || cityNameText == null) yield break;
        
        // Guarda a posição inicial
        RectTransform textRect = cityNameText.GetComponent<RectTransform>();
        Vector2 startPosition = textRect.anchoredPosition;
        Vector2 startPositionAbove = startPosition + Vector2.up * cityNameFloatDownDistance;
        
        // Posiciona o texto acima da posição original
        if (textRect != null)
        {
            textRect.anchoredPosition = startPositionAbove;
        }
        
        // Fade In com movimento para baixo
        cityNameCanvasGroup.blocksRaycasts = false;
        cityNameCanvasGroup.interactable = false;
        
        float timer = 0f;
        while (timer < cityNameFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / cityNameFadeDuration);
            cityNameCanvasGroup.alpha = progress;
            // Move o texto para baixo suavemente até a posição original
            if (textRect != null)
            {
                float floatProgress = 1f - progress; // Inverte para começar acima e ir para baixo
                textRect.anchoredPosition = Vector2.Lerp(startPosition, startPositionAbove, floatProgress);
            }
            yield return null;
        }
        cityNameCanvasGroup.alpha = 1f;
        // Garante que está na posição correta
        if (textRect != null)
        {
            textRect.anchoredPosition = startPosition;
        }
        
        // Aguarda o tempo de exibição
        yield return new WaitForSecondsRealtime(cityNameDisplayDuration);
        
        // Fade Out com movimento para cima
        timer = 0f;
        while (timer < cityNameFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / cityNameFadeDuration);
            cityNameCanvasGroup.alpha = 1f - progress;
            // Move o texto para cima suavemente
            if (textRect != null)
            {
                float floatProgress = progress; // Usa a mesma progressão do fade
                textRect.anchoredPosition = startPosition + Vector2.up * (cityNameFloatDistance * floatProgress);
            }
            yield return null;
        }
        cityNameCanvasGroup.alpha = 0f;
        cityNameCanvasGroup.blocksRaycasts = false;
        
        // Restaura a posição original
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
        // Verifica se o jogador começa em uma área externa de cidade
        var managerCamera = FindFirstObjectByType<ManagerCamera>();
        if (managerCamera != null && managerCamera.startAreaId != WorldAreaId.None)
        {
            WorldAreaId initialArea = managerCamera.startAreaId;
            if (IsCityExternalArea(initialArea) && !_hasShownCityName)
            {
                _hasShownCityName = true;
                // Aguarda um pouco antes de mostrar o nome (para não conflitar com a cutscene inicial)
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

