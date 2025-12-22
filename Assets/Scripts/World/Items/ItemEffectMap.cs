using UnityEngine;
public class ItemEffectMap : ItemEffect
{
    [Header("ConfiguraÃ§Ã£o")]
    public SOZoneFlag mapAppUnlockFlag;
    private SaveClientZone saveZone;
    public override void Initialize(SOItemData item)
    {
        base.Initialize(item);
        saveZone = FindFirstObjectByType<SaveClientZone>();
    }
    public override void OnActivate()
    {
        if (saveZone != null && mapAppUnlockFlag != null)
        {
            saveZone.SetFlag(mapAppUnlockFlag, 1);
        }
    }
    public override void OnDeactivate()
    {
    }
}

