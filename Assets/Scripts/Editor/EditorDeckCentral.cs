using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class EditorDeckCentral : EditorWindow
{
    private List<SODeckData> allDecks = new List<SODeckData>();
    private SODeckData selectedDeck;
    private Vector2 deckListScroll;
    private Vector2 cardListScroll;
    private string searchFilter = "";
    private bool showOnlyDemo = false;
    
    private readonly HashSet<string> demoCardNames = new HashSet<string>
    {
        "Samurai fashion",
        "Samurai montado",
        "Samurai matador de ninja",
        "Samurai matador de oni",
        "Espadachim da lÃ¢mina cega",
        "Ultimo Samurai",
        "Ninja disfarce de vendedor",
        "Ninja da corda bamba",
        "Ninja fofoqueiro",
        "Ninja do bambu que balanÃ§a",
        "Mestre da Estrela de 4 pontas",
        "Ninja dos mil jutsus",
        "Monstro chorÃ£o",
        "Monstro da pele brilhante",
        "Monstro cego",
        "Monstro das asas pequenas",
        "Kappa da Ã¡gua fervente",
        "Criatura do pantano",
        "Kitsune de trÃªs caudas",
        "Xounin do imposto",
        "Espadachim Zen"
    };
    
    private readonly Dictionary<CardRarity, string> rarityEmojis = new Dictionary<CardRarity, string>
    {
        { CardRarity.Common, "âšª" },
        { CardRarity.Uncommon, "ðŸ”µ" },
        { CardRarity.Rare, "ðŸŸ£" },
        { CardRarity.Legendary, "ðŸŸ¡" },
        { CardRarity.Special, "â­" }
    };
    
    [MenuItem("Central de ConfiguraÃ§Ã£o/Central de Decks")]
    public static void ShowWindow()
    {
        EditorDeckCentral window = GetWindow<EditorDeckCentral>("Central de Decks");
        window.minSize = new Vector2(1000, 600);
        window.Show();
    }
    
    void OnEnable()
    {
        LoadAllDecks();
    }
    
    void LoadAllDecks()
    {
        allDecks.Clear();
        string[] guids = AssetDatabase.FindAssets("t:SODeckData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SODeckData deck = AssetDatabase.LoadAssetAtPath<SODeckData>(path);
            if (deck != null)
            {
                allDecks.Add(deck);
            }
        }
        allDecks = allDecks.OrderBy(d => d.name).ToList();
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Central de Decks", EditorStyles.boldLabel);
        if (GUILayout.Button("Recarregar", GUILayout.Width(100)))
        {
            LoadAllDecks();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Buscar:", GUILayout.Width(60));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = showOnlyDemo ? Color.cyan : Color.white;
        if (GUILayout.Button("Mostrar sÃ³ Demo", GUILayout.Height(25)))
        {
            showOnlyDemo = !showOnlyDemo;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.BeginVertical("box", GUILayout.Width(300));
        EditorGUILayout.LabelField($"Decks ({allDecks.Count})", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        deckListScroll = EditorGUILayout.BeginScrollView(deckListScroll);
        
        var filteredDecks = allDecks.Where(d => 
            string.IsNullOrEmpty(searchFilter) || 
            d.name.ToLower().Contains(searchFilter.ToLower())
        ).ToList();
        
        foreach (var deck in filteredDecks)
        {
            bool isSelected = selectedDeck == deck;
            GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
            
            if (GUILayout.Button(deck.name, EditorStyles.miniButton))
            {
                selectedDeck = deck;
                EditorUtility.SetDirty(deck);
            }
            
            GUI.backgroundColor = Color.white;
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
        
        if (selectedDeck != null)
        {
            EditorGUILayout.LabelField($"Deck Selecionado: {selectedDeck.name}", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Selecionar Asset", GUILayout.Width(150)))
            {
                Selection.activeObject = selectedDeck;
                EditorGUIUtility.PingObject(selectedDeck);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Cartas do Deck (5 slots)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            cardListScroll = EditorGUILayout.BeginScrollView(cardListScroll);
            
            if (selectedDeck.cards == null || selectedDeck.cards.Length != 5)
            {
                selectedDeck.cards = new SOCardData[5];
                EditorUtility.SetDirty(selectedDeck);
            }
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            DrawCardVisual(0, selectedDeck);
            DrawCardVisual(1, selectedDeck);
            DrawCardVisual(2, selectedDeck);
            DrawCardVisual(3, selectedDeck);
            DrawCardVisual(4, selectedDeck);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Selecione um deck da lista ao lado para visualizar e editar suas cartas.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }
    
    void DrawCardVisual(int slotIndex, SODeckData deck)
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(150), GUILayout.ExpandWidth(false));
        
        SOCardData currentCard = deck.cards[slotIndex];
        SOCardData newCard;
        
        if (showOnlyDemo)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(currentCard, typeof(SOCardData), false, GUILayout.Width(110));
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                ShowDemoCardSelector(slotIndex, deck);
            }
            EditorGUILayout.EndHorizontal();
            newCard = currentCard;
        }
        else
        {
            newCard = (SOCardData)EditorGUILayout.ObjectField(
                deck.cards[slotIndex], 
                typeof(SOCardData), 
                false,
                GUILayout.Width(140)
            );
        }
        
        if (newCard != deck.cards[slotIndex])
        {
            deck.cards[slotIndex] = newCard;
            EditorUtility.SetDirty(deck);
        }
        
        if (deck.cards[slotIndex] != null)
        {
            SOCardData card = deck.cards[slotIndex];
            string cardDisplayName = GetCardDisplayName(card);
            bool isDemoCard = IsDemoCard(cardDisplayName);
            string rarityEmoji = rarityEmojis.ContainsKey(card.rarity) ? rarityEmojis[card.rarity] : "";
            
            EditorGUILayout.LabelField($"â†‘{card.top} â†’{card.right} â†“{card.bottom} â†{card.left}", EditorStyles.miniLabel, GUILayout.Width(140));
            EditorGUILayout.LabelField($"{card.type} | {card.subType}", EditorStyles.miniLabel, GUILayout.Width(140));
            
            EditorGUILayout.BeginHorizontal(GUILayout.Width(140), GUILayout.ExpandWidth(false));
            string specialText = card.special != SpecialType.None ? card.special.ToString() : "Nenhum";
            Color originalSpecialColor = GUI.color;
            if (card.special != SpecialType.None)
            {
                GUI.color = GetSpecialColor(card.special);
            }
            EditorGUILayout.LabelField(specialText, EditorStyles.miniLabel, GUILayout.Width(60));
            GUI.color = originalSpecialColor;
            
            EditorGUILayout.LabelField("|", EditorStyles.miniLabel, GUILayout.Width(10));
            
            Color originalTriadColor = GUI.color;
            GUI.color = GetTriadColor(card.triad);
            EditorGUILayout.LabelField(card.triad.ToString(), EditorStyles.miniLabel, GUILayout.Width(60));
            GUI.color = originalTriadColor;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(GUILayout.Width(140), GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField($"{rarityEmoji} {card.rarity}", EditorStyles.miniLabel, GUILayout.Width(70));
            if (isDemoCard)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("âœ“ DEMO", EditorStyles.miniLabel, GUILayout.Width(60));
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.gray;
                EditorGUILayout.LabelField("NÃ£o Demo", EditorStyles.miniLabel, GUILayout.Width(60));
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("Vazio", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(140));
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void ShowCardSelector(int slotIndex, SODeckData deck)
    {
        GenericMenu menu = new GenericMenu();
        
        List<SOCardData> allCards = GetAllCards();
        allCards = allCards.OrderBy(c => c.cardIndex).ToList();
        
        foreach (var card in allCards)
        {
            string cardDisplayName = GetCardDisplayName(card);
            bool isDemoCard = IsDemoCard(cardDisplayName);
            string menuLabel = $"{card.cardIndex:D3} - {cardDisplayName}";
            
            if (isDemoCard)
            {
                menuLabel += " [DEMO]";
            }
            
            string rarityEmoji = rarityEmojis.ContainsKey(card.rarity) 
                ? rarityEmojis[card.rarity] 
                : "";
            
            menuLabel = $"{rarityEmoji} {menuLabel}";
            
            menu.AddItem(new GUIContent(menuLabel), false, () => 
            {
                deck.cards[slotIndex] = card;
                EditorUtility.SetDirty(deck);
            });
        }
        
        menu.ShowAsContext();
    }
    
    List<SOCardData> GetAllCards()
    {
        List<SOCardData> cards = new List<SOCardData>();
        string[] guids = AssetDatabase.FindAssets("t:SOCardData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SOCardData card = AssetDatabase.LoadAssetAtPath<SOCardData>(path);
            if (card != null)
            {
                cards.Add(card);
            }
        }
        return cards;
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
    
    bool IsDemoCard(string cardDisplayName)
    {
        return demoCardNames.Contains(cardDisplayName);
    }
    
    void ShowDemoCardSelector(int slotIndex, SODeckData deck)
    {
        GenericMenu menu = new GenericMenu();
        
        List<SOCardData> allCards = GetAllCards();
        List<SOCardData> demoCards = allCards.Where(c => 
        {
            string cardDisplayName = GetCardDisplayName(c);
            return IsDemoCard(cardDisplayName);
        }).OrderBy(c => c.cardIndex).ToList();
        
        menu.AddItem(new GUIContent("(Nenhum)"), deck.cards[slotIndex] == null, () => 
        {
            deck.cards[slotIndex] = null;
            EditorUtility.SetDirty(deck);
        });
        
        menu.AddSeparator("");
        
        foreach (var card in demoCards)
        {
            string cardDisplayName = GetCardDisplayName(card);
            string rarityEmoji = rarityEmojis.ContainsKey(card.rarity) ? rarityEmojis[card.rarity] : "";
            string menuLabel = $"{card.cardIndex:D3} - {cardDisplayName} [{rarityEmoji} {card.rarity}]";
            
            bool isSelected = deck.cards[slotIndex] == card;
            menu.AddItem(new GUIContent(menuLabel), isSelected, () => 
            {
                deck.cards[slotIndex] = card;
                EditorUtility.SetDirty(deck);
            });
        }
        
        menu.ShowAsContext();
    }
    
    Color GetSpecialColor(SpecialType special)
    {
        return special switch
        {
            SpecialType.Domain => new Color(0.9f, 0.7f, 0.1f),
            SpecialType.Camouflage => new Color(0.3f, 0.7f, 0.3f),
            SpecialType.Aura => new Color(0.7f, 0.3f, 0.9f),
            _ => Color.white
        };
    }
    
    Color GetTriadColor(TriadType triad)
    {
        return triad switch
        {
            TriadType.Power => new Color(0.9f, 0.2f, 0.2f),
            TriadType.Agility => new Color(0.2f, 0.8f, 0.3f),
            TriadType.Magic => new Color(0.2f, 0.5f, 0.9f),
            _ => Color.white
        };
    }
}
