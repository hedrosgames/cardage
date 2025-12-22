using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
public class EditorCardGenerator : EditorWindow
{
    private string csvPath = "";
    private string outputPath = "Assets/Data/Cards";
    private Vector2 scrollPosition;
    [MenuItem("Tools/Gerador de Cartas")]
    public static void ShowWindow()
    {
        EditorCardGenerator window = GetWindow<EditorCardGenerator>("Gerador de Cartas");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("Gerador de Cartas do CSV", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Caminho do CSV:", GUILayout.Width(120));
        csvPath = EditorGUILayout.TextField(csvPath);
        if (GUILayout.Button("Procurar", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Selecione o arquivo CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                csvPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pasta de Saída:", GUILayout.Width(120));
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Procurar", GUILayout.Width(80)))
        {
            string path = EditorUtility.SaveFolderPanel("Selecione a pasta de saída", outputPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    outputPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Gerar Todas as Cartas", GUILayout.Height(30)))
        {
            GenerateCards();
        }
        if (GUILayout.Button("Deletar Cartas Antigas", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirmar",
            "Isso vai deletar todas as cartas antigas (Card_*.asset).\n\nTem certeza?",
            "Sim, deletar", "Cancelar"))
            {
                DeleteOldCards();
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Instruções:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
        "1. Selecione o arquivo CSV com os dados das cartas\n" +
        "2. Escolha a pasta onde os ScriptableObjects serão criados\n" +
        "3. Clique em 'Gerar Todas as Cartas'\n\n" +
        "O CSV deve ter as colunas: id, Type, Rarity, SubType, Name, Top., Rig., Down, Left, Triad, Special",
        MessageType.Info
        );
    }
    void GenerateCards()
    {
        if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
        {
            EditorUtility.DisplayDialog("Erro", "Por favor, selecione um arquivo CSV válido.", "OK");
            return;
        }
        if (string.IsNullOrEmpty(outputPath))
        {
            EditorUtility.DisplayDialog("Erro", "Por favor, selecione uma pasta de saída.", "OK");
            return;
        }
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        string[] lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            EditorUtility.DisplayDialog("Erro", "O arquivo CSV está vazio ou não tem dados.", "OK");
            return;
        }
        int created = 0;
        int updated = 0;
        int errors = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string[] values = ParseCSVLine(line);
                if (values.Length < 11)
                {
                    Debug.LogWarning($"Linha {i + 1} tem menos de 11 colunas: {values.Length}");
                    continue;
                }
                string idStr = values[0].Trim();
                string typeStr = values[1].Trim();
                string rarityStr = values[2].Trim();
                string subTypeStr = values[3].Trim();
                string name = values[4].Trim();
                string topStr = values[5].Trim();
                string rightStr = values[6].Trim();
                string downStr = values[7].Trim();
                string leftStr = values[8].Trim();
                string triadStr = values[9].Trim();
                string specialStr = values.Length > 10 ? values[10].Trim() : "";
                int cardIndex = 0;
                if (idStr.StartsWith("#"))
                {
                    int.TryParse(idStr.Substring(1), out cardIndex);
                }
                else
                {
                    int.TryParse(idStr, out cardIndex);
                }
                if (cardIndex == 0) continue;
                CardType type = ParseCardType(typeStr);
                CardRarity rarity = ParseCardRarity(rarityStr);
                CardSubType subType = ParseCardSubType(subTypeStr);
                CollectionType collection = CollectionType.FeudalJapan;
                SpecialType special = ParseSpecialType(specialStr);
                TriadType triad = ParseTriadType(triadStr);
                int top = int.TryParse(topStr, out int t) ? t : 0;
                int right = int.TryParse(rightStr, out int r) ? r : 0;
                int bottom = int.TryParse(downStr, out int d) ? d : 0;
                int left = int.TryParse(leftStr, out int l) ? l : 0;
                string sanitizedName = SanitizeFileName(name);
                string fileName = $"{cardIndex:D3}_{sanitizedName}";
                string assetPath = $"{outputPath}/{fileName}.asset";
                string localizationId = $"CARD_NAME_{cardIndex}";
                SOCardData card = AssetDatabase.LoadAssetAtPath<SOCardData>(assetPath);
                if (card == null)
                {
                    card = ScriptableObject.CreateInstance<SOCardData>();
                    AssetDatabase.CreateAsset(card, assetPath);
                    created++;
                }
                else
                {
                    updated++;
                }
                card.cardIndex = cardIndex;
                card.cardName = localizationId;
                card.rarity = rarity;
                card.type = type;
                card.subType = subType;
                card.collection = collection;
                card.special = special;
                card.triad = triad;
                card.top = top;
                card.right = right;
                card.bottom = bottom;
                card.left = left;
                card.name = fileName;
                EditorUtility.SetDirty(card);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao processar linha {i + 1}: {e.Message}");
                errors++;
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
        "Concluído",
        $"Cartas geradas com sucesso!\n\n" +
        $"Criadas: {created}\n" +
        $"Atualizadas: {updated}\n" +
        $"Erros: {errors}",
        "OK"
        );
    }
    string[] ParseCSVLine(string line)
    {
        System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
        bool inQuotes = false;
        string current = "";
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (i + 1 < line.Length && line[i + 1] == '"' && inQuotes)
                {
                    current += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }
        result.Add(current.Trim());
        return result.ToArray();
    }
    CardType ParseCardType(string str)
    {
        str = str.ToLower().Trim();
        if (str.Contains("samurai")) return CardType.Samurai;
        if (str.Contains("ninja")) return CardType.Ninja;
        if (str.Contains("monstro")) return CardType.Monster;
        return CardType.Samurai;
    }
    CardRarity ParseCardRarity(string str)
    {
        str = str.ToLower().Trim();
        if (str.Contains("comum")) return CardRarity.Common;
        if (str.Contains("incomum")) return CardRarity.Uncommon;
        if (str.Contains("rara")) return CardRarity.Rare;
        if (str.Contains("lendária") || str.Contains("lendaria")) return CardRarity.Legendary;
        if (str.Contains("especial")) return CardRarity.Special;
        return CardRarity.Common;
    }
    CardSubType ParseCardSubType(string str)
    {
        str = str.ToLower().Trim();
        if (str.Contains("criatura")) return CardSubType.Creature;
        if (str.Contains("equipamento")) return CardSubType.Equipment;
        if (str.Contains("magia")) return CardSubType.Magic;
        return CardSubType.Creature;
    }
    SpecialType ParseSpecialType(string str)
    {
        if (string.IsNullOrEmpty(str)) return SpecialType.None;
        str = str.ToLower().Trim();
        if (str.Contains("domínio") || str.Contains("dominio")) return SpecialType.Domain;
        if (str.Contains("camuflagem")) return SpecialType.Camouflage;
        if (str.Contains("aura")) return SpecialType.Aura;
        return SpecialType.None;
    }
    TriadType ParseTriadType(string str)
    {
        str = str.ToLower().Trim();
        if (str.Contains("poder")) return TriadType.Power;
        if (str.Contains("agilidade")) return TriadType.Agility;
        if (str.Contains("magia")) return TriadType.Magic;
        return TriadType.Power;
    }
    string SanitizeFileName(string fileName)
    {
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }
        while (fileName.Contains("  "))
        {
            fileName = fileName.Replace("  ", " ");
        }
        return fileName.Trim();
    }
    void DeleteOldCards()
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            EditorUtility.DisplayDialog("Erro", "Por favor, selecione uma pasta primeiro.", "OK");
            return;
        }
        if (!Directory.Exists(outputPath))
        {
            EditorUtility.DisplayDialog("Info", "A pasta não existe. Nada para deletar.", "OK");
            return;
        }
        string[] guids = AssetDatabase.FindAssets("t:SOCardData", new[] { outputPath });
        int deleted = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.StartsWith("Card_") && fileName.Length > 5)
            {
                string numberPart = fileName.Substring(5);
                if (int.TryParse(numberPart, out _))
                {
                    AssetDatabase.DeleteAsset(path);
                    deleted++;
                }
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
        "Concluído",
        $"Cartas antigas deletadas: {deleted}",
        "OK"
        );
    }
}
