using UnityEngine;
using UnityEngine.EventSystems;
public class SliderSaveHandler : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        SaveEvents.RaiseSave();
    }
}

