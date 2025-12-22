using UnityEngine;
[CreateAssetMenu(menuName = "Game/Item")]
public class SOItemData : ScriptableObject
{
    [Header("Identificação")]
    public string itemName;
    public string description;
    [Header("Flag")]
    public SOZoneFlag itemFlag;
    [Header("Efeito")]
    public ItemEffectType effectType;
    public MonoBehaviour effectScript;
    [Header("UI")]
    public Sprite itemIcon;
}
public enum ItemEffectType
{
    None,
    SpeedBoots,
    DuelChecker,
    LostCardChecker,
    RarityChecker,
    Tournament,
    Map
}

