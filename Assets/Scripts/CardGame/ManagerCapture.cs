using UnityEngine;
using System.Collections.Generic;
public class ManagerCapture : MonoBehaviour
{
    public static ManagerCapture Instance { get; private set; }
    public RuleBasic basicRule;
    public List<SOCapture> activeRules = new List<SOCapture>();
    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable() => GameEvents.OnCardPlayed += HandleCardPlayed;
    private void OnDisable() => GameEvents.OnCardPlayed -= HandleCardPlayed;
    public void SetRules(List<SOCapture> rules)
    {
        activeRules.Clear();
        if (basicRule != null) activeRules.Add(basicRule);
        foreach (var r in rules)
        {
            if (r != null && r != basicRule)
            activeRules.Add(r);
        }
    }
    private void HandleCardPlayed(CardSlot slot, SOCardData data, int ownerId)
    {
        ManagerBoard board = ManagerGame.Instance != null ? ManagerGame.Instance.boardManager : FindFirstObjectByType<ManagerBoard>();
        if (board == null) return;
        List<CardSlot> captured = new List<CardSlot>();
        foreach (var rule in activeRules)
        {
            rule.OnCardPlayed(slot, data, ownerId, board, captured);
        }
        if (captured.Count > 0)
        {
            if (CardEffectManager.Instance != null)
            CardEffectManager.Instance.MarkCardCaptured(slot);
            GameEvents.OnCardsCaptured?.Invoke(new List<CardSlot>(captured), ownerId);
        }
        ApplyPostCaptureRules(captured, ownerId, board);
    }
    private void ApplyPostCaptureRules(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
        foreach (var rule in activeRules)
        rule.OnCardsCaptured(captured, ownerId, board);
        ApplyCardEffectsOnCapture(captured, ownerId, board);
    }
    private void ApplyCardEffectsOnCapture(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
        foreach (var slot in captured)
        {
            if (slot == null || !slot.IsOccupied || slot.currentCardView == null) continue;
            SOCardData card = slot.currentCardView.cardData;
            if (card == null || card.generalRules == null) continue;
            if (card.generalRules.Contains(CardGeneralRule.Sacrifice))
            {
                int previousOwner = slot.currentCardView.ownerId == ManagerGame.ID_PLAYER
                ? ManagerGame.ID_OPPONENT
                : ManagerGame.ID_PLAYER;
                ApplySacrificeBonus(board, previousOwner);
            }
        }
    }
    private void ApplySacrificeBonus(ManagerBoard board, int ownerId)
    {
        if (CardEffectManager.Instance != null)
        {
            CardEffectManager.Instance.AddSacrificeBonus(ownerId, 1);
        }
    }
}

