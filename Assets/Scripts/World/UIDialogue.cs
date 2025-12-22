using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
public class UIDialogue : MonoBehaviour
{
    public ManagerDialogue dialogueManager;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI speakerLabel;
    public TextMeshProUGUI textLabel;
    public Image portraitImage;
    public float fadeInDuration = 0.2f;
    public float fadeOutDuration = 0.2f;
    public bool useTypewriter = true;
    public float charsPerSecond = 40f;
    Coroutine fadeRoutine;
    Coroutine typeRoutine;
    InputAction advanceDialogueAction;
    private float inputCooldownTimestamp = 0f;
    void Awake()
    {
        if (dialogueManager == null) dialogueManager = FindFirstObjectByType<ManagerDialogue>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (textLabel != null) textLabel.text = "";
        if (speakerLabel != null) speakerLabel.text = "";
        if (portraitImage != null) { portraitImage.sprite = null; portraitImage.enabled = false; }
        advanceDialogueAction = new InputAction("AdvanceDialogue", type: InputActionType.Button);
        advanceDialogueAction.AddBinding("<Keyboard>/enter");
        advanceDialogueAction.AddBinding("<Mouse>/leftButton");
        advanceDialogueAction.AddBinding("<Keyboard>/space");
        advanceDialogueAction.AddBinding("<Keyboard>/e");
    }
    void OnEnable()
    {
        GameEvents.OnDialogueFinished += HandleSequenceFinished;
        if (dialogueManager == null) return;
        dialogueManager.OnSequenceStarted += HandleSequenceStarted;
        dialogueManager.OnLineShown += HandleLineShown;
        advanceDialogueAction.performed += OnAdvancePerformed;
        advanceDialogueAction.Enable();
    }
    void OnDisable()
    {
        GameEvents.OnDialogueFinished -= HandleSequenceFinished;
        if (dialogueManager != null)
        {
            dialogueManager.OnSequenceStarted -= HandleSequenceStarted;
            dialogueManager.OnLineShown -= HandleLineShown;
        }
        advanceDialogueAction.performed -= OnAdvancePerformed;
        advanceDialogueAction.Disable();
    }
    void OnAdvancePerformed(InputAction.CallbackContext context)
    {
        if (Time.unscaledTime < inputCooldownTimestamp) return;
        if (canvasGroup != null && canvasGroup.alpha > 0.9f && dialogueManager != null)
        {
            if (typeRoutine != null)
            {
                StopCoroutine(typeRoutine);
                typeRoutine = null;
                var line = dialogueManager.currentSequence.lines[dialogueManager.currentIndex];
                textLabel.text = GetLocalizedText(line.text);
            }
            else
            {
                dialogueManager.NextLine();
            }
        }
    }
    void HandleSequenceStarted(SODialogueSequence seq)
    {
        inputCooldownTimestamp = Time.unscaledTime + 0.5f;
        StartFadeIn();
    }
    void HandleSequenceFinished(SODialogueSequence seq)
    {
        StartFadeOut();
    }
    void HandleLineShown(SODialogueSequence.DialogueLine line)
    {
        if (line == null) return;
        if (speakerLabel != null)
        {
            string speakerKey = string.IsNullOrEmpty(line.speaker) ? "" : line.speaker;
            speakerLabel.text = GetLocalizedText(speakerKey);
        }
        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = line.portrait != null;
        }
        if (textLabel == null) return;
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }
        string finalText = GetLocalizedText(line.text);
        if (useTypewriter)
        typeRoutine = StartCoroutine(TypeRoutine(finalText));
        else
        textLabel.text = finalText;
    }
    string GetLocalizedText(string keyOrText)
    {
        if (string.IsNullOrEmpty(keyOrText)) return "";
        if (ManagerLocalization.Instance != null) return ManagerLocalization.Instance.GetText(keyOrText);
        return keyOrText;
    }
    void StartFadeIn()
    {
        if (canvasGroup == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration));
        canvasGroup.blocksRaycasts = true;
    }
    void StartFadeOut()
    {
        if (canvasGroup == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(canvasGroup.alpha, 0f, fadeOutDuration));
        canvasGroup.blocksRaycasts = false;
    }
    IEnumerator FadeRoutine(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration > 0f ? t / duration : 1f;
            k = Mathf.Clamp01(k);
            canvasGroup.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        canvasGroup.alpha = to;
        fadeRoutine = null;
    }
    IEnumerator TypeRoutine(string fullText)
    {
        if (textLabel == null) yield break;
        textLabel.text = "";
        if (string.IsNullOrEmpty(fullText)) yield break;
        float t = 0f;
        int length = fullText.Length;
        while (true)
        {
            t += Time.unscaledDeltaTime * charsPerSecond;
            int charCount = Mathf.Clamp(Mathf.FloorToInt(t), 0, length);
            textLabel.text = fullText.Substring(0, charCount);
            if (charCount >= length) break;
            yield return null;
        }
        textLabel.text = fullText;
        typeRoutine = null;
    }
}

