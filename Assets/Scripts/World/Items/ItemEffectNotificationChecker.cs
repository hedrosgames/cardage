using UnityEngine;
public abstract class ItemEffectNotificationChecker : ItemEffect
{
    protected SOZoneFlag notificationFlag;
    public override void Initialize(SOItemData item)
    {
        base.Initialize(item);
        if (item != null && item.itemFlag != null)
        {
            notificationFlag = item.itemFlag;
        }
    }
    public override void OnActivate()
    {
        if (ManagerNotification.Instance != null && notificationFlag != null)
        {
            if (!ManagerNotification.Instance.itemFlags.Contains(notificationFlag))
            {
                ManagerNotification.Instance.itemFlags.Add(notificationFlag);
            }
            ManagerNotification.Instance.RefreshNotifications();
        }
    }
    public override void OnDeactivate()
    {
        if (ManagerNotification.Instance != null && notificationFlag != null)
        {
            ManagerNotification.Instance.itemFlags.Remove(notificationFlag);
            ManagerNotification.Instance.RefreshNotifications();
        }
    }
}
public class ItemEffectDuelChecker : ItemEffectNotificationChecker
{
}
public class ItemEffectLostCardChecker : ItemEffectNotificationChecker
{
}
public class ItemEffectRarityChecker : ItemEffectNotificationChecker
{
}
public class ItemEffectTournament : ItemEffectNotificationChecker
{
}

