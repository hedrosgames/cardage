using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ManagerCheat : MonoBehaviour
{
    [Header("UI - Painel")]
    public GameObject panel;
    
    [Header("UI - Regras (Checkboxes)")]
    public Toggle tglRuleOpen;
    public Toggle tglRuleSame;
    public Toggle tglRulePlus;
    public Toggle tglRuleCombo;
    
    [Header("UI - IA (Radio Buttons)")]
    public Toggle tglAIEasy;   
    public Toggle tglAINormal; 
    public Toggle tglAIHard;   
    
    [Header("UI - Reset")]
    public Button btnInitialSetup;

    [Header("Dados (Assets)")]
    public SOCapture ruleOpen;
    public SOCapture ruleSame;
    public SOCapture rulePlus;
    public SOCapture ruleCombo;
    
    [Space(10)]
    public AIBehaviorBase aiEasy;   
    public AIBehaviorBase aiNormal; 
    public AIBehaviorBase aiHard;   

    // --- MEMÓRIA ESTÁTICA (Persiste entre cenas) ---
    private static bool s_HasInitialized = false; // Sabe se é a primeira vez rodando
    private static bool s_Open, s_Same, s_Plus, s_Combo;
    private static int s_AiType = 0; // 0=Easy, 1=Normal, 2=Hard

    private void Start()
    {
        panel.SetActive(false);

        // --- SETUP LISTENERS COM SALVAMENTO ---
        // Agora, além de atualizar o jogo, salvamos o estado nas variáveis estáticas
        
        tglRuleOpen.onValueChanged.AddListener((val) => { s_Open = val; UpdateRules(); });
        tglRuleSame.onValueChanged.AddListener((val) => { s_Same = val; UpdateRules(); });
        tglRulePlus.onValueChanged.AddListener((val) => { s_Plus = val; UpdateRules(); });
        tglRuleCombo.onValueChanged.AddListener((val) => { s_Combo = val; UpdateRules(); });

        // Listeners de IA (Salva o ID da IA selecionada)
        tglAIEasy.onValueChanged.AddListener((isOn) => { if(isOn) { s_AiType = 0; SetAI(aiEasy); } });
        tglAINormal.onValueChanged.AddListener((isOn) => { if(isOn) { s_AiType = 1; SetAI(aiNormal); } });
        tglAIHard.onValueChanged.AddListener((isOn) => { if(isOn) { s_AiType = 2; SetAI(aiHard); } });

        btnInitialSetup.onClick.AddListener(ResetCheat);

        // --- INICIALIZAÇÃO INTELIGENTE ---
        // Se é a primeira vez que o jogo abre, reseta tudo.
        // Se for um Rematch (reload de cena), restaura o que estava antes.
        if (!s_HasInitialized)
        {
            Invoke(nameof(ResetCheat), 0.1f);
            s_HasInitialized = true;
        }
        else
        {
            Invoke(nameof(RestoreState), 0.1f);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    // Reaplica as configurações salvas nas variáveis estáticas
    void RestoreState()
    {
        Debug.Log("[CHEAT] Restaurando estado anterior...");

        // 1. Restaura Regras (Ao setar .isOn, o listener roda e chama UpdateRules automaticamente)
        tglRuleOpen.isOn = s_Open;
        tglRuleSame.isOn = s_Same;
        tglRulePlus.isOn = s_Plus;
        tglRuleCombo.isOn = s_Combo;

        // 2. Restaura IA
        // Desligamos notify primeiro para limpar a seleção visualmente, depois ativamos a certa
        tglAIEasy.SetIsOnWithoutNotify(false);
        tglAINormal.SetIsOnWithoutNotify(false);
        tglAIHard.SetIsOnWithoutNotify(false);

        switch (s_AiType)
        {
            case 0: tglAIEasy.isOn = true; break;   // Dispara SetAI(aiEasy)
            case 1: tglAINormal.isOn = true; break; // Dispara SetAI(aiNormal)
            case 2: tglAIHard.isOn = true; break;   // Dispara SetAI(aiHard)
        }
    }

    void UpdateRules()
    {
        if (ManagerCapture.Instance == null) return;

        List<SOCapture> activeRules = new List<SOCapture>();

        if (tglRuleOpen.isOn && ruleOpen != null) activeRules.Add(ruleOpen);
        if (tglRuleSame.isOn && ruleSame != null) activeRules.Add(ruleSame);
        if (tglRulePlus.isOn && rulePlus != null) activeRules.Add(rulePlus);
        if (tglRuleCombo.isOn && ruleCombo != null) activeRules.Add(ruleCombo);

        ManagerCapture.Instance.SetRules(activeRules);
        
        if (ManagerGame.Instance != null) ManagerGame.Instance.CheckHandVisibility();
        
        // Debug.Log($"[CHEAT] Regras atualizadas: {activeRules.Count} ativas.");
    }

    void SetAI(AIBehaviorBase ai)
    {
        if (ManagerGame.Instance == null || ManagerGame.Instance.setup == null) return;
        
        ManagerGame.Instance.setup.opponent.behavior = ai;
        
        if (ManagerGame.Instance != null) ManagerGame.Instance.CheckHandVisibility();

        // Debug.Log($"[CHEAT] IA trocada para: {ai.name}");
    }

    void ResetCheat()
    {
        // Limpa visualmente e logicamente
        tglRuleOpen.isOn = false;
        tglRuleSame.isOn = false;
        tglRulePlus.isOn = false;
        tglRuleCombo.isOn = false;
        
        // Reseta IA para Easy
        tglAIEasy.isOn = true; 
        
        // Atualiza os estáticos para o padrão
        s_Open = false; s_Same = false; s_Plus = false; s_Combo = false;
        s_AiType = 0;

        Debug.Log("[CHEAT] Reset Completo (Padrão).");
    }
}