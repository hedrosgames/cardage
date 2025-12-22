using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class EditorCardCentral : EditorWindow
{
    private List<SOCardData> allCards = new List<SOCardData>();
    private Vector2 scrollPosition;
    private string searchFilter = "";
    private CollectionType collectionFilter = (CollectionType)(-1);
    private CardRarity rarityFilter = (CardRarity)(-1);
    private CardType typeFilter = (CardType)(-1);
    private CardSubType subTypeFilter = (CardSubType)(-1);
    
    private readonly Color samuraiColor = new Color(1f, 0.7f, 0.1f, 0.4f);
    private readonly Color ninjaColor = new Color(0.5f, 0.2f, 0.9f, 0.4f);
    private readonly Color monsterColor = new Color(0.6f, 0.35f, 0.15f, 0.4f);
    private readonly Color powerColor = new Color(0.9f, 0.2f, 0.2f);
    private readonly Color agilityColor = new Color(0.2f, 0.8f, 0.3f);
    private readonly Color magicColor = new Color(0.2f, 0.5f, 0.9f);
    private readonly Color domainColor = new Color(0.9f, 0.7f, 0.1f);
    private readonly Color camouflageColor = new Color(0.3f, 0.7f, 0.3f);
    private readonly Color auraColor = new Color(0.7f, 0.3f, 0.9f);
    
    [MenuItem("Central de ConfiguraÃ§Ã£o/Central de Cartas")]
    public static void ShowWindow()
    {
        EditorCardCentral window = GetWindow<EditorCardCentral>("Central de Cartas");
        window.minSize = new Vector2(1200, 600);
        window.Show();
    }
    
    void OnEnable()
    {
        LoadAllCards();
    }
    
    void LoadAllCards()
    {
        allCards.Clear();
        string[] guids = AssetDatabase.FindAssets("t:SOCardData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SOCardData card = AssetDatabase.LoadAssetAtPath<SOCardData>(path);
            if (card != null)
            {
                allCards.Add(card);
            }
        }
        allCards = allCards.OrderBy(c => c.cardIndex).ToList();
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Central de Cartas", EditorStyles.boldLabel);
        if (GUILayout.Button("Recarregar", GUILayout.Width(100)))
        {
            LoadAllCards();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Buscar:", GUILayout.Width(60));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Filtros", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ColeÃ§Ã£o:", GUILayout.Width(70));
        bool feudalJapanSelected = collectionFilter == CollectionType.FeudalJapan;
        GUI.backgroundColor = feudalJapanSelected ? Color.cyan : Color.white;
        if (GUILayout.Button("JapÃ£o Feudal", GUILayout.Height(20)))
        {
            collectionFilter = feudalJapanSelected ? (CollectionType)(-1) : CollectionType.FeudalJapan;
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Tipo:", GUILayout.Width(50));
        foreach (CardType type in System.Enum.GetValues(typeof(CardType)))
        {
            bool isSelected = typeFilter == type;
            GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
            string buttonText = GetTypeEmoji(type) + " " + type.ToString();
            if (GUILayout.Button(buttonText, GUILayout.Height(20)))
            {
                typeFilter = isSelected ? (CardType)(-1) : type;
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Raridade:", GUILayout.Width(70));
        foreach (CardRarity rarity in System.Enum.GetValues(typeof(CardRarity)))
        {
            bool isSelected = rarityFilter == rarity;
            GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
            string buttonText = GetRarityEmoji(rarity) + " " + rarity.ToString();
            if (GUILayout.Button(buttonText, GUILayout.Height(20)))
            {
                rarityFilter = isSelected ? (CardRarity)(-1) : rarity;
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Subtipo:", GUILayout.Width(70));
        foreach (CardSubType subType in System.Enum.GetValues(typeof(CardSubType)))
        {
            bool isSelected = subTypeFilter == subType;
            GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
            string buttonText = GetSubTypeEmoji(subType) + " " + subType.ToString();
            if (GUILayout.Button(buttonText, GUILayout.Height(20)))
            {
                subTypeFilter = isSelected ? (CardSubType)(-1) : subType;
            }
        }
        GUI.backgroundColor = Color.white;
        
        if (GUILayout.Button("Limpar Todos", GUILayout.Width(100), GUILayout.Height(20)))
        {
            collectionFilter = (CollectionType)(-1);
            typeFilter = (CardType)(-1);
            rarityFilter = (CardRarity)(-1);
            subTypeFilter = (CardSubType)(-1);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        var filteredCards = GetFilteredCards();
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField($"Total: {allCards.Count}", GUILayout.Width(100));
        EditorGUILayout.LabelField($"Filtradas: {filteredCards.Count}", GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("ID", EditorStyles.boldLabel, GUILayout.Width(50));
        EditorGUILayout.LabelField("Nome", EditorStyles.boldLabel, GUILayout.Width(250));
        EditorGUILayout.LabelField("Raridade", EditorStyles.boldLabel, GUILayout.Width(100));
        EditorGUILayout.LabelField("Tipo", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.LabelField("Subtipo", EditorStyles.boldLabel, GUILayout.Width(90));
        EditorGUILayout.LabelField("Especial", EditorStyles.boldLabel, GUILayout.Width(100));
        EditorGUILayout.LabelField("Triad", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.LabelField("Top", EditorStyles.boldLabel, GUILayout.Width(50));
        EditorGUILayout.LabelField("Right", EditorStyles.boldLabel, GUILayout.Width(50));
        EditorGUILayout.LabelField("Bottom", EditorStyles.boldLabel, GUILayout.Width(60));
        EditorGUILayout.LabelField("Left", EditorStyles.boldLabel, GUILayout.Width(50));
        EditorGUILayout.LabelField("AÃ§Ãµes", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();
        
        foreach (var card in filteredCards)
        {
            DrawCardRow(card);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    string GetRarityEmoji(CardRarity rarity)
    {
        return rarity switch
        {
            CardRarity.Common => "âšª",
            CardRarity.Uncommon => "ðŸ”µ",
            CardRarity.Rare => "ðŸŸ£",
            CardRarity.Legendary => "ðŸŸ¡",
            CardRarity.Special => "â­",
            _ => ""
        };
    }
    
    string GetTypeEmoji(CardType type)
    {
        return type switch
        {
            CardType.Samurai => "âš”ï¸",
            CardType.Ninja => "ðŸ¥·",
            CardType.Monster => "ðŸ‘¹",
            _ => ""
        };
    }
    
    string GetSubTypeEmoji(CardSubType subType)
    {
        return subType switch
        {
            CardSubType.Creature => "ðŸ‘¤",
            CardSubType.Equipment => "ðŸ›¡ï¸",
            CardSubType.Magic => "âœ¨",
            _ => ""
        };
    }
    
    string GetCardDisplayName(SOCardData card)
    {
        string assetName = card.name;
        int underscoreIndex = assetName.IndexOf('_');
        if (underscoreIndex >= 0 && underscoreIndex < assetName.Length - 1)
        {
            return assetName.Substring(underscoreIndex + 1);
        }
        return assetName;
    }
    
    Color GetTypeColor(CardType type)
    {
        return type switch
        {
            CardType.Samurai => samuraiColor,
            CardType.Ninja => ninjaColor,
            CardType.Monster => monsterColor,
            _ => Color.clear
        };
    }
    
    Color GetTriadColor(TriadType triad)
    {
        return triad switch
        {
            TriadType.Power => powerColor,
            TriadType.Agility => agilityColor,
            TriadType.Magic => magicColor,
            _ => Color.white
        };
    }
    
    Color GetSpecialColor(SpecialType special)
    {
        return special switch
        {
            SpecialType.Domain => domainColor,
            SpecialType.Camouflage => camouflageColor,
            SpecialType.Aura => auraColor,
            _ => Color.white
        };
    }
    
    List<SOCardData> GetFilteredCards()
    {
        var filtered = allCards.Where(c => c != null).ToList();
        
        if (!string.IsNullOrEmpty(searchFilter))
        {
            filtered = filtered.Where(c => 
                GetCardDisplayName(c).ToLower().Contains(searchFilter.ToLower()) ||
                c.cardIndex.ToString().Contains(searchFilter) ||
                c.cardName.ToLower().Contains(searchFilter.ToLower())
            ).ToList();
        }
        
        if (collectionFilter != (CollectionType)(-1))
        {
            filtered = filtered.Where(c => c.collection == collectionFilter).ToList();
        }
        
        if (rarityFilter != (CardRarity)(-1))
        {
            filtered = filtered.Where(c => c.rarity == rarityFilter).ToList();
        }
        
        if (typeFilter != (CardType)(-1))
        {
            filtered = filtered.Where(c => c.type == typeFilter).ToList();
        }
        
        if (subTypeFilter != (CardSubType)(-1))
        {
            filtered = filtered.Where(c => c.subType == subTypeFilter).ToList();
        }
        
        return filtered;
    }
    
    void DrawCardRow(SOCardData card)
    {
        Color backgroundColor = GetTypeColor(card.type);
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = backgroundColor;
        
        EditorGUILayout.BeginHorizontal("box");
        GUI.backgroundColor = originalColor;
        
        EditorGUILayout.LabelField(card.cardIndex.ToString(), GUILayout.Width(50));
        EditorGUILayout.LabelField(GetCardDisplayName(card), GUILayout.Width(250));
        
        string rarityText = GetRarityEmoji(card.rarity) + " " + card.rarity.ToString();
        EditorGUILayout.LabelField(rarityText, GUILayout.Width(100));
        
        string typeText = GetTypeEmoji(card.type) + " " + card.type.ToString();
        EditorGUILayout.LabelField(typeText, GUILayout.Width(80));
        
        string subTypeText = GetSubTypeEmoji(card.subType) + " " + card.subType.ToString();
        EditorGUILayout.LabelField(subTypeText, GUILayout.Width(90));
        
        Color originalSpecialColor = GUI.color;
        GUI.color = GetSpecialColor(card.special);
        EditorGUILayout.LabelField(card.special.ToString(), GUILayout.Width(100));
        GUI.color = originalSpecialColor;
        
        Color originalTextColor = GUI.color;
        GUI.color = GetTriadColor(card.triad);
        EditorGUILayout.LabelField(card.triad.ToString(), GUILayout.Width(80));
        GUI.color = originalTextColor;
        
        EditorGUILayout.LabelField(card.top.ToString(), GUILayout.Width(50));
        EditorGUILayout.LabelField(card.right.ToString(), GUILayout.Width(50));
        EditorGUILayout.LabelField(card.bottom.ToString(), GUILayout.Width(60));
        EditorGUILayout.LabelField(card.left.ToString(), GUILayout.Width(50));
        
        if (GUILayout.Button("Selecionar", GUILayout.Width(70)))
        {
            Selection.activeObject = card;
            EditorGUIUtility.PingObject(card);
        }
        
        EditorGUILayout.EndHorizontal();
    }
}
