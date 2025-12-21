using System;
using System.Collections.Generic;
using UnityEngine;
public class ManagerTutorial : MonoBehaviour
{
    [Header("Sistema")]
    public SOTutorial currentTutorial;
    public UITutorial uiTutorial;
    public PlayerControl playerControl;
    [Header("Input Cooldown")]
    [Tooltip("Tempo em segundos para ignorar inputs após a ativação do tutorial (evita inputs durante fade in)")]
    public float inputCooldown = 0.5f;
    private HashSet<string> completedTutorials = new HashSet<string>();
    private SOTutorialCondition currentCondition;
    private bool checking;
    private float tutorialStartTime;
    public event Action<SOTutorial> OnTutorialShown;
    public event Action<SOTutorial> OnTutorialClosed;
    void Awake()
    {
        if (uiTutorial == null) uiTutorial = FindFirstObjectByType<UITutorial>();
        if (playerControl == null) playerControl = FindFirstObjectByType<PlayerControl>();
    }
    void OnEnable()
    {
        GameEvents.OnRequestTutorialByAsset += ShowTutorial;
    }
    void OnDisable()
    {
        GameEvents.OnRequestTutorialByAsset -= ShowTutorial;
    }
    void Update()
    {
        if (!checking) return;
        if (currentTutorial == null || currentCondition == null) return;
        
        // Ignora inputs durante o cooldown (evita inputs durante fade in)
        float timeSinceStart = Time.unscaledTime - tutorialStartTime;
        if (timeSinceStart < inputCooldown) return;
        
        if (currentCondition.CheckCompleted(this))
        {
            CompleteCurrentTutorial();
        }
    }
    public void ShowTutorial(SOTutorial tutorial)
    {
        if (tutorial == null) return;
        if (tutorial.runOnce && completedTutorials.Contains(tutorial.name)) return;
        currentTutorial = tutorial;
        currentCondition = tutorial.condition;
        if (playerControl != null)
        {
            if (tutorial.blockMovement) playerControl.SetMovement(false);
            if (tutorial.blockInteraction) playerControl.SetInteraction(false);
        }
        if (currentCondition != null) currentCondition.OnStart(this);
        checking = true;
        tutorialStartTime = Time.unscaledTime;
        OnTutorialShown?.Invoke(tutorial);
    }
    void CompleteCurrentTutorial()
    {
        if (currentTutorial == null) return;
        if (playerControl != null)
        {
            playerControl.SetMovement(true);
            playerControl.SetInteraction(true);
        }
        Completed(currentTutorial);
        OnTutorialClosed?.Invoke(currentTutorial);
        currentTutorial = null;
        currentCondition = null;
        checking = false;
        tutorialStartTime = 0f;
    }
    void Completed(SOTutorial tutorial)
    {
        if (tutorial == null) return;
        completedTutorials.Add(tutorial.name);
    }
    public bool IsCompleted(string tutorialName) => completedTutorials.Contains(tutorialName);
    public List<string> GetCompletedTutorialIds() => new List<string>(completedTutorials);
    public void SetCompletedTutorialIds(IEnumerable<string> ids) => completedTutorials = new HashSet<string>(ids);
}

