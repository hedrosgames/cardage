using UnityEngine;

public enum CardRarity { Common, Uncommon, Rare, Legendary, Special }
public enum CardType { Samurai, Ninja, Monster }
public enum CardSubType { Creature, Equipment, Magic }
public enum CollectionType { FeudalJapan }
public enum SpecialType { None, Domain, Camouflage, Aura }
public enum TriadType { Power, Agility, Magic }

[CreateAssetMenu(menuName = "Game/Card")]
public class SOCardData : ScriptableObject
{
    public string cardName;
    public int cardIndex;
    public CardRarity rarity;
    public CardType type;
    public CardSubType subType;
    public CollectionType collection = CollectionType.FeudalJapan;
    public SpecialType special = SpecialType.None;
    public TriadType triad;
    public int top;
    public int right;
    public int bottom;
    public int left;
    public Sprite customArt;
}

