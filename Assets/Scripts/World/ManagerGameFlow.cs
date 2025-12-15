using UnityEngine;
using Game.World;
using UnityEngine.SceneManagement;
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
    private SaveClientZone _saveZone;
    void Awake()
    {
        if (playerControl != null) playerControl.SetControl(false);
        _saveZone = FindFirstObjectByType<SaveClientZone>();
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
        if (playerCutscene != null)
        playerCutscene.PlayCutsceneNoControl("Wake", 0.3f, "Interact", StartIntroDialogue);
        else
        StartIntroDialogue();
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

