using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class UITutorial : MonoBehaviour
{
    public ManagerTutorial tutorialManager;
    public CanvasGroup canvasGroup;
    public Image iconImage;
    public float fadeInDuration = 0.25f;
    public float fadeOutDuration = 0.2f;
    public bool loopScaleWhileVisible = true;
    public float scaleAmplitude = 0.1f;
    public float scaleSpeed = 2f;
    Coroutine fadeRoutine;
    Coroutine scaleRoutine;
    RectTransform rectTransform;
    Vector3 baseScale = Vector3.one;
    void Awake()
    {
        if (tutorialManager == null)
        {
            tutorialManager = FindFirstObjectByType<ManagerTutorial>();
        }
        if (iconImage != null)
        {
            rectTransform = iconImage.rectTransform;
        }
        if (rectTransform != null)
        {
            baseScale = rectTransform.localScale;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    void OnEnable()
    {
        if (tutorialManager == null) return;
        tutorialManager.OnTutorialShown += HandleTutorialShown;
        tutorialManager.OnTutorialClosed += HandleTutorialClosed;
    }
    void OnDisable()
    {
        if (tutorialManager == null) return;
        tutorialManager.OnTutorialShown -= HandleTutorialShown;
        tutorialManager.OnTutorialClosed -= HandleTutorialClosed;
    }
    void HandleTutorialShown(SOTutorial tutorial)
    {
        if (tutorial == null) return;
        if (iconImage != null)
        {
            iconImage.sprite = tutorial.icon;
            iconImage.enabled = tutorial.icon != null;
        }
        StartFadeIn();
    }
    void HandleTutorialClosed(SOTutorial tutorial)
    {
        StartFadeOut();
    }
    void StartFadeIn()
    {
        if (canvasGroup == null) return;
        StopFadeRoutine();
        StopScaleRoutine();
        fadeRoutine = StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration));
        if (loopScaleWhileVisible)
        {
            scaleRoutine = StartCoroutine(ScaleLoopRoutine());
        }
    }
    void StartFadeOut()
    {
        if (canvasGroup == null) return;
        StopFadeRoutine();
        fadeRoutine = StartCoroutine(FadeOutAndStopScaleRoutine());
    }
    IEnumerator FadeOutAndStopScaleRoutine()
    {
        yield return StartCoroutine(FadeRoutine(canvasGroup.alpha, 0f, fadeOutDuration));
        StopScaleRoutine();
    }
    IEnumerator FadeRoutine(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration > 0f ? t / duration : 1f;
            k = Mathf.Clamp01(k);
            float alpha = Mathf.Lerp(from, to, k);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = to;
        fadeRoutine = null;
    }
    IEnumerator ScaleLoopRoutine()
    {
        if (rectTransform == null)
        {
            yield break;
        }
        while (true)
        {
            float time = Time.unscaledTime * scaleSpeed;
            float s = 1f + Mathf.Sin(time) * scaleAmplitude;
            rectTransform.localScale = baseScale * s;
            yield return null;
        }
    }
    void StopFadeRoutine()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }
    void StopScaleRoutine()
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
            scaleRoutine = null;
        }
        if (rectTransform != null)
        {
            rectTransform.localScale = baseScale;
        }
    }
}

