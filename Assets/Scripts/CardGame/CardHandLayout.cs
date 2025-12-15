using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CardHandLayout : MonoBehaviour
{
    public float spread = 140f;
    public float arc = 10f;
    public float yOffset = 0f;
    public float centerLift = 40f;
    public float centerFactor = 0.7f;
    public float animTime = 0.25f;
    public float openDelay = 0.35f;
    RectTransform rt;
    List<CardButton> buttons = new List<CardButton>();
    Coroutine animRoutine;
    public bool isAnimatingOpen = false;
    bool hasOpened = false;
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (Application.isPlaying)
        {
            ResetAllCards();
            StartCoroutine(OpenAfterDelay());
        }
    }
    IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSeconds(openDelay);
        Rebuild(true);
    }
    void ResetAllCards()
    {
        buttons.Clear();
        foreach (Transform t in transform)
        {
            CardButton b = t.GetComponent<CardButton>();
            if (b != null)
            {
                RectTransform r = b.rectTransform;
                r.anchoredPosition = Vector2.zero;
                r.localRotation = Quaternion.identity;
            }
        }
    }
    public void Rebuild(bool force = false)
    {
        buttons.Clear();
        foreach (Transform t in transform)
        {
            CardButton b = t.GetComponent<CardButton>();
            if (b != null && t.gameObject.activeSelf)
            buttons.Add(b);
        }
        int count = buttons.Count;
        if (count == 0) return;
        if (Application.isPlaying && !force && !hasOpened)
        return;
        List<Vector2> targetPos = new List<Vector2>();
        List<Quaternion> targetRot = new List<Quaternion>();
        if (count == 1)
        {
            float lift = centerLift * centerFactor;
            targetPos.Add(new Vector2(0f, yOffset + lift));
            targetRot.Add(Quaternion.identity);
            if (animRoutine != null) StopCoroutine(animRoutine);
            animRoutine = StartCoroutine(Animate(targetPos, targetRot));
            return;
        }
        float centerIndex = (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float dx = (i - centerIndex) * spread;
            float factor = 1f - Mathf.Abs(i - centerIndex) / centerIndex;
            factor = Mathf.Clamp01(factor);
            float lift = centerLift * factor;
            if (count % 2 == 1 && Mathf.Abs(i - centerIndex) < 0.001f)
            lift *= centerFactor;
            Vector2 pos = new Vector2(dx, yOffset + lift);
            float zRot = -(i - centerIndex) * arc;
            targetPos.Add(pos);
            targetRot.Add(Quaternion.Euler(0f, 0f, zRot));
        }
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(Animate(targetPos, targetRot));
    }
    IEnumerator Animate(List<Vector2> targetPos, List<Quaternion> targetRot)
    {
        isAnimatingOpen = true;
        float t = 0f;
        List<Vector2> startPos = new List<Vector2>();
        List<Quaternion> startRot = new List<Quaternion>();
        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform r = buttons[i].rectTransform;
            startPos.Add(r.anchoredPosition);
            startRot.Add(r.localRotation);
        }
        while (t < 1f)
        {
            t += Time.deltaTime / animTime;
            float k = Mathf.Clamp01(t);
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].IsDragging()) continue;
                RectTransform r = buttons[i].rectTransform;
                r.anchoredPosition = Vector2.Lerp(startPos[i], targetPos[i], k);
                r.localRotation = Quaternion.Lerp(startRot[i], targetRot[i], k);
            }
            yield return null;
        }
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i].IsDragging()) continue;
            RectTransform r = buttons[i].rectTransform;
            r.anchoredPosition = targetPos[i];
            r.localRotation = targetRot[i];
            buttons[i].UpdateInitialStateFromLayout();
        }
        isAnimatingOpen = false;
        hasOpened = true;
        animRoutine = null;
    }
}

