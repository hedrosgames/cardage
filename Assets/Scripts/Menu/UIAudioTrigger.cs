using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UIAudioTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public bool useDefaultSounds = true;
    [Header("Custom Sounds (Opcional)")]
    public AudioClip customHover;
    public AudioClip customClick;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ManagerAudio.Instance == null || ManagerAudio.Instance.audioConfig == null) return;
        AudioClip clip = useDefaultSounds ? ManagerAudio.Instance.audioConfig.uiHover : customHover;
        ManagerAudio.Instance.PlaySFX(clip);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ManagerAudio.Instance == null || ManagerAudio.Instance.audioConfig == null) return;
        AudioClip clip = useDefaultSounds ? ManagerAudio.Instance.audioConfig.uiClick : customClick;
        ManagerAudio.Instance.PlaySFX(clip);
    }
}

