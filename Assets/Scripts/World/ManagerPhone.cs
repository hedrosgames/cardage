using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
public class ManagerPhone : MonoBehaviour
{
    public static ManagerPhone Instance { get; private set; }
    public bool IsOpen => isOpen;
    [Header("UI Geral")]
    public CanvasGroup phoneCanvasGroup;
    public CanvasGroup homeScreenGroup;
    [Header("NavegaÃ§Ã£o")]
    public Button btnHome;
    public Button btnClose;
    [Header("PainÃ©is dos Apps")]
    public GameObject pnlBank;
    public GameObject pnlZap;
    public GameObject pnlDrive;
    [Header("ConfiguraÃ§Ã£o - Geral")]
    public SOZoneFlag unlockFlag;
    public float fadeDuration = 0.3f;
    [Header("ConfiguraÃ§Ã£o - DiÃ¡logo ao Fechar")]
    public SOZoneFlag flagConditionToPlayDialogue;
    public SODialogueSequence dialogueOnClose;
    private bool isOpen = false;
    private bool hasPlayedCloseDialogueSession = false;
    private bool tabBlocked = false;
    private SaveClientZone _saveZone;
    private PlayerControl _playerControl;
    private ManagerDialogue _managerDialogue;
    private SaveClientZone SaveZone {
        get { if (_saveZone == null) _saveZone = FindFirstObjectByType<SaveClientZone>(); return _saveZone; }
    }
    private PlayerControl Player {
        get { if (_playerControl == null) _playerControl = FindFirstObjectByType<PlayerControl>(); return _playerControl; }
    }
    private ManagerDialogue Dialogue {
        get { if (_managerDialogue == null) _managerDialogue = FindFirstObjectByType<ManagerDialogue>(); return _managerDialogue; }
    }
    void Awake()
    {
        Instance = this;
        if (phoneCanvasGroup != null)
        {
            phoneCanvasGroup.alpha = 0f;
            phoneCanvasGroup.interactable = false;
            phoneCanvasGroup.blocksRaycasts = false;
        }
        if (btnHome != null) btnHome.onClick.AddListener(GoHome);
        if (btnClose != null) btnClose.onClick.AddListener(ClosePhone);
        CloseAllApps();
    }
    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame && !isOpen) return;
            if (Keyboard.current.tabKey.wasPressedThisFrame && tabBlocked) return;
            TogglePhone();
        }
    }
    public void SetTabBlocked(bool blocked)
    {
        tabBlocked = blocked;
    }
    public void TogglePhone()
    {
        if (!isOpen)
        {
            if (SaveZone != null && unlockFlag != null)
            {
                if (!SaveZone.HasFlag(unlockFlag)) return;
            }
            SetPhoneState(true);
        }
        else
        {
            SetPhoneState(false);
        }
    }
    public void ClosePhone()
    {
        if (isOpen) SetPhoneState(false);
    }
    void SetPhoneState(bool open)
    {
        isOpen = open;
        if (!isOpen)
        {
            GoHome();
            bool startedDialogue = CheckCloseDialogueCondition();
            if (!startedDialogue)
            {
                if (Player != null)
                {
                    Player.SetMovement(true);
                    Player.SetInteraction(true);
                }
            }
        }
        else
        {
            if (Player != null)
            {
                Player.SetMovement(false);
                Player.SetInteraction(false);
            }
        }
        StopAllCoroutines();
        StartCoroutine(AnimateCanvas(isOpen));
    }
    bool CheckCloseDialogueCondition()
    {
        if (hasPlayedCloseDialogueSession) return false;
        if (SaveZone == null || flagConditionToPlayDialogue == null || dialogueOnClose == null) return false;
        if (SaveZone.HasFlag(flagConditionToPlayDialogue))
        {
            hasPlayedCloseDialogueSession = true;
            if (Dialogue != null)
            {
                Dialogue.StartSequence(dialogueOnClose);
                return true;
            }
        }
        return false;
    }
    public void GoHome()
    {
        CloseAllApps();
        if (homeScreenGroup != null)
        {
            homeScreenGroup.alpha = 1f;
            homeScreenGroup.blocksRaycasts = true;
            homeScreenGroup.interactable = true;
        }
    }
    public void OpenApp_Bank()
    {
        OpenPanel(pnlBank);
        var bankScript = pnlBank.GetComponent<PhoneApp_Bank>();
        if (bankScript != null) bankScript.OnAppOpen();
    }
    public void OpenApp_Zap()
    {
        OpenPanel(pnlZap);
        var zapScript = pnlZap.GetComponent<PhoneApp_Zap>();
        if (zapScript != null) zapScript.OnAppOpen();
    }
    public void OpenApp_Drive()
    {
        OpenPanel(pnlDrive);
        var driveScript = pnlDrive.GetComponent<PhoneApp_Drive>();
        if (driveScript != null) driveScript.OnAppOpen();
    }
    void OpenPanel(GameObject panel)
    {
        if (homeScreenGroup != null)
        {
            homeScreenGroup.alpha = 0f;
            homeScreenGroup.blocksRaycasts = false;
        }
        CloseAllApps();
        if (panel != null) panel.SetActive(true);
    }
    void CloseAllApps()
    {
        if (pnlBank != null) pnlBank.SetActive(false);
        if (pnlZap != null) pnlZap.SetActive(false);
        if (pnlDrive != null) pnlDrive.SetActive(false);
    }
    IEnumerator AnimateCanvas(bool show)
    {
        float start = phoneCanvasGroup.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;
        if (!show)
        {
            phoneCanvasGroup.interactable = false;
            phoneCanvasGroup.blocksRaycasts = false;
        }
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            phoneCanvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        phoneCanvasGroup.alpha = end;
        if (show)
        {
            phoneCanvasGroup.interactable = true;
            phoneCanvasGroup.blocksRaycasts = true;
        }
    }
}

