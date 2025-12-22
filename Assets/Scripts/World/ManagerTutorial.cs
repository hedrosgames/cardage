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
        if (currentTutorial != null)
        {
            if (currentTutorial.name == "Tutorial_Movement" || currentTutorial.name == "Tutorial_OpenPhone")
            {
                StartCoroutine(SaveTutorialDelayed());
            }
        }
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
    System.Collections.IEnumerator SaveTutorialDelayed()
    {
        yield return null;
        if (ManagerSave.Instance != null)
        {
            SaveClientWorld saveWorld = FindFirstObjectByType<SaveClientWorld>();
            if (saveWorld == null)
            {
                SaveClientWorld[] allSaveWorlds = FindObjectsByType<SaveClientWorld>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
                if (allSaveWorlds != null && allSaveWorlds.Length > 0)
                {
                    saveWorld = allSaveWorlds[0];
                }
            }
            if (saveWorld != null && saveWorld.saveDefinition != null && !string.IsNullOrEmpty(saveWorld.saveDefinition.id))
            {
                ManagerSave.Instance.SaveSpecific(saveWorld.saveDefinition.id);
            }
        }
    }
}

