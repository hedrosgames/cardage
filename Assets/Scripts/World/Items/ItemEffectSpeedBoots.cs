using UnityEngine;
public class ItemEffectSpeedBoots : ItemEffect
{
    private PlayerRun playerRun;
    private bool wasPlayerRunEnabled = false;
    public override void OnActivate()
    {
        if (itemData == null) return;
        PlayerControl playerControl = FindFirstObjectByType<PlayerControl>();
        if (playerControl != null && playerControl.playerRun != null)
        {
            playerRun = playerControl.playerRun;
            wasPlayerRunEnabled = playerRun.enabled;
            playerRun.enabled = true;
        }
    }
    public override void OnDeactivate()
    {
        if (playerRun != null)
        {
            playerRun.enabled = wasPlayerRunEnabled;
        }
        playerRun = null;
    }
}

