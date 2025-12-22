using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class FootstepDust : MonoBehaviour
{
    [Header("ConfiguraÃ§Ã£o")]
    public float movementThreshold = 0.001f;
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

