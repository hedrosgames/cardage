using UnityEngine;
using System.Collections.Generic;
public class ManagerRule : MonoBehaviour
{
    public static ManagerRule Instance { get; private set; }
    public SORuleSet currentSet;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ApplyCurrentRuleSet();
    }
    public void ApplyCurrentRuleSet()
    {
        if (ManagerCapture.Instance == null)
        return;
        List<SOCapture> rules = new List<SOCapture>();
        if (currentSet != null && currentSet.activeRules != null)
        {
            foreach (var r in currentSet.activeRules)
            if (r != null)
            rules.Add(r);
        }
        ManagerCapture.Instance.SetRules(rules);
        foreach (var r in rules)
        {
            r.OnMatchStart();
        }
        if (currentSet != null && currentSet.victoryRule != null)
        {
            currentSet.victoryRule.OnMatchStart();
        }
    }
    public void SetRuleSet(SORuleSet set)
    {
        currentSet = set;
        ApplyCurrentRuleSet();
    }
}

