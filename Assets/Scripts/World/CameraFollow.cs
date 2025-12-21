using UnityEngine;
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public Vector2 offset;
    [Header("Estado (Controlado pelo Manager)")]
    public bool useBounds = false;
    public Vector2 minLimit;
    public Vector2 maxLimit;
    private Camera cam;
    private float camHeight;
    private float camWidth;
    private float lastAspect; // Cache do aspect para evitar verificação desnecessária
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraSize();
        lastAspect = cam.aspect; // Inicializar cache
    }
    void UpdateCameraSize()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam.orthographic)
        {
            camHeight = cam.orthographicSize;
            camWidth = camHeight * cam.aspect;
            lastAspect = cam.aspect; // Atualizar cache
        }
    }
    public void SetBounds(bool active, Vector2 min, Vector2 max)
    {
        useBounds = active;
        minLimit = min;
        maxLimit = max;
        UpdateCameraSize();
    }
    void LateUpdate()
    {
        if (target == null) return;
        // Otimização: Verificar aspect apenas se realmente mudou
        if (cam.aspect != lastAspect)
        {
            UpdateCameraSize();
        }
        Vector3 targetPos = transform.position;
        float desiredX = target.position.x + offset.x;
        float desiredY = target.position.y + offset.y;
        targetPos.x = Mathf.Lerp(targetPos.x, desiredX, speed * Time.deltaTime);
        targetPos.y = Mathf.Lerp(targetPos.y, desiredY, speed * Time.deltaTime);
        if (useBounds)
        {
            float minX = minLimit.x + camWidth;
            float maxX = maxLimit.x - camWidth;
            float minY = minLimit.y + camHeight;
            float maxY = maxLimit.y - camHeight;
            if (maxX < minX) targetPos.x = (minLimit.x + maxLimit.x) / 2;
            else targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            if (maxY < minY) targetPos.y = (minLimit.y + maxLimit.y) / 2;
            else targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }
}

