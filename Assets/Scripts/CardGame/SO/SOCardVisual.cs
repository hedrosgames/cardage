using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Config/CardVisual")]
public class SOCardVisual : ScriptableObject
{
    public Sprite cardBackground;
    public Sprite diamondHand;
    public Sprite diamondBoard;
    public Sprite art;
    public Sprite rarityBaseSprite;
    public Color rarityCommonColor = Color.white;
    public Color rarityUncommonColor = Color.white;
    public Color rarityRareColor = Color.white;
    public Color rarityLegendaryColor = Color.white;
    public Color raritySpecialColor = Color.white;
    public Sprite ornamentBaseSprite;
    public Color ornamentColor = Color.white;
    public Color playerColor = new Color(0.2f, 0.4f, 1f);
    public Color opponentColor = new Color(1f, 0.2f, 0.2f);
    [System.Serializable] public struct TypeIconEntry { public CardType type; public Sprite icon; }
    [System.Serializable] public struct CollectionIconEntry { public CollectionType type; public Sprite icon; }
    [System.Serializable] public struct SpecialIconEntry { public SpecialType type; public Sprite icon; }
    [System.Serializable] public struct TriadIconEntry { public TriadType type; public Sprite icon; }
    public List<TypeIconEntry> typeIcons = new List<TypeIconEntry>();
    public List<CollectionIconEntry> collectionIcons = new List<CollectionIconEntry>();
    public List<SpecialIconEntry> specialIcons = new List<SpecialIconEntry>();
    public List<TriadIconEntry> triadIcons = new List<TriadIconEntry>();
    public Color GetRarityColor(CardRarity rarity)
    {
        return rarity switch
        {
            CardRarity.Common => rarityCommonColor,
            CardRarity.Uncommon => rarityUncommonColor,
            CardRarity.Rare => rarityRareColor,
            CardRarity.Legendary => rarityLegendaryColor,
            CardRarity.Special => raritySpecialColor,
            _ => Color.white
        };
    }
    public Sprite GetTypeSprite(CardType type)
    {
        foreach (var entry in typeIcons)
        if (entry.type == type) return entry.icon;
        return null;
    }
    public Sprite GetCollectionSprite(CollectionType type)
    {
        foreach (var entry in collectionIcons)
        if (entry.type == type) return entry.icon;
        return null;
    }
    public Sprite GetSpecialSprite(SpecialType type)
    {
        foreach (var entry in specialIcons)
        if (entry.type == type) return entry.icon;
        return null;
    }
    public Sprite GetTriadSprite(TriadType type)
    {
        foreach (var entry in triadIcons)
        if (entry.type == type) return entry.icon;
        return null;
    }
}

