using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
public class ManagerQuitLogic : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    public ManagerMainMenu mainMenu;
    public TextMeshProUGUI messageText;
    public GameObject buttonsContainer;
    public Button yesButton;
    public Button noButton;
    [System.Serializable]
    class QuitData
    {
        public bool completo;
    }
    int currentStage = 0;
    void OnEnable()
    {
        currentStage = 0;
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.RegisterClient(saveDefinition, this);
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        ManagerLocalization.OnLanguageChanged += UpdateUI;
    }
    void Start()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.LoadSpecific(saveDefinition.id);
        }
        UpdateUI();
    }
    void OnDisable()
    {
        if (currentStage != 2)
        {
            currentStage = 0;
            UpdateUI();
        }
        if (ManagerSave.Instance != null && saveDefinition != null)
        ManagerSave.Instance.UnregisterClient(saveDefinition, this);
        yesButton.onClick.RemoveListener(OnYesClicked);
        noButton.onClick.RemoveListener(OnNoClicked);
        ManagerLocalization.OnLanguageChanged -= UpdateUI;
    }
    void OnYesClicked()
    {
        if (currentStage == 0)
        {
            currentStage = 1;
            UpdateUI();
        }
        else if (currentStage == 1)
        {
            currentStage = 2;
            AchievementSystem.Unlock("achiev1");
            UpdateUI();
            if (saveDefinition != null)
            {
                SaveEvents.RaiseSaveSpecific(saveDefinition.id);
            }
        }
    }
    void OnNoClicked()
    {
        ResetState();
        if (mainMenu != null)
        {
            mainMenu.CancelQuit();
        }
    }
    public void ResetState()
    {
        if (currentStage != 2)
        {
            currentStage = 0;
            StartCoroutine(UpdateUIDelayed());
        }
    }
    System.Collections.IEnumerator UpdateUIDelayed()
    {
        yield return null;
        UpdateUI();
    }
    void UpdateUI()
    {
        if (ManagerLocalization.Instance == null) return;
        if (currentStage == 0)
        {
            messageText.text = ManagerLocalization.Instance.GetText("MENU_TXTQUIT_1");
            buttonsContainer.SetActive(true);
        }
        else if (currentStage == 1)
        {
            messageText.text = ManagerLocalization.Instance.GetText("MENU_TXTQUIT_2");
            buttonsContainer.SetActive(true);
        }
        else
        {
            messageText.text = ManagerLocalization.Instance.GetText("MENU_TXTQUIT_3");
            buttonsContainer.SetActive(false);
        }
    }
    public string Save(SOSaveDefinition definition)
    {
        if (currentStage == 2)
        {
            var data = new QuitData { completo = true };
            return JsonUtility.ToJson(data);
        }
        return string.Empty;
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            currentStage = 0;
            UpdateUI();
            return;
        }
        var data = JsonUtility.FromJson<QuitData>(json);
        if (data != null && data.completo)
        {
            currentStage = 2;
        }
        else
        {
            currentStage = 0;
        }
        UpdateUI();
    }
}

