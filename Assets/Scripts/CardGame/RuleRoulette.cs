using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Game/Rules/Roulette")]
public class RuleRoulette : SOCapture
{
    [Header("Regras DisponÃ­veis")]
    public SOCapture[] availableRules;
    private SOCapture selectedRule;
    public override void OnMatchStart()
    {
        if (availableRules != null && availableRules.Length > 0)
        {
            selectedRule = availableRules[Random.Range(0, availableRules.Length)];
            if (selectedRule != null)
            {
                if (ManagerCapture.Instance != null)
                {
                    List<SOCapture> currentRules = new List<SOCapture>(ManagerCapture.Instance.activeRules);
                    if (!currentRules.Contains(selectedRule))
                    {
                        currentRules.Add(selectedRule);
                        ManagerCapture.Instance.SetRules(currentRules);
                    }
                }
            }
        }
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
}

