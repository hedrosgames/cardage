using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class UIEntryAnimation : MonoBehaviour
{
    [Header("ConfiguraÃ§Ã£o")]
    public List<RectTransform> elements;
    [Header("AnimaÃ§Ã£o")]
    public float startDelay = 2.0f;
    public float staggerTime = 0.1f;
    public float slideDuration = 0.5f;
    public float startOffsetX = -100f;
    private List<Vector2> finalPositions = new List<Vector2>();
    private List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
    private LayoutGroup layoutGroup;
    private bool wasLayoutEnabled;
    void Awake()
    {
        layoutGroup = GetComponent<LayoutGroup>();
        wasLayoutEnabled = layoutGroup != null && layoutGroup.enabled;
    }
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        if (wasLayoutEnabled)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            layoutGroup.enabled = false;
        }
        PrepareElements();
        yield return new WaitForSeconds(startDelay);
        yield return StartCoroutine(AnimateInRoutine());
        if (wasLayoutEnabled && layoutGroup != null)
        {
            layoutGroup.enabled = true;
        }
    }
    void PrepareElements()
    {
        finalPositions.Clear();
        canvasGroups.Clear();
        foreach (var el in elements)
        {
            if (el == null) continue;
            finalPositions.Add(el.anchoredPosition);
            CanvasGroup cg = el.GetComponent<CanvasGroup>();
            if (cg == null) cg = el.gameObject.AddComponent<CanvasGroup>();
            canvasGroups.Add(cg);
            cg.alpha = 0f;
            cg.interactable = false;
        }
    }
    IEnumerator AnimateInRoutine()
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i] != null)
            {
                Vector2 dest = finalPositions[i];
                Vector2 start = new Vector2(dest.x + startOffsetX, dest.y);
                StartCoroutine(SlideElementRoutine(elements[i], canvasGroups[i], start, dest));
                yield return new WaitForSeconds(staggerTime);
            }
        }
    }
    IEnumerator SlideElementRoutine(RectTransform rect, CanvasGroup cg, Vector2 startPos, Vector2 targetPos)
    {
        float t = 0f;
        rect.anchoredPosition = startPos;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / slideDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
            cg.alpha = Mathf.Lerp(0f, 1f, smoothT);
            yield return null;
        }
        rect.anchoredPosition = targetPos;
        cg.alpha = 1f;
        cg.interactable = true;
    }
}

