using UnityEngine;
using System.Collections.Generic;
public enum CardRarity { Common, Uncommon, Rare, Legendary, Special }
public enum CardType { Samurai, Ninja, Monster }
public enum CardSubType { Creature, Equipment, Magic }
public enum CollectionType { FeudalJapan }
public enum SpecialType { None, Domain, Camouflage, Aura }
public enum TriadType { Power, Agility, Magic }
public enum CardGeneralRule { None, Attack, Defense, Protection, Aura, Corruption, Wear, Sacrifice, Echo, Retaliation, Territory, TerritoryStrong, Stealth, CenterStrong, CornerStrong, SideStrong, Bonus1, Bonus2, Penalty1, Penalty2, Betrayal }
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
    [Header("Regras Gerais")]
    public List<CardGeneralRule> generalRules = new List<CardGeneralRule>();
}

