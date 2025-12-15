using UnityEngine;
using Game.World;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ManagerCamera : MonoBehaviour
{
    public CameraFollow cameraFollow;
    public Transform player;
    public SOCameraConfig config;
    public WorldAreaId startAreaId;
    [Header("Visual")]
    public DoorCurtain doorCurtain;
    [SerializeField] SOCameraConfig.AreaSettings currentArea;
    public SOCameraConfig.AreaSettings CurrentArea => currentArea;
    void Awake()
    {
        if (doorCurtain != null) doorCurtain.gameObject.SetActive(true);
        if (config == null) Debug.LogError("[ManagerCamera] Config ausente!");
    }
    void OnEnable() => GameEvents.OnPlayerTeleport += OnPlayerTeleport;
    void OnDisable() => GameEvents.OnPlayerTeleport -= OnPlayerTeleport;
    void Start()
    {
        if (startAreaId != WorldAreaId.None)
        SetArea(startAreaId);
    }
    void OnPlayerTeleport(Vector3 targetPos, WorldAreaId areaId)
    {
        if (areaId != WorldAreaId.None) SetArea(areaId);
    }
    public void SetArea(WorldAreaId areaId)
    {
        if (config == null) return;
        var area = config.GetArea(areaId);
        if (area == null)
        {
            return;
        }
        currentArea = area;
        ApplyArea();
    }
    public bool CurrentAreaFollowsPlayer => currentArea != null && currentArea.followPlayer;
    void ApplyArea()
    {
        if (cameraFollow == null) return;
        if (currentArea == null)
        {
            SnapToTarget();
            cameraFollow.enabled = true;
            cameraFollow.SetBounds(false, Vector2.zero, Vector2.zero);
            return;
        }
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;
        bool useBounds = false;
        if (currentArea.followPlayer && config.boundaryLibrary != null && currentArea.boundaryId != BoundaryId.None)
        {
            if (config.boundaryLibrary.GetBoundary(currentArea.boundaryId.ToString(), out min, out max))
            {
                useBounds = true;
            }
        }
        cameraFollow.SetBounds(useBounds, min, max);
        if (currentArea.followPlayer)
        {
            cameraFollow.target = player;
            cameraFollow.offset = config.globalFollowOffset;
            SnapToTarget();
            cameraFollow.enabled = true;
        }
        else
        {
            cameraFollow.enabled = false;
            cameraFollow.target = null;
            Vector3 pos = currentArea.fixedPosition;
            if (pos.z == 0) pos.z = cameraFollow.transform.position.z;
            cameraFollow.transform.position = pos;
        }
    }
    public void SnapToTarget()
    {
        if (player == null || cameraFollow == null) return;
        Vector3 targetPos = player.position;
        if (config != null)
        {
            targetPos.x += config.globalFollowOffset.x;
            targetPos.y += config.globalFollowOffset.y;
        }
        targetPos.z = cameraFollow.transform.position.z;
        if (cameraFollow.useBounds && cameraFollow.GetComponent<Camera>() != null)
        {
            Camera cam = cameraFollow.GetComponent<Camera>();
            float h = cam.orthographicSize;
            float w = h * cam.aspect;
            float minX = cameraFollow.minLimit.x + w;
            float maxX = cameraFollow.maxLimit.x - w;
            float minY = cameraFollow.minLimit.y + h;
            float maxY = cameraFollow.maxLimit.y - h;
            if (maxX >= minX) targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            else targetPos.x = (minX + maxX) / 2;
            if (maxY >= minY) targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
            else targetPos.y = (minY + maxY) / 2;
        }
        cameraFollow.transform.position = targetPos;
    }
    private void OnDrawGizmos()
    {
        if (config == null || config.areas == null) return;
        if (config.boundaryLibrary == null) return;
        float camHeight = 5f;
        float camWidth = 9f;
        if (cameraFollow != null)
        {
            var cam = cameraFollow.GetComponent<Camera>();
            if (cam != null)
            {
                camHeight = cam.orthographicSize;
                camWidth = camHeight * cam.aspect;
            }
        }
        foreach (var area in config.areas)
        {
            if (area != null && area.followPlayer && area.boundaryId != BoundaryId.None)
            {
                Vector2 min, max;
                if (config.boundaryLibrary.GetBoundary(area.boundaryId.ToString(), out min, out max))
                {
                    Vector3 center = (min + max) / 2f;
                    Vector3 size = max - min;
                    if (size.sqrMagnitude > 0.1f)
                    {
                        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                        Gizmos.DrawCube(center, size);
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(center, size);
                        float safeMinX = min.x + camWidth;
                        float safeMaxX = max.x - camWidth;
                        float safeMinY = min.y + camHeight;
                        float safeMaxY = max.y - camHeight;
                        float safeW = safeMaxX - safeMinX;
                        float safeH = safeMaxY - safeMinY;
                        Vector3 safeCenter = new Vector3((safeMinX + safeMaxX) / 2f, (safeMinY + safeMaxY) / 2f, 0);
                        Vector3 safeSize = new Vector3(Mathf.Max(0, safeW), Mathf.Max(0, safeH), 1f);
                        if (safeW < 0)
                        {
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(center + Vector3.left, center + Vector3.right);
                            Gizmos.DrawLine(center + Vector3.up, center + Vector3.down);
                            #if UNITY_EDITOR
                            Handles.Label(center + Vector3.up * 2, "LARGURA INSUFICIENTE!\nCâmera > Limite", new GUIStyle { normal = { textColor = Color.magenta }, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
                            #endif
                        }
                        else
                        {
                            Gizmos.color = Color.cyan;
                            Gizmos.DrawWireCube(safeCenter, safeSize);
                        }
                        #if UNITY_EDITOR
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.white;
                        style.alignment = TextAnchor.MiddleCenter;
                        Handles.Label(min, area.boundaryId.ToString(), style);
                        #endif
                    }
                }
            }
        }
    }
}

