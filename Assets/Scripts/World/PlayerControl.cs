using UnityEngine;
public class PlayerControl : MonoBehaviour
{
    public PlayerMove playerMove;
    public PlayerRun playerRun;
    public PlayerInteract playerInteract;
    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerRun = GetComponent<PlayerRun>();
        playerInteract = GetComponent<PlayerInteract>();
    }
    public void SetControl(bool enabledControl)
    {
        SetMovement(enabledControl);
        SetInteraction(enabledControl);
    }
    public void SetMovement(bool active)
    {
        if (playerMove != null) playerMove.enabled = active;
        if (playerRun != null) playerRun.enabled = active;
    }
    public void SetInteraction(bool active)
    {
        if (playerInteract != null) playerInteract.enabled = active;
    }
}

