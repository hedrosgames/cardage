using UnityEngine;
[CreateAssetMenu(menuName = "Game/Item")]
public class SOItemData : ScriptableObject
{
    [Header("IdentificaÃ§Ã£o")]
    public string itemName;
    [TextArea(3, 5)]
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

