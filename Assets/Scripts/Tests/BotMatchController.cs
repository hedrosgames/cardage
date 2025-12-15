using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class BotMatchController : MonoBehaviour
{
    public static BotMatchController Instance { get; private set; }

    [Header("Configuração de Bateria")]
    public bool useBatchMode = false;
    [Tooltip("Lista de IAs para testar em sequência.")]
    public List<AIBehaviorBase> aiBatchList;
    [Tooltip("Quantas partidas cada IA vai jogar.")]
    public int matchesPerAI = 50;

    [Header("Configuração Single Mode")]
    public AIBehaviorBase singlePlayerAI; 
    public int singleTargetMatches = 100;

    [Header("Configuração Geral")]
    public bool autoStart = false;
    public float actionDelay = 1.0f; 
    
    // --- ESTADO ---
    public bool isRunning = false;
    private int currentBatchIndex = 0;
    
    // Stats Lógicos
    private int wins = 0;
    private int losses = 0;
    private int draws = 0;
    private int matchCount = 0;
    
    // VERIFICAÇÃO VISUAL (Integridade)
    private int logicFlips = 0;
    private int visualFlips = 0;
    
    private int logicSame = 0;
    private int visualSame = 0;
    
    private int logicPlus = 0;
    private int visualPlus = 0;
    
    private int logicCombo = 0;
    private int visualCombo = 0;

    // Erros acumulados
    private int totalFlipFailures = 0;
    private int totalVfxFailures = 0;

    private float currentTimeScale = 1f;

    [System.Serializable]
    public class AIReport
    {
        public string aiName;
        public int wins;
        public int losses;
        public float winRate;
        public string integrityReport; 
    }
    private List<AIReport> batchReports = new List<AIReport>();

    // UI
    private bool showDebugUI = true;
    private Rect windowRect = new Rect(10, 10, 280, 600);
    private Texture2D bgTexture;
    private GUIStyle backgroundStyle;

    private AIBehaviorBase CurrentAI
    {
        get
        {
            if (useBatchMode && aiBatchList != null && aiBatchList.Count > 0)
            {
                if (currentBatchIndex < aiBatchList.Count)
                    return aiBatchList[currentBatchIndex];
                return null;
            }
            return singlePlayerAI;
        }
    }

    private int CurrentTargetMatches => useBatchMode ? matchesPerAI : singleTargetMatches;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, Color.white);
        bgTexture.Apply();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.OnMatchFinished += OnMatchFinished;
        GameEvents.OnTurnOwnerChanged += OnTurnOwnerChanged;
        
        GameEvents.OnCardsCaptured += OnLogicCapture;
        GameEvents.OnSameTriggered += OnLogicSame;
        GameEvents.OnPlusTriggered += OnLogicPlus;
        GameEvents.OnComboStep += OnLogicComboStep;

        GameEvents.OnDebugVisualConfirmation += OnVisualConfirmation;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.OnMatchFinished -= OnMatchFinished;
        GameEvents.OnTurnOwnerChanged -= OnTurnOwnerChanged;

        GameEvents.OnCardsCaptured -= OnLogicCapture;
        GameEvents.OnSameTriggered -= OnLogicSame;
        GameEvents.OnPlusTriggered -= OnLogicPlus;
        GameEvents.OnComboStep -= OnLogicComboStep;
        
        GameEvents.OnDebugVisualConfirmation -= OnVisualConfirmation;
    }

    // --- Handlers Lógicos ---
    private void OnLogicCapture(List<CardSlot> slots, int ownerId) 
    { 
        if(isRunning) logicFlips += slots.Count; 
    }
    
    private void OnLogicSame(List<CardSlot> slots) 
    { 
        if(isRunning) logicSame += slots.Count; 
    }
    
    private void OnLogicPlus(List<CardSlot> slots) 
    { 
        if(isRunning) logicPlus += slots.Count; 
    }
    
    private void OnLogicComboStep(CardSlot slot, int ownerId) 
    { 
        if(isRunning) 
        {
            logicCombo++;
            // Combo gera flip visual, então conta aqui também
            logicFlips++; 
        }
    }

    // --- Handler Visual ---
    private void OnVisualConfirmation(string type)
    {
        if (!isRunning) return;
        switch (type)
        {
            case "FLIP": visualFlips++; break;
            case "SAME": visualSame++; break;
            case "PLUS": visualPlus++; break;
            case "COMBO": visualCombo++; break;
        }
    }

    // --- Loop de Jogo ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isRunning) return;
        Time.timeScale = currentTimeScale;
        StartCoroutine(CheckFirstTurn());
    }

    IEnumerator CheckFirstTurn()
    {
        yield return new WaitForSeconds(1f);
        if (ManagerTurn.Instance != null && ManagerTurn.Instance.IsPlayerTurn())
            StartCoroutine(PerformPlayerMove());
    }

    private void OnTurnOwnerChanged(bool isPlayerTurn)
    {
        if (!isRunning) return;
        if (isPlayerTurn) StartCoroutine(PerformPlayerMove());
    }

    IEnumerator PerformPlayerMove()
    {
        yield return new WaitForSeconds(actionDelay);
        if (ManagerGame.Instance == null || CurrentAI == null) yield break;

        var hand = ManagerGame.Instance.playerHandButtons;
        var board = ManagerGame.Instance.GetBoard();

        CardButton chosenCard = null;
        CardSlot chosenSlot = null;

        try {
            chosenCard = CurrentAI.ChooseCard(hand);
            if (chosenCard == null) yield break; 
            
            chosenSlot = CurrentAI.ChooseSlot(board);
        } catch { }

        if (chosenCard != null && chosenSlot != null)
        {
            // 1. Coloca a carta visualmente
            chosenSlot.PlaceCard(chosenCard.GetCardData(), ManagerGame.ID_PLAYER);
            chosenCard.Disable();
            
            // 2. DISPARA O EVENTO DE LÓGICA (Crítico para ativar regras e visuais)
            // Isso simula o que o CardButton.OnEndDrag faz
            GameEvents.OnCardPlayed?.Invoke(chosenSlot, chosenCard.GetCardData(), ManagerGame.ID_PLAYER);
            
            // 3. Passa o turno
            if (ManagerGame.Instance.turnManager != null) 
                ManagerGame.Instance.turnManager.EndTurn();
        }
    }

    private void OnMatchFinished(MatchResult result, int pScore, int oScore)
    {
        if (!isRunning) return;

        CheckIntegrity();

        matchCount++;
        if (result == MatchResult.PlayerWin) wins++;
        else if (result == MatchResult.OpponentWin) losses++;
        else draws++;

        if (matchCount < CurrentTargetMatches)
        {
            StartCoroutine(RestartMatchRoutine());
        }
        else
        {
            HandleBatchLogic();
        }
    }

    void CheckIntegrity()
    {
        int flipDiff = Mathf.Abs(logicFlips - visualFlips);
        int sameDiff = Mathf.Abs(logicSame - visualSame);
        int plusDiff = Mathf.Abs(logicPlus - visualPlus);
        int comboDiff = Mathf.Abs(logicCombo - visualCombo);

        totalFlipFailures += flipDiff;
        totalVfxFailures += (sameDiff + plusDiff + comboDiff);

        if (flipDiff > 0 || sameDiff > 0 || plusDiff > 0 || comboDiff > 0)
        {
            Debug.LogError($"[INTEGRIDADE] Falha na Partida {matchCount + 1}!\n" +
                           $"Flips: {logicFlips} vs {visualFlips}\n" +
                           $"Same: {logicSame} vs {visualSame}\n" +
                           $"Combo: {logicCombo} vs {visualCombo}");
        }
    }

    void HandleBatchLogic()
    {
        if (useBatchMode)
        {
            SaveCurrentReport();
            currentBatchIndex++;
            if (currentBatchIndex < aiBatchList.Count)
            {
                ResetCurrentStats();
                Debug.Log($"<color=yellow>[Bot] Iniciando bateria {currentBatchIndex + 1}/{aiBatchList.Count}: {CurrentAI.name}</color>");
                StartCoroutine(RestartMatchRoutine());
            }
            else
            {
                PrintFinalReport();
                StopBot();
            }
        }
        else
        {
            StopBot();
        }
    }

    void SaveCurrentReport()
    {
        AIReport r = new AIReport();
        r.aiName = CurrentAI != null ? CurrentAI.name : "Unknown";
        r.wins = wins;
        r.losses = losses;
        r.winRate = matchCount > 0 ? (float)wins / matchCount * 100f : 0;
        
        if (totalFlipFailures == 0 && totalVfxFailures == 0)
            r.integrityReport = "<color=green>OK</color>";
        else
            r.integrityReport = $"<color=red>FALHA (Flip:{totalFlipFailures}, VFX:{totalVfxFailures})</color>";
            
        batchReports.Add(r);
    }

    void ResetCurrentStats()
    {
        wins = 0; losses = 0; draws = 0; matchCount = 0;
        logicFlips = 0; visualFlips = 0;
        logicSame = 0; visualSame = 0;
        logicPlus = 0; visualPlus = 0;
        logicCombo = 0; visualCombo = 0;
        totalFlipFailures = 0;
        totalVfxFailures = 0;
    }

    void PrintFinalReport()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>========== RELATÓRIO DE INTEGRIDADE ==========</b>");
        foreach(var r in batchReports)
        {
            sb.AppendLine($"<b>AI: {r.aiName}</b> | WinRate: {r.winRate:F1}%");
            sb.AppendLine($"   Status Visual: {r.integrityReport}");
            sb.AppendLine("-----------------------------------------");
        }
        Debug.Log(sb.ToString());
    }

    IEnumerator RestartMatchRoutine()
    {
        float wait = (currentTimeScale > 20f) ? 0.1f : 2f;
        yield return new WaitForSeconds(wait);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartBot()
    {
        isRunning = true;
        ResetCurrentStats();
        currentBatchIndex = 0;
        batchReports.Clear();
        if (ManagerTurn.Instance != null && ManagerTurn.Instance.IsPlayerTurn())
            StartCoroutine(PerformPlayerMove());
    }

    public void StopBot()
    {
        isRunning = false;
        SetSpeed(1f);
    }

    public void SetSpeed(float scale)
    {
        currentTimeScale = scale;
        Time.timeScale = scale;
    }

    void OnGUI()
    {
        if (backgroundStyle == null) {
            backgroundStyle = new GUIStyle(GUI.skin.box);
            backgroundStyle.normal.background = bgTexture; 
        }

        if (!showDebugUI) {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            GUI.contentColor = Color.white;
            if (GUI.Button(new Rect(10, 10, 120, 30), "BOT 🤖")) showDebugUI = true;
            return;
        }

        Color oldBg = GUI.backgroundColor;
        Color oldContent = GUI.contentColor;

        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f); 
        GUI.Box(windowRect, "", backgroundStyle);
        GUI.contentColor = Color.white;

        GUILayout.BeginArea(windowRect);
        GUILayout.BeginVertical(GUILayout.Width(260));
        GUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        GUILayout.Label(useBatchMode ? "<b><size=14>BATCH MODE</size></b>" : "<b><size=14>SINGLE MODE</size></b>");
        if (GUILayout.Button("X", GUILayout.Width(25))) showDebugUI = false;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (!isRunning)
        {
            useBatchMode = GUILayout.Toggle(useBatchMode, "Bateria (Lista)");
            GUILayout.Space(5);
            if (useBatchMode) {
                GUILayout.Label($"IAs na Lista: {aiBatchList?.Count ?? 0}");
                GUILayout.Label($"Partidas por IA: {matchesPerAI}");
                matchesPerAI = (int)GUILayout.HorizontalSlider(matchesPerAI, 10, 200);
            } else {
                GUILayout.Label($"IA Atual: {singlePlayerAI?.name ?? "None"}");
                GUILayout.Label($"Alvo: {singleTargetMatches} partidas");
                singleTargetMatches = (int)GUILayout.HorizontalSlider(singleTargetMatches, 10, 200);
            }
            
            GUILayout.Space(10);
            GUI.backgroundColor = Color.green; 
            if (GUILayout.Button("INICIAR", GUILayout.Height(30))) StartBot();
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f); 
        }
        else
        {
            GUI.backgroundColor = Color.red; 
            if (GUILayout.Button("PARAR", GUILayout.Height(25))) StopBot();
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f); 
            
            GUILayout.Space(10);
            GUILayout.Label("Velocidade:");
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("1x")) SetSpeed(1f);
            if(GUILayout.Button("5x")) SetSpeed(5f);
            if(GUILayout.Button("20x")) SetSpeed(20f);
            if(GUILayout.Button("50x")) SetSpeed(50f);
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
            GUILayout.Label("<b>INTEGRIDADE (Logic vs Visual)</b>");
            
            DrawStatLine("Flips", logicFlips, visualFlips);
            DrawStatLine("Same", logicSame, visualSame);
            DrawStatLine("Plus", logicPlus, visualPlus);
            DrawStatLine("Combo", logicCombo, visualCombo);

            GUILayout.Space(10);
            if (totalFlipFailures > 0 || totalVfxFailures > 0)
                GUILayout.Label($"<color=red><b>FALHAS DETECTADAS!</b></color>");
            else
                GUILayout.Label($"<color=lime><b>SISTEMA ESTÁVEL</b></color>");
                
            GUILayout.Space(5);
            GUILayout.Label($"Partidas: {matchCount} / {CurrentTargetMatches}");
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();

        GUI.backgroundColor = oldBg;
        GUI.contentColor = oldContent;
    }

    void DrawStatLine(string label, int logic, int visual)
    {
        string color = (logic == visual) ? "lime" : "red";
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}:", GUILayout.Width(60));
        GUILayout.Label($"{logic}", GUILayout.Width(40));
        GUILayout.Label("vs");
        GUILayout.Label($"<color={color}><b>{visual}</b></color>");
        GUILayout.EndHorizontal();
    }
}