using UnityEngine;
using TMPro;
using System.Collections;
public class UIFloatingText : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public float duration = 1.5f;
    public float floatSpeed = 50f;
    public Vector3 offset = new Vector3(0, 50, 0);
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 0.2f, 1f);
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0.8f, 1f, 1f, 0f);
    private float timer = 0f;
    private Vector3 startPos;
    void Awake()
    {
        if (tmpText == null) tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null) tmpText.raycastTarget = false;
    }
    void Start()
    {
        startPos = transform.position;
        startPos += new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
        transform.position = startPos;
    }
    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;
        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = startPos + (Vector3.up * floatSpeed * progress);
        float s = scaleCurve.Evaluate(progress);
        transform.localScale = Vector3.one * s;
        if (tmpText != null)
        {
            float a = alphaCurve.Evaluate(progress);
            Color c = tmpText.color;
            c.a = a;
            tmpText.color = c;
        }
    }
    public void SetText(string text, Color color)
    {
        if (tmpText != null)
        {
            tmpText.text = text;
            tmpText.color = color;
        }
    }
}

