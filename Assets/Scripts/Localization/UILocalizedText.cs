using UnityEngine;
using TMPro;
[RequireComponent(typeof(TMP_Text))]
public class UILocalizedText : MonoBehaviour
{
    public string localizationKey;
    private TMP_Text targetText;
    private void Awake()
    {
        targetText = GetComponent<TMP_Text>();
    }
    private void OnEnable()
    {
        ManagerLocalization.OnLanguageChanged += UpdateText;
        UpdateText();
    }
    private void OnDisable()
    {
        ManagerLocalization.OnLanguageChanged -= UpdateText;
    }
    public void UpdateText()
    {
        if (targetText == null || ManagerLocalization.Instance == null) return;
        if (!string.IsNullOrEmpty(localizationKey))
        {
            targetText.text = ManagerLocalization.Instance.GetText(localizationKey);
        }
    }
}

