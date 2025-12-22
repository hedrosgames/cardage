using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class MatchSimulator : MonoBehaviour
{
    [Header("Configuração")]
    public int matchesPerRule = 100;
    public bool runOnStart = false;
    public bool showDetailedProgress = true;
    [Header("Resultados")]
    public List<RuleActivationStats> ruleStats = new List<RuleActivationStats>();
    private void Start()
    {
        if (runOnStart)
        {
            RunAllSimulations();
        }
    }
    [ContextMenu("Executar Todas as Simulações")]
    public void RunAllSimulations()
    {
        ruleStats.Clear();
        float startTime = Time.realtimeSinceStartup;
        var rulesToTest = new List<(string name, System.Type type)>
        {
            ("Basic", typeof(RuleBasic)),
            ("Same", typeof(RuleSame)),
            ("Plus", typeof(RulePlus)),
            ("Combo", typeof(RuleCombo)),
            ("Reverse", typeof(RuleReverse)),
            ("Ace", typeof(RuleAce)),
            ("PlusWall", typeof(RulePlusWall)),
            ("SameWall", typeof(RuleSameWall)),
            ("Triad", typeof(RuleTriad)),
            ("SpecialBlock", typeof(RuleSpecialBlock)),
            ("Attack", typeof(RuleAttack)),
            ("Defense", typeof(RuleDefense)),
            ("Protection", typeof(RuleProtection)),
            ("AuraEffect", typeof(RuleAuraEffect)),
            ("Corruption", typeof(RuleCorruption)),
            ("Wear", typeof(RuleWear)),
            ("Sacrifice", typeof(RuleSacrifice)),
            ("Echo", typeof(RuleEcho)),
            ("Retaliation", typeof(RuleRetaliation)),
            ("Territory", typeof(RuleTerritory)),
            ("TerritoryStrong", typeof(RuleTerritoryStrong)),
            ("Stealth", typeof(RuleStealth)),
            ("CenterStrong", typeof(RuleCenterStrong)),
            ("CornerStrong", typeof(RuleCornerStrong)),
            ("SideStrong", typeof(RuleSideStrong)),
            ("Bonus1", typeof(RuleBonus1)),
            ("Bonus2", typeof(RuleBonus2)),
            ("Penalty1", typeof(RulePenalty1)),
            ("Penalty2", typeof(RulePenalty2)),
            ("Betrayal", typeof(RuleBetrayal)),
            ("Open", typeof(RuleOpen)),
            ("Closed", typeof(RuleClosed)),
            ("Random", typeof(RuleRandom)),
            ("Roulette", typeof(RuleRoulette)),
            ("HandSpecial", typeof(RuleHandSpecial)),
            ("HandLegend", typeof(RuleHandLegend)),
            ("Domain", typeof(RuleDomain)),
            ("StealthEffect", typeof(RuleStealthEffect)),
        };
        foreach (var (name, type) in rulesToTest)
        {
            SimulateRule(name, type, matchesPerRule);
        }
        var victoryRules = new List<(string name, System.Type type)>
        {
            ("WinChosen", typeof(RuleWinChosen)),
            ("WinDiff", typeof(RuleWinDiff)),
            ("WinAll", typeof(RuleWinAll)),
            ("SuddenDeath", typeof(RuleSuddenDeath)),
            ("WinNothing", typeof(RuleWinNothing)),
        };
        foreach (var (name, type) in victoryRules)
        {
            SimulateVictoryRule(name, type, matchesPerRule);
        }
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        GenerateReport();
    }
    private void SimulateRule(string ruleName, System.Type ruleType, int matches)
    {
        var stats = new RuleActivationStats
        {
            ruleName = ruleName,
            totalMatches = matches,
            activations = 0,
            successfulActivations = 0,
            failedActivations = 0,
            averageActivationsPerMatch = 0f
        };
        for (int i = 0; i < matches; i++)
        {
            try
            {
                var result = SimulateSingleMatch(ruleType);
                stats.activations += result.activations;
                stats.successfulActivations += result.successfulActivations;
                stats.failedActivations += result.failedActivations;
                foreach (var error in result.errors)
                {
                    if (stats.errors.Count < 5)
                    stats.errors.Add(error);
                }
                if (showDetailedProgress)
                {
                    int progressInterval = matches > 100 ? 50 : (matches > 10 ? 10 : 1);
                    if (i % progressInterval == 0 && i > 0)
                    {
                    }
                }
            }
            catch (System.Exception e)
            {
                stats.failedActivations++;
                if (stats.errors.Count < 5)
                stats.errors.Add(e.Message);
            }
        }
        stats.averageActivationsPerMatch = stats.activations / (float)matches;
        ruleStats.Add(stats);
    }
    private void SimulateVictoryRule(string ruleName, System.Type ruleType, int matches)
    {
        var stats = new RuleActivationStats
        {
            ruleName = ruleName,
            totalMatches = matches,
            activations = 0,
            successfulActivations = 0,
            failedActivations = 0,
            averageActivationsPerMatch = 0f
        };
        for (int i = 0; i < matches; i++)
        {
            try
            {
                var result = SimulateVictoryMatch(ruleType);
                stats.activations += result.activations;
                stats.successfulActivations += result.successfulActivations;
                stats.failedActivations += result.failedActivations;
            }
            catch (System.Exception e)
            {
                stats.failedActivations++;
                if (stats.errors.Count < 5)
                stats.errors.Add(e.Message);
            }
        }
        stats.averageActivationsPerMatch = stats.activations / (float)matches;
        ruleStats.Add(stats);
    }
    private (int activations, int successfulActivations, int failedActivations, List<string> errors) SimulateSingleMatch(System.Type ruleType)
    {
        var simEnv = CreateSimulationEnvironment(ruleType);
        int activations = 0;
        int successfulActivations = 0;
        int failedActivations = 0;
        List<string> errors = new List<string>();
        for (int turn = 0; turn < 9; turn++)
        {
            int currentPlayer = turn % 2 == 0 ? ManagerGame.ID_PLAYER : ManagerGame.ID_OPPONENT;
            var emptySlots = simEnv.board.GetAllSlots().Where(s => !s.IsOccupied).ToList();
            if (emptySlots.Count == 0) break;
            var targetSlot = emptySlots[Random.Range(0, emptySlots.Count)];
            var card = CreateRandomCard();
            PlaceCardSimulated(targetSlot, card, currentPlayer);
            List<CardSlot> captured = new List<CardSlot>();
            foreach (var rule in simEnv.activeRules)
            {
                if (rule.GetType() == ruleType)
                {
                    activations++;
                    try
                    {
                        rule.OnCardPlayed(targetSlot, card, currentPlayer, simEnv.board, captured);
                        successfulActivations++;
                    }
                    catch (System.Exception e)
                    {
                        failedActivations++;
                        if (errors.Count < 5)
                        errors.Add($"{e.GetType().Name}: {e.Message}");
                    }
                }
                else
                {
                    rule.OnCardPlayed(targetSlot, card, currentPlayer, simEnv.board, captured);
                }
            }
            if (captured.Count > 0)
            {
                foreach (var rule in simEnv.activeRules)
                {
                    rule.OnCardsCaptured(captured, currentPlayer, simEnv.board);
                }
                GameEvents.OnCardsCaptured?.Invoke(captured, currentPlayer);
            }
            GameEvents.OnTurnChanged?.Invoke(turn + 1);
        }
        CleanupSimulationEnvironment(simEnv);
        return (activations, successfulActivations, failedActivations, errors);
    }
    private (int activations, int successfulActivations, int failedActivations) SimulateVictoryMatch(System.Type ruleType)
    {
        var simEnv = CreateSimulationEnvironment(null);
        FillBoardRandomly(simEnv);
        var victoryRule = System.Activator.CreateInstance(ruleType) as SOVictoryRule;
        int playerScore = CountPlayerCards(simEnv.board, ManagerGame.ID_PLAYER);
        int opponentScore = CountPlayerCards(simEnv.board, ManagerGame.ID_OPPONENT);
        var result = victoryRule.CalculateVictory(simEnv.board, playerScore, opponentScore);
        CleanupSimulationEnvironment(simEnv);
        bool isValid = result == MatchResult.PlayerWin || result == MatchResult.OpponentWin || result == MatchResult.Draw;
        return (1, isValid ? 1 : 0, isValid ? 0 : 1);
    }
    private SimulationEnvironment CreateSimulationEnvironment(System.Type primaryRuleType)
    {
        var env = new SimulationEnvironment();
        GameObject boardGO = new GameObject("SimBoard");
        boardGO.hideFlags = HideFlags.HideAndDontSave;
        env.board = boardGO.AddComponent<ManagerBoard>();
        env.board.allSlots = new CardSlot[9];
        for (int i = 0; i < 9; i++)
        {
            GameObject slotGO = new GameObject($"Slot_{i}");
            slotGO.hideFlags = HideFlags.HideAndDontSave;
            var slot = slotGO.AddComponent<CardSlot>();
            slot.gridPosition = new Vector2Int(i % 3, i / 3);
            env.board.allSlots[i] = slot;
        }
        GameObject captureGO = new GameObject("SimCapture");
        captureGO.hideFlags = HideFlags.HideAndDontSave;
        env.captureManager = captureGO.AddComponent<ManagerCapture>();
        env.captureManager.basicRule = ScriptableObject.CreateInstance<RuleBasic>();
        GameObject effectGO = new GameObject("SimEffectManager");
        effectGO.hideFlags = HideFlags.HideAndDontSave;
        env.effectManager = effectGO.AddComponent<CardEffectManager>();
        GameObject bonusGO = new GameObject("SimBonusManager");
        bonusGO.hideFlags = HideFlags.HideAndDontSave;
        env.bonusManager = bonusGO.AddComponent<SlotBonusManager>();
        env.activeRules = new List<SOCapture>();
        env.activeRules.Add(env.captureManager.basicRule);
        if (primaryRuleType != null)
        {
            var primaryRule = System.Activator.CreateInstance(primaryRuleType) as SOCapture;
            env.activeRules.Add(primaryRule);
        }
        env.captureManager.SetRules(env.activeRules);
        foreach (var rule in env.activeRules)
        {
            rule.OnMatchStart();
        }
        return env;
    }
    private void FillBoardRandomly(SimulationEnvironment env)
    {
        var slots = env.board.GetAllSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            var card = CreateRandomCard();
            int owner = Random.Range(0, 2);
            PlaceCardSimulated(slots[i], card, owner);
        }
    }
    private void PlaceCardSimulated(CardSlot slot, SOCardData card, int ownerId)
    {
        GameObject cardGO = new GameObject("SimCard");
        cardGO.hideFlags = HideFlags.HideAndDontSave;
        cardGO.transform.SetParent(slot.transform);
        var cardView = cardGO.AddComponent<CardView>();
        cardView.Setup(card, ownerId);
        slot.currentCardView = cardView;
    }
    private SOCardData CreateRandomCard()
    {
        var card = ScriptableObject.CreateInstance<SOCardData>();
        card.cardName = $"TestCard_{Random.Range(1000, 9999)}";
        card.top = Random.Range(1, 10);
        card.right = Random.Range(1, 10);
        card.bottom = Random.Range(1, 10);
        card.left = Random.Range(1, 10);
        card.rarity = (CardRarity)Random.Range(0, 5);
        card.type = (CardType)Random.Range(0, 3);
        card.subType = (CardSubType)Random.Range(0, 3);
        card.triad = (TriadType)Random.Range(0, 3);
        card.special = (SpecialType)Random.Range(0, 4);
        if (Random.value < 0.3f)
        {
            var randomRule = (CardGeneralRule)Random.Range(1, System.Enum.GetValues(typeof(CardGeneralRule)).Length);
            card.generalRules = new List<CardGeneralRule> { randomRule };
        }
        return card;
    }
    private int CountPlayerCards(ManagerBoard board, int playerId)
    {
        int count = 0;
        foreach (var slot in board.GetAllSlots())
        {
            if (slot.IsOccupied && slot.currentCardView.ownerId == playerId)
            {
                if (slot.currentCardView.cardData.rarity != CardRarity.Special)
                count++;
            }
        }
        return count;
    }
    private void CleanupSimulationEnvironment(SimulationEnvironment env)
    {
        if (env.board != null) DestroyImmediate(env.board.gameObject);
        if (env.captureManager != null) DestroyImmediate(env.captureManager.gameObject);
        if (env.effectManager != null) DestroyImmediate(env.effectManager.gameObject);
        if (env.bonusManager != null) DestroyImmediate(env.bonusManager.gameObject);
    }
    private void GenerateReport()
    {
        var report = new StringBuilder();
        report.AppendLine("========================================");
        report.AppendLine("RELATÓRIO DE SIMULAÇÃO DE REGRAS");
        report.AppendLine("========================================");
        report.AppendLine();
        report.AppendLine($"Total de Regras Testadas: {ruleStats.Count}");
        report.AppendLine($"Total de Partidas Simuladas: {ruleStats.Sum(s => s.totalMatches)}");
        report.AppendLine($"Total de Ativações: {ruleStats.Sum(s => s.activations)}");
        report.AppendLine($"Partidas por Regra: {matchesPerRule}");
        report.AppendLine();
        report.AppendLine("=== ESTATÍSTICAS POR REGRA ===");
        report.AppendLine();
        foreach (var stat in ruleStats.OrderByDescending(s => s.activations))
        {
            float successRate = stat.totalMatches > 0 ? (stat.successfulActivations / (float)stat.activations * 100f) : 0f;
            report.AppendLine($"[{stat.ruleName}]");
            report.AppendLine($"  Partidas: {stat.totalMatches}");
            report.AppendLine($"  Ativações: {stat.activations}");
            report.AppendLine($"  Sucessos: {stat.successfulActivations}");
            report.AppendLine($"  Falhas: {stat.failedActivations}");
            report.AppendLine($"  Taxa de Sucesso: {successRate:F2}%");
            report.AppendLine($"  Média de Ativações/Partida: {stat.averageActivationsPerMatch:F2}");
            if (stat.errors.Count > 0)
            {
                report.AppendLine($"  Erros: {stat.errors.Count}");
                foreach (var error in stat.errors)
                {
                    report.AppendLine($"    - {error}");
                }
            }
            report.AppendLine();
        }
        report.AppendLine("=== TOP 10 REGRAS MAIS ATIVADAS ===");
        var top10 = ruleStats.OrderByDescending(s => s.activations).Take(10);
        foreach (var stat in top10)
        {
            report.AppendLine($"{stat.ruleName}: {stat.activations} ativações");
        }
        report.AppendLine();
        var problematic = ruleStats.Where(s => s.failedActivations > 0 || s.errors.Count > 0).ToList();
        if (problematic.Count > 0)
        {
            report.AppendLine("=== REGRAS COM PROBLEMAS ===");
            foreach (var stat in problematic)
            {
                report.AppendLine($"{stat.ruleName}: {stat.failedActivations} falhas, {stat.errors.Count} erros");
            }
        }
        else
        {
            report.AppendLine("âœ“ Nenhuma regra apresentou problemas!");
        }
        #if UNITY_EDITOR
        string path = System.IO.Path.Combine(Application.dataPath, "SimulationReport.txt");
        System.IO.File.WriteAllText(path, report.ToString());
        #endif
    }
}
[System.Serializable]
public class RuleActivationStats
{
    public string ruleName;
    public int totalMatches;
    public int activations;
    public int successfulActivations;
    public int failedActivations;
    public float averageActivationsPerMatch;
    public List<string> errors = new List<string>();
}
public class SimulationEnvironment
{
    public ManagerBoard board;
    public ManagerCapture captureManager;
    public CardEffectManager effectManager;
    public SlotBonusManager bonusManager;
    public List<SOCapture> activeRules;
}

