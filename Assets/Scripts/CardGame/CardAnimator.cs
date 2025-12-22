using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class CardAnimator : MonoBehaviour
{
    public RectTransform cardRect;
    public Image background;
    bool animating;
    public void PlayCaptureAnimation(Color newColor)
    {
        if (!animating)
        StartCoroutine(Flip(newColor));
    }
    IEnumerator Flip(Color newColor)
    {
        animating = true;
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            cardRect.localScale = new Vector3(1f - (t / 0.15f), 1f, 1f);
            yield return null;
        }
        background.color = newColor;
        t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            cardRect.localScale = new Vector3((t / 0.15f), 1f, 1f);
            yield return null;
        }
        cardRect.localScale = Vector3.one;
        animating = false;
    }
}

