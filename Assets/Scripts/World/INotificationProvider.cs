using UnityEngine;
public interface INotificationProvider
{
    NotificationType GetNotificationType();
    SpriteRenderer GetImgAlert();
}

