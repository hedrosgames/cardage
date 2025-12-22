using UnityEngine;
using System.Collections.Generic;
public static class CardValueCalculator
{
    public static int GetCardValue(SOCardData card, CardSlot slot, ManagerBoard board, int ownerId,
    CardDirection direction, bool isAttacking, List<CardSlot> capturedThisTurn = null, bool applyPostCaptureBonuses = false)
    {
        if (card == null || slot == null) return 0;
        int baseValue = GetBaseValue(card, direction);
        int bonus = 0;
        if (card.generalRules != null)
        {
            foreach (var rule in card.generalRules)
            {
                SOCapture activeRule = GetActiveRuleForEffect(rule);
                if (activeRule != null)
                {
                    bonus += GetRuleBonusFromInstance(activeRule, rule, card, slot, board, ownerId, isAttacking, capturedThisTurn);
                }
            }
        }
        if (applyPostCaptureBonuses)
        {
            bonus += GetSpecialTypeBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
        }
        return Mathf.Max(0, baseValue + bonus);
    }
    private static int GetBaseValue(SOCardData card, CardDirection direction)
    {
        switch (direction)
        {
            case CardDirection.Top: return card.top;
            case CardDirection.Right: return card.right;
            case CardDirection.Bottom: return card.bottom;
            case CardDirection.Left: return card.left;
            default: return 0;
        }
    }
    private static SOCapture GetActiveRuleForEffect(CardGeneralRule effectType)
    {
        if (ManagerCapture.Instance == null) return null;
        foreach (var rule in ManagerCapture.Instance.activeRules)
        {
            switch (effectType)
            {
                case CardGeneralRule.Attack:
                if (rule is RuleAttack) return rule;
                break;
                case CardGeneralRule.Defense:
                if (rule is RuleDefense) return rule;
                break;
                case CardGeneralRule.Protection:
                if (rule is RuleProtection) return rule;
                break;
                case CardGeneralRule.Aura:
                if (rule is RuleAuraEffect) return rule;
                break;
                case CardGeneralRule.Corruption:
                if (rule is RuleCorruption) return rule;
                break;
                case CardGeneralRule.Wear:
                if (rule is RuleWear) return rule;
                break;
                case CardGeneralRule.Sacrifice:
                if (rule is RuleSacrifice) return rule;
                break;
                case CardGeneralRule.Echo:
                if (rule is RuleEcho) return rule;
                break;
                case CardGeneralRule.Retaliation:
                if (rule is RuleRetaliation) return rule;
                break;
                case CardGeneralRule.Territory:
                if (rule is RuleTerritory) return rule;
                break;
                case CardGeneralRule.TerritoryStrong:
                if (rule is RuleTerritoryStrong) return rule;
                break;
                case CardGeneralRule.Stealth:
                if (rule is RuleStealth) return rule;
                break;
                case CardGeneralRule.CenterStrong:
                if (rule is RuleCenterStrong) return rule;
                break;
                case CardGeneralRule.CornerStrong:
                if (rule is RuleCornerStrong) return rule;
                break;
                case CardGeneralRule.SideStrong:
                if (rule is RuleSideStrong) return rule;
                break;
                case CardGeneralRule.Bonus1:
                if (rule is RuleBonus1) return rule;
                break;
                case CardGeneralRule.Bonus2:
                if (rule is RuleBonus2) return rule;
                break;
                case CardGeneralRule.Penalty1:
                if (rule is RulePenalty1) return rule;
                break;
                case CardGeneralRule.Penalty2:
                if (rule is RulePenalty2) return rule;
                break;
                case CardGeneralRule.Betrayal:
                if (rule is RuleBetrayal) return rule;
                break;
            }
        }
        return null;
    }
    private static int GetRuleBonusFromInstance(SOCapture ruleInstance, CardGeneralRule ruleType,
    SOCardData card, CardSlot slot, ManagerBoard board, int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        switch (ruleInstance)
        {
            case RuleAttack r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleDefense r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleProtection r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleAuraEffect r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleCorruption r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleWear r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleSacrifice r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleEcho r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleRetaliation r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleTerritory r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleTerritoryStrong r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleStealth r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleCenterStrong r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleCornerStrong r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleSideStrong r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleBonus1 r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleBonus2 r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RulePenalty1 r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RulePenalty2 r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            case RuleBetrayal r: return r.GetBonus(card, slot, board, ownerId, isAttacking, capturedThisTurn);
            default: return 0;
        }
    }
    private static bool HasAdjacentSameOwner(CardSlot slot, ManagerBoard board, int ownerId)
    {
        if (board == null) return false;
        Vector2Int pos = slot.gridPosition;
        CardSlot[] neighbors = {
            board.GetSlot(pos.x, pos.y + 1),
            board.GetSlot(pos.x + 1, pos.y),
            board.GetSlot(pos.x, pos.y - 1),
            board.GetSlot(pos.x - 1, pos.y)
        };
        foreach (var neigh in neighbors)
        {
            if (neigh != null && neigh.IsOccupied && neigh.currentCardView.ownerId == ownerId)
            return true;
        }
        return false;
    }
    private static bool HasAdjacentEnemy(CardSlot slot, ManagerBoard board, int ownerId)
    {
        if (board == null) return false;
        Vector2Int pos = slot.gridPosition;
        CardSlot[] neighbors = {
            board.GetSlot(pos.x, pos.y + 1),
            board.GetSlot(pos.x + 1, pos.y),
            board.GetSlot(pos.x, pos.y - 1),
            board.GetSlot(pos.x - 1, pos.y)
        };
        foreach (var neigh in neighbors)
        {
            if (neigh != null && neigh.IsOccupied && neigh.currentCardView.ownerId != ownerId)
            return true;
        }
        return false;
    }
    private static bool HasClanCardAdjacent(CardSlot slot, ManagerBoard board, CardType clanType, int ownerId)
    {
        if (board == null) return false;
        Vector2Int pos = slot.gridPosition;
        CardSlot[] neighbors = {
            board.GetSlot(pos.x, pos.y + 1),
            board.GetSlot(pos.x + 1, pos.y),
            board.GetSlot(pos.x, pos.y - 1),
            board.GetSlot(pos.x - 1, pos.y)
        };
        foreach (var neigh in neighbors)
        {
            if (neigh != null && neigh.IsOccupied &&
            neigh.currentCardView.ownerId == ownerId &&
            neigh.currentCardView.cardData.type == clanType)
            return true;
        }
        return false;
    }
    private static bool IsCenterSlot(CardSlot slot)
    {
        return slot.gridPosition.x == 1 && slot.gridPosition.y == 1;
    }
    private static bool IsCornerSlot(CardSlot slot)
    {
        Vector2Int pos = slot.gridPosition;
        return (pos.x == 0 && pos.y == 0) || (pos.x == 2 && pos.y == 0) ||
        (pos.x == 0 && pos.y == 2) || (pos.x == 2 && pos.y == 2);
    }
    private static bool IsSideSlot(CardSlot slot)
    {
        Vector2Int pos = slot.gridPosition;
        return (pos.x == 1 && (pos.y == 0 || pos.y == 2)) ||
        ((pos.x == 0 || pos.x == 2) && pos.y == 1);
    }
    private static int GetSpecialTypeBonus(SOCardData card, CardSlot slot, ManagerBoard board,
    int ownerId, bool isAttacking, List<CardSlot> capturedThisTurn)
    {
        if (card == null) return 0;
        switch (card.special)
        {
            case SpecialType.Domain:
            if (IsDomainRuleActive() && CardEffectManager.Instance != null && CardEffectManager.Instance.DidCardCapture(slot))
            {
                return 1;
            }
            return 0;
            case SpecialType.Camouflage:
            if (HasCompletedLine(slot, board, ownerId))
            {
                return 1;
            }
            return 0;
            case SpecialType.Aura:
            SOCapture auraRule = GetActiveRuleForEffect(CardGeneralRule.Aura);
            if (auraRule != null && HasAdjacentEnemy(slot, board, ownerId))
            {
                return 1;
            }
            return 0;
            default:
            return 0;
        }
    }
    private static bool HasCompletedLine(CardSlot slot, ManagerBoard board, int ownerId)
    {
        if (board == null || slot == null) return false;
        Vector2Int pos = slot.gridPosition;
        bool horizontal = true;
        for (int x = 0; x < 3; x++)
        {
            CardSlot s = board.GetSlot(x, pos.y);
            if (s == null || !s.IsOccupied || s.currentCardView.ownerId != ownerId)
            {
                horizontal = false;
                break;
            }
        }
        if (horizontal) return true;
        bool vertical = true;
        for (int y = 0; y < 3; y++)
        {
            CardSlot s = board.GetSlot(pos.x, y);
            if (s == null || !s.IsOccupied || s.currentCardView.ownerId != ownerId)
            {
                vertical = false;
                break;
            }
        }
        if (vertical) return true;
        return false;
    }
    private static bool IsDomainRuleActive()
    {
        if (ManagerCapture.Instance == null) return false;
        foreach (var rule in ManagerCapture.Instance.activeRules)
        {
            if (rule is RuleDomain)
            return true;
        }
        return false;
    }
}
public enum CardDirection { Top, Right, Bottom, Left }

