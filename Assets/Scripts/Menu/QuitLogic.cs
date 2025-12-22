using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class QuitLogic : MonoBehaviour
{
    public ManagerMainMenu mainMenu;
    public TextMeshProUGUI messageText;
    public GameObject buttonsContainer;
    public Button yesButton;
    public Button noButton;

    int currentStage = 0;

    void OnEnable()
    {
        currentStage = 0;
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        ManagerLocalization.OnLanguageChanged += UpdateUI;
        
        if (AchievementSystem.IsUnlocked("achiev1"))
        {
            currentStage = 2;
        }
        UpdateUI();
    }

    void OnDisable()
    {
        if (currentStage != 2)
        {
            currentStage = 0;
        }
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
            if (ManagerAchievements.Instance != null)
            {
                ManagerAchievements.Instance.UnlockQuitAchievement();
            }
            else
            {
                AchievementSystem.Unlock("achiev1");
            }
            UpdateUI();
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

    IEnumerator UpdateUIDelayed()
    {
        yield return null;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (ManagerLocalization.Instance == null || messageText == null) return;

        if (currentStage == 0)
        {
            messageText.text = ManagerLocalization.Instance.GetText("MENU_TXTQUIT_1");
            if (buttonsContainer != null) buttonsContainer.SetActive(true);
        }
        else if (currentStage == 1)
        {
            messageText.text = ManagerLocalization.Instance.GetText("MENU_TXTQUIT_2");
            if (buttonsContainer != null) buttonsContainer.SetActive(true);
        }
        else
        {
            messageText.text = ManagerLocalization.Instance.GetText("MENU_TXTQUIT_3");
            if (buttonsContainer != null) buttonsContainer.SetActive(false);
        }
    }
}
