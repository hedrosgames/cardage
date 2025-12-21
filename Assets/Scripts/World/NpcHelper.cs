using UnityEngine;
public class NpcHelper : MonoBehaviour
{
    [Header("Verifica a Flag e Ativa os componentes")]
    public SOZoneFlag flagToActivate;
    [Header("Verifica a Flag e Troca o GameSetup")]
    public SOZoneFlag flagToChangeSetup;
    [Header("Configurações Alternativas")]
    [Tooltip("Game setup alternativo que será usado quando a flag for true")]
    public SOGameSetup alternateGameSetup;
    [Tooltip("Diálogo alternativo que será usado quando a flag for true")]
    public SODialogueSequence alternateDialogue;
    private bool alsoCheckOnStart = true;
    private SaveClientMoment saveMoment;
    private Interactable interactable;
    private InteractableCardGame interactableCardGame;
    private InteractableSimple interactableSimple;
    private InteractableLogical interactableLogical;
    private InteractableStoryNPC interactableStoryNPC;
    private SOGameSetup originalGameSetup;
    private SODialogueSequence originalDialogue;
    private SODialogueSequence originalDialogueFail;
    private SODialogueSequence originalDialogueClosing;
    private SpriteRenderer spriteRenderer;
    private BoxCollider boxCollider;
    private BoxCollider2D boxCollider2D;
    private void Awake()
    {
        saveMoment = FindFirstObjectByType<SaveClientMoment>();
        interactable = GetComponent<Interactable>();
        interactableCardGame = GetComponent<InteractableCardGame>();
        interactableSimple = GetComponent<InteractableSimple>();
        interactableLogical = GetComponent<InteractableLogical>();
        interactableStoryNPC = GetComponent<InteractableStoryNPC>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Guarda os valores originais baseado no tipo de Interactable
        StoreOriginalValues();

        CheckFlagAndActivateComponents();
        CheckFlagAndSwap();
    }

    private void StoreOriginalValues()
    {
        if (interactableCardGame != null)
        {
            originalGameSetup = interactableCardGame.gameSetup;
            originalDialogue = interactableCardGame.dialogueBeforeGame;
        }
        else if (interactableSimple != null)
        {
            originalDialogue = interactableSimple.dialogue;
        }
        else if (interactableLogical != null)
        {
            originalDialogue = interactableLogical.dialogueOnSuccess;
            originalDialogueFail = interactableLogical.dialogueOnFail;
        }
        else if (interactableStoryNPC != null)
        {
            originalDialogue = interactableStoryNPC.dialogue;
            originalDialogueClosing = interactableStoryNPC.dialogueClosing;
        }
    }
    private void CheckFlagAndActivateComponents()
    {
        if (flagToActivate == null) return;
        if (saveMoment == null)
        {
            saveMoment = FindFirstObjectByType<SaveClientMoment>();
            if (saveMoment == null) return;
        }
        if (saveMoment.HasFlag(flagToActivate))
        {
            // Ativa qualquer Interactable encontrado
            if (interactable != null)
            {
                interactable.enabled = true;
            }
            // Ativa componentes visuais
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            if (boxCollider != null)
            {
                boxCollider.enabled = true;
            }
            if (boxCollider2D != null)
            {
                boxCollider2D.enabled = true;
            }
        }
    }
    private void OnEnable()
    {
        CheckFlagAndSwap();
    }
    private void Start()
    {
        if (saveMoment != null)
        {
            if (alsoCheckOnStart)
            {
                CheckFlagAndSwap();
            }
            saveMoment.OnLoadComplete += OnSaveLoaded;
        }
        else
        {
            saveMoment = FindFirstObjectByType<SaveClientMoment>();
            if (saveMoment != null)
            {
                saveMoment.OnLoadComplete += OnSaveLoaded;
            }
            if (alsoCheckOnStart)
            {
                CheckFlagAndSwap();
            }
        }
    }
    private void OnDestroy()
    {
        if (saveMoment != null)
        {
            saveMoment.OnLoadComplete -= OnSaveLoaded;
        }
    }
    private void OnSaveLoaded()
    {
        CheckFlagAndSwap();
    }
    public void ForceCheckAndSwap()
    {
        CheckFlagAndSwap();
    }
    private void CheckFlagAndSwap()
    {
        if (flagToChangeSetup == null) return;
        if (interactable == null) return;
        if (saveMoment == null)
        {
            saveMoment = FindFirstObjectByType<SaveClientMoment>();
            if (saveMoment == null) return;
        }

        bool hasFlag = saveMoment.HasFlag(flagToChangeSetup);

        // Troca baseado no tipo de Interactable
        if (interactableCardGame != null)
        {
            SwapCardGameValues(hasFlag);
        }
        else if (interactableSimple != null)
        {
            SwapSimpleValues(hasFlag);
        }
        else if (interactableLogical != null)
        {
            SwapLogicalValues(hasFlag);
        }
        else if (interactableStoryNPC != null)
        {
            SwapStoryNPCValues(hasFlag);
        }
    }

    private void SwapCardGameValues(bool hasFlag)
    {
        if (hasFlag)
        {
            if (alternateGameSetup != null)
            {
                interactableCardGame.gameSetup = alternateGameSetup;
            }
            if (alternateDialogue != null)
            {
                interactableCardGame.dialogueBeforeGame = alternateDialogue;
            }
        }
        else
        {
            if (originalGameSetup != null)
            {
                interactableCardGame.gameSetup = originalGameSetup;
            }
            if (originalDialogue != null)
            {
                interactableCardGame.dialogueBeforeGame = originalDialogue;
            }
        }
    }

    private void SwapSimpleValues(bool hasFlag)
    {
        if (hasFlag)
        {
            if (alternateDialogue != null)
            {
                interactableSimple.dialogue = alternateDialogue;
            }
        }
        else
        {
            if (originalDialogue != null)
            {
                interactableSimple.dialogue = originalDialogue;
            }
        }
    }

    private void SwapLogicalValues(bool hasFlag)
    {
        if (hasFlag)
        {
            if (alternateDialogue != null)
            {
                interactableLogical.dialogueOnSuccess = alternateDialogue;
            }
        }
        else
        {
            if (originalDialogue != null)
            {
                interactableLogical.dialogueOnSuccess = originalDialogue;
            }
            if (originalDialogueFail != null)
            {
                interactableLogical.dialogueOnFail = originalDialogueFail;
            }
        }
    }

    private void SwapStoryNPCValues(bool hasFlag)
    {
        if (hasFlag)
        {
            if (alternateDialogue != null)
            {
                interactableStoryNPC.dialogue = alternateDialogue;
            }
        }
        else
        {
            if (originalDialogue != null)
            {
                interactableStoryNPC.dialogue = originalDialogue;
            }
            if (originalDialogueClosing != null)
            {
                interactableStoryNPC.dialogueClosing = originalDialogueClosing;
            }
        }
    }
}

