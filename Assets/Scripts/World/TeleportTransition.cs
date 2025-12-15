using System.Collections;
using UnityEngine;
public class TeleportTransition : MonoBehaviour
{
    [HideInInspector] public TeleportDoor teleportDoor;
    [HideInInspector] public ManagerCamera managerCamera;
    void Awake() { AutoSetup(); }
    #if UNITY_EDITOR
    void OnValidate() { AutoSetup(); }
    #endif
    void AutoSetup()
    {
        if (teleportDoor == null) teleportDoor = GetComponent<TeleportDoor>();
        if (managerCamera == null)
        {
            GameObject go = GameObject.Find("ManagerCamera") ?? GameObject.Find("_CameraManager");
            if (go == null)
            {
                var found = FindFirstObjectByType<ManagerCamera>();
                if(found) go = found.gameObject;
            }
            if (go != null) managerCamera = go.GetComponent<ManagerCamera>();
        }
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        if (teleportDoor == null || teleportDoor.IsBlocked) return;
        if (managerCamera != null && managerCamera.doorCurtain != null)
        {
            StartCoroutine(SequenceTransition(col.transform));
        }
        else
        {
            teleportDoor.TeleportPlayer(col.transform);
        }
    }
    IEnumerator SequenceTransition(Transform player)
    {
        SetPlayerMovement(player, false);
        yield return StartCoroutine(managerCamera.doorCurtain.PlayCloseRoutine());
        yield return new WaitForEndOfFrame();
        teleportDoor.TeleportPlayer(player);
        if (managerCamera.cameraFollow != null)
        {
            managerCamera.cameraFollow.enabled = false;
        }
        yield return new WaitForSeconds(0.15f);
        if (managerCamera.CurrentAreaFollowsPlayer && managerCamera.cameraFollow != null)
        {
            managerCamera.SnapToTarget();
            managerCamera.cameraFollow.enabled = true;
        }
        yield return StartCoroutine(managerCamera.doorCurtain.PlayOpenRoutine());
        SetPlayerMovement(player, true);
    }
    void SetPlayerMovement(Transform player, bool value)
    {
        var move = player.GetComponent<PlayerMove>();
        var run = player.GetComponent<PlayerRun>();
        var anim = player.GetComponentInChildren<Animator>();
        if (move != null) move.enabled = value;
        if (run != null) run.enabled = value;
        if (anim != null && !value)
        {
            anim.SetBool("isMoving", false);
            if (move != null)
            {
                anim.SetFloat("directionX", move.LastDirection.x);
                anim.SetFloat("directionY", move.LastDirection.y);
            }
        }
    }
}

