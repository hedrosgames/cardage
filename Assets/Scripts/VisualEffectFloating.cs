using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class VisualEffectFloating : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    [Header("AnimaÃ§Ã£o")]
    public float duration = 1.0f;
    public float floatSpeed = 2f;
    public Vector3 endScale = new Vector3(1.2f, 1.2f, 1f);
    [Header("Curvas")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.5f, 0.2f, 1f);
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0.7f, 1f, 1f, 0f);
    private float timer = 0f;
    private Vector3 startPos;
    private Vector3 startScale;
    void Awake()
    {
        if (spriteRenderer == null)
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        startPos = transform.position;
        startScale = transform.localScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
        }
    }
    public void Setup(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
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
        transform.localScale = Vector3.Lerp(startScale, endScale, s);
        if (spriteRenderer != null)
        {
            float a = alphaCurve.Evaluate(progress);
            Color c = spriteRenderer.color;
            c.a = a;
            spriteRenderer.color = c;
        }
    }
}

