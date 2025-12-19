using UnityEngine;
public abstract class ItemEffect : MonoBehaviour
{
    protected SOItemData itemData;
    public virtual void Initialize(SOItemData item)
    {
        itemData = item;
    }
    public abstract void OnActivate();
    public abstract void OnDeactivate();
}

