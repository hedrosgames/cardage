using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ManagerCaptureVisual : MonoBehaviour
{
    public static ManagerCaptureVisual Instance { get; private set; }
    private Queue<IEnumerator> visualQueue = new Queue<IEnumerator>();
    private bool isProcessingQueue = false;
    public bool IsBusy => visualQueue.Count > 0 || isProcessingQueue;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            this.enabled = false;
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        if (Instance != null && Instance != this) return;
        GameEvents.OnCardsCaptured += HandleBasicCapture;
        GameEvents.OnComboStep += HandleComboStep;
        GameEvents.OnSameTriggered += HandleSameTriggered;
        GameEvents.OnPlusTriggered += HandlePlusTriggered;
    }
    private void OnDisable()
    {
        GameEvents.OnCardsCaptured -= HandleBasicCapture;
        GameEvents.OnComboStep -= HandleComboStep;
        GameEvents.OnSameTriggered -= HandleSameTriggered;
        GameEvents.OnPlusTriggered -= HandlePlusTriggered;
    }
    void HandleBasicCapture(List<CardSlot> capturedSlots, int ownerId)
    {
        EnqueueAction(AnimateFlipSequence(capturedSlots, ownerId, null));
    }
    void HandleComboStep(CardSlot slot, int ownerId)
    {
        if (slot != null) {
            List<CardSlot> singleList = new List<CardSlot> { slot };
            EnqueueAction(AnimateFlipSequence(singleList, ownerId, "COMBO"));
        }
    }
    void HandleSameTriggered(List<CardSlot> slots) { EnqueueAction(AnimateVFXOnly(slots, "SAME")); }
    void HandlePlusTriggered(List<CardSlot> slots) { EnqueueAction(AnimateVFXOnly(slots, "PLUS")); }
    void EnqueueAction(IEnumerator routine)
    {
        visualQueue.Enqueue(routine);
        if (!isProcessingQueue) StartCoroutine(ProcessQueue());
    }
    IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;
        while (visualQueue.Count > 0) yield return StartCoroutine(visualQueue.Dequeue());
        isProcessingQueue = false;
    }
    IEnumerator AnimateFlipSequence(List<CardSlot> slots, int ownerId, string vfxType)
    {
        float stagger = 0.1f;
        foreach (var slot in slots)
        {
            CardView view = slot.currentCardView;
            if (view != null)
            {
                GameEvents.OnDebugVisualConfirmation?.Invoke("FLIP");
                CardAnimator animator = view.GetComponent<CardAnimator>();
                if (animator == null)
                {
                    animator = view.gameObject.AddComponent<CardAnimator>();
                    animator.cardRect = view.transform as RectTransform;
                    animator.background = view.background;
                }
                Color finalVisualColor = Color.white;
                if (view.visualConfig != null)
                {
                    bool isPlayer = (ownerId == ManagerGame.ID_PLAYER);
                    finalVisualColor = isPlayer ? view.visualConfig.playerColor : view.visualConfig.opponentColor;
                }
                else if (ManagerGame.Instance != null)
                {
                    finalVisualColor = ManagerGame.Instance.GetColorById(ownerId);
                }
                animator.PlayCaptureAnimation(finalVisualColor);
                if (vfxType == "COMBO" && ManagerVisualFeedback.Instance != null)
                ManagerVisualFeedback.Instance.SpawnCombo(slot.transform.position);
                yield return new WaitForSeconds(stagger);
            }
        }
        yield return new WaitForSeconds(0.15f);
    }
    IEnumerator AnimateVFXOnly(List<CardSlot> slots, string type)
    {
        if (ManagerVisualFeedback.Instance != null)
        {
            foreach (var s in slots)
            {
                if (type == "SAME") ManagerVisualFeedback.Instance.SpawnSame(s.transform.position);
                if (type == "PLUS") ManagerVisualFeedback.Instance.SpawnPlus(s.transform.position);
            }
        }
        yield return new WaitForSeconds(0.5f);
    }
}

