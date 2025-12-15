using System.Collections;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
public class ScreenTransition : MonoBehaviour
{
    public Canvas canvas;
    public CanvasGroup canvasGroup;
    [Header("Configuração")]
    [Tooltip("Tempo em segundos para o fade acontecer")]
    public float fadeDuration = 1f;
    void Awake()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        if (fadeDuration <= 0f) fadeDuration = 0.5f;
    }
    public void SetInstantAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
        bool isVisible = alpha > 0.01f;
        canvasGroup.blocksRaycasts = isVisible;
        canvas.enabled = isVisible;
    }
    public IEnumerator PlayCloseRoutine()
    {
        canvas.enabled = true;
        canvasGroup.blocksRaycasts = true;
        float timer = 0f;
        float startAlpha = canvasGroup.alpha;
        float duration = Mathf.Max(fadeDuration, 0.1f);
        while (timer < duration)
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            timer += dt;
            float progress = Mathf.Clamp01(timer / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, progress);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    public IEnumerator PlayOpenRoutine()
    {
        canvas.enabled = true;
        canvasGroup.blocksRaycasts = true;
        float timer = 0f;
        float startAlpha = canvasGroup.alpha;
        float duration = Mathf.Max(fadeDuration, 0.1f);
        while (timer < duration)
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            timer += dt;
            float progress = Mathf.Clamp01(timer / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvas.enabled = false;
    }
}

