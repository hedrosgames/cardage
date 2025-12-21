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
            StartCoroutine(NotifyCurtainOpenedWhenNoCurtain());
        }
    }
    IEnumerator NotifyCurtainOpenedWhenNoCurtain()
    {
        yield return null;
        yield return null;
        yield return null;
        if (managerCamera != null)
        {
            Game.World.WorldAreaId areaId = Game.World.WorldAreaId.None;
            if (managerCamera.CurrentArea != null &&
            managerCamera.CurrentArea.id != Game.World.WorldAreaId.None)
            {
                areaId = managerCamera.CurrentArea.id;
            }
            else if (managerCamera.startAreaId != Game.World.WorldAreaId.None)
            {
                areaId = managerCamera.startAreaId;
            }
            if (areaId != Game.World.WorldAreaId.None)
            {
                GameEvents.OnCurtainOpenedAfterTeleport?.Invoke(areaId);
            }
        }
    }
    IEnumerator SequenceTransition(Transform player)
    {
        if (player == null)
        {
            yield break;
        }
        if (managerCamera == null)
        {
            yield break;
        }
        if (teleportDoor == null)
        {
            yield break;
        }
        SetPlayerMovement(player, false);
        yield return StartCoroutine(managerCamera.doorCurtain.PlayCloseRoutine());
        yield return new WaitForEndOfFrame();
        if (managerCamera.cameraFollow != null)
        {
            managerCamera.cameraFollow.enabled = false;
        }
        else
        {
        }
        teleportDoor.TeleportPlayer(player);
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.05f);
        if (managerCamera != null)
        {
            if (managerCamera.CurrentArea != null)
            {
                managerCamera.ApplyArea();
                if (managerCamera.CurrentAreaFollowsPlayer)
                {
                    managerCamera.SnapToTarget();
                }
                else
                {
                }
            }
            else
            {
                if (managerCamera.player == null)
                {
                    managerCamera.player = player;
                }
                yield return null;
                if (managerCamera.CurrentArea != null)
                {
                    managerCamera.ApplyArea();
                    if (managerCamera.CurrentAreaFollowsPlayer)
                    {
                        managerCamera.SnapToTarget();
                    }
                }
                else
                {
                }
            }
        }
        else
        {
        }
        yield return StartCoroutine(managerCamera.doorCurtain.PlayOpenRoutine());
        SetPlayerMovement(player, true);
        Game.World.WorldAreaId areaId = Game.World.WorldAreaId.None;
        if (managerCamera != null)
        {
            if (managerCamera.CurrentArea != null &&
            managerCamera.CurrentArea.id != Game.World.WorldAreaId.None)
            {
                areaId = managerCamera.CurrentArea.id;
            }
            else if (managerCamera.startAreaId != Game.World.WorldAreaId.None)
            {
                areaId = managerCamera.startAreaId;
            }
        }
        if (areaId == Game.World.WorldAreaId.None && teleportDoor != null)
        {
            Game.World.WorldAreaId targetID = teleportDoor.destination;
            if (targetID != Game.World.WorldAreaId.None)
            {
                areaId = targetID;
            }
        }
        if (areaId != Game.World.WorldAreaId.None)
        {
            GameEvents.OnCurtainOpenedAfterTeleport?.Invoke(areaId);
        }
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

