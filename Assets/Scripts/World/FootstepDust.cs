using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class FootstepDust : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Distância mínima percorrida no frame para emitir fumaça.")]
    public float movementThreshold = 0.001f;
    [Tooltip("Se verdadeiro, a fumaça para se o tempo estiver pausado (Time.timeScale = 0)")]
    public bool respectTimeScale = true;
    private ParticleSystem ps;
    private ParticleSystem.EmissionModule emission;
    private Vector3 lastPosition;
    private Transform targetTransform;
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        emission = ps.emission;
        emission.enabled = false;
    }
    void Start()
    {
        targetTransform = transform.parent != null ? transform.parent : transform;
        lastPosition = targetTransform.position;
    }
    void LateUpdate()
    {
        if (respectTimeScale && Time.timeScale <= 0)
        {
            emission.enabled = false;
            return;
        }
        float distanceMoved = (targetTransform.position - lastPosition).sqrMagnitude;
        bool isMoving = distanceMoved > (movementThreshold * movementThreshold);
        emission.enabled = isMoving;
        lastPosition = targetTransform.position;
    }
}

