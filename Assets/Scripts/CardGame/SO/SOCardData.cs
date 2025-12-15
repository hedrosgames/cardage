using UnityEngine;
public enum CardRarity { Common, Uncommon, Rare, Legendary }
public enum ElementType { None, Fire, Water, Electric, Plant, Earth, Wind, Dark, Light, Ice, Poison, Metal }
public enum CollectionType { None, Egypt }
public enum SpecialType { None, Attack, Defense }
public enum OtherType { None, Other }
[CreateAssetMenu(menuName = "Game/Card")]
public class SOCardData : ScriptableObject
{
    public string cardName;
    public CardRarity rarity;
    public ElementType element = ElementType.None;
    public CollectionType collection = CollectionType.None;
    public SpecialType special = SpecialType.None;
    public OtherType other = OtherType.None;
    public int top;
    public int right;
    public int bottom;
    public int left;
    public int power;
    public Sprite customArt;
    private void OnValidate()
    {
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(cardName) || cardName != name)
        {
            cardName = name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
}

