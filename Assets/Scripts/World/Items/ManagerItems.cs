using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class ManagerItems : MonoBehaviour
{
    public static ManagerItems Instance { get; private set; }
    [Header("Configuração")]
    public List<SOItemData> allItems = new List<SOItemData>();
    private SaveClientZone saveZone;
    private Dictionary<SOItemData, bool> itemActiveStates = new Dictionary<SOItemData, bool>();
    private Dictionary<SOItemData, ItemEffect> activeEffects = new Dictionary<SOItemData, ItemEffect>();
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        saveZone = FindFirstObjectByType<SaveClientZone>();
        if (saveZone != null)
        {
            saveZone.OnFlagChanged += OnFlagChanged;
            saveZone.OnLoadComplete += OnSaveLoaded;
        }
        RefreshAllItemStates();
    }
    void OnDestroy()
    {
        if (saveZone != null)
        {
            saveZone.OnFlagChanged -= OnFlagChanged;
            saveZone.OnLoadComplete -= OnSaveLoaded;
        }
    }
    void OnFlagChanged(SOZoneFlag flag)
    {
        foreach (var item in allItems)
        {
            if (item != null && item.itemFlag != null && item.itemFlag.id == flag.id)
            {
                RefreshItemState(item);
            }
        }
    }
    void OnSaveLoaded()
    {
        RefreshAllItemStates();
    }
    void RefreshAllItemStates()
    {
        foreach (var item in allItems)
        {
            if (item != null)
            {
                RefreshItemState(item);
            }
        }
    }
    void RefreshItemState(SOItemData item)
    {
        if (item == null || item.itemFlag == null || saveZone == null) return;
        bool isActive = saveZone.HasFlag(item.itemFlag);
        bool wasActive = itemActiveStates.ContainsKey(item) && itemActiveStates[item];
        itemActiveStates[item] = isActive;
        if (wasActive != isActive)
        {
            if (isActive)
            {
                ActivateItemEffect(item);
            }
            else
            {
                DeactivateItemEffect(item);
            }
        }
    }
    void ActivateItemEffect(SOItemData item)
    {
        if (item == null) return;
        if (activeEffects.ContainsKey(item))
        {
            DeactivateItemEffect(item);
        }
        ItemEffect effect = CreateItemEffect(item);
        if (effect != null)
        {
            activeEffects[item] = effect;
            effect.OnActivate();
        }
    }
    void DeactivateItemEffect(SOItemData item)
    {
        if (item == null) return;
        if (activeEffects.ContainsKey(item))
        {
            activeEffects[item].OnDeactivate();
            Destroy(activeEffects[item]);
            activeEffects.Remove(item);
        }
    }
    ItemEffect CreateItemEffect(SOItemData item)
    {
        if (item == null) return null;
        GameObject effectObject = new GameObject($"ItemEffect_{item.itemName}");
        effectObject.transform.SetParent(transform);
        ItemEffect effect = null;
        switch (item.effectType)
        {
            case ItemEffectType.SpeedBoots:
            effect = effectObject.AddComponent<ItemEffectSpeedBoots>();
            break;
            case ItemEffectType.DuelChecker:
            effect = effectObject.AddComponent<ItemEffectDuelChecker>();
            break;
            case ItemEffectType.LostCardChecker:
            effect = effectObject.AddComponent<ItemEffectLostCardChecker>();
            break;
            case ItemEffectType.RarityChecker:
            effect = effectObject.AddComponent<ItemEffectRarityChecker>();
            break;
            case ItemEffectType.Tournament:
            effect = effectObject.AddComponent<ItemEffectTournament>();
            break;
            case ItemEffectType.Map:
            effect = effectObject.AddComponent<ItemEffectMap>();
            break;
            default:
            Destroy(effectObject);
            return null;
        }
        if (effect != null)
        {
            effect.Initialize(item);
        }
        return effect;
    }
    public bool HasItem(SOItemData item)
    {
        if (item == null || item.itemFlag == null || saveZone == null) return false;
        return saveZone.HasFlag(item.itemFlag);
    }
    public bool IsItemActive(SOItemData item)
    {
        if (item == null) return false;
        return itemActiveStates.ContainsKey(item) && itemActiveStates[item];
    }
    public void SetItemActive(SOItemData item, bool active)
    {
        if (item == null || item.itemFlag == null || saveZone == null) return;
        saveZone.SetFlag(item.itemFlag, active ? 1 : 0);
        RefreshItemState(item);
    }
    public List<SOItemData> GetOwnedItems()
    {
        List<SOItemData> owned = new List<SOItemData>();
        foreach (var item in allItems)
        {
            if (item != null && HasItem(item))
            {
                owned.Add(item);
            }
        }
        return owned;
    }
}

