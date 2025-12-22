using UnityEngine;
public class NpcHelper : MonoBehaviour
{
    public SOZoneFlag flagToActivate;
    public SOZoneFlag flagToChangeSetup;
    public SOGameSetup alternateGameSetup;
    public SODialogueSequence alternateDialogue;
    private SaveClientMoment saveMoment;
    private SaveClientZone saveZone;
    private InteractableCardGame interactableCardGame;
    private InteractableSimple interactableSimple;
    private SOGameSetup originalGameSetup;
    private SODialogueSequence originalDialogue;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private void Awake()
    {
        saveMoment = FindFirstObjectByType<SaveClientMoment>();
        saveZone = FindFirstObjectByType<SaveClientZone>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        interactableCardGame = GetComponent<InteractableCardGame>();
        interactableSimple = GetComponent<InteractableSimple>();
        if (interactableCardGame != null)
        {
            originalGameSetup = interactableCardGame.gameSetup;
            originalDialogue = interactableCardGame.dialogueBeforeGame;
        }
        else if (interactableSimple != null)
        {
            originalDialogue = interactableSimple.dialogue;
        }
        if (flagToActivate != null) SetComponentsEnabled(false);
    }
    private void Start()
    {
        if (saveMoment != null) saveMoment.OnLoadComplete += RefreshNPC;
        if (saveZone != null) saveZone.OnLoadComplete += RefreshNPC;
        RefreshNPC();
    }
    private void OnDestroy()
    {
        if (saveMoment != null) saveMoment.OnLoadComplete -= RefreshNPC;
        if (saveZone != null) saveZone.OnLoadComplete -= RefreshNPC;
    }
    public void ForceCheckAndSwap() => RefreshNPC();
    private void RefreshNPC()
    {
        if (flagToActivate != null)
        {
            bool active = CheckFlag(flagToActivate);
            SetComponentsEnabled(active);
        }
        if (flagToChangeSetup != null)
        {
            bool hasFlag = CheckFlag(flagToChangeSetup);
            if (interactableCardGame != null)
            {
                interactableCardGame.gameSetup = hasFlag && alternateGameSetup != null ? alternateGameSetup : originalGameSetup;
                interactableCardGame.dialogueBeforeGame = hasFlag && alternateDialogue != null ? alternateDialogue : originalDialogue;
            }
            if (interactableSimple != null)
            {
                interactableSimple.dialogue = hasFlag && alternateDialogue != null ? alternateDialogue : originalDialogue;
            }
        }
    }
    private bool CheckFlag(SOZoneFlag flag)
    {
        if (saveZone != null && saveZone.HasFlag(flag)) return true;
        if (saveMoment != null && saveMoment.HasFlag(flag)) return true;
        return false;
    }
    private void SetComponentsEnabled(bool state)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = state;
        if (boxCollider2D != null) boxCollider2D.enabled = state;
        if (interactableCardGame != null) interactableCardGame.enabled = state;
        if (interactableSimple != null) interactableSimple.enabled = state;
    }
}

