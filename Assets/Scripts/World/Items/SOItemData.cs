using UnityEngine;
[CreateAssetMenu(menuName = "Game/Item")]
public class SOItemData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("Nome do item")]
    public string itemName;
    [Tooltip("Descrição do item")]
    [TextArea(3, 5)]
    public string description;
    [Header("Flag")]
    [Tooltip("Flag que indica se o jogador possui este item")]
    public SOZoneFlag itemFlag;
    [Header("Efeito")]
    [Tooltip("Tipo de efeito que este item aplica")]
    public ItemEffectType effectType;
    [Tooltip("Script de efeito customizado (opcional, para efeitos específicos)")]
    public MonoBehaviour effectScript;
    [Header("UI")]
    [Tooltip("Ícone do item para exibir no Drive")]
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

