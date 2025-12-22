using System;
using System.Collections.Generic;
using UnityEngine;
using Game.World;
public static class GameEvents
{
    public static Action<CardSlot, SOCardData, int> OnCardPlayed;
    public static Action<List<CardSlot>, int> OnCardsCaptured;
    public static Action OnComboTriggered;
    public static Action<CardSlot, int> OnComboStep;
    public static Action<List<CardSlot>> OnSameTriggered;
    public static Action<List<CardSlot>> OnPlusTriggered;
    public static Action<int> OnTurnChanged;
    public static Action<bool> OnTurnOwnerChanged;
    public static Action<int, int> OnScoreChanged;
    public static Action<string> OnPlayerNameChanged;
    public static Action<string> OnOpponentNameChanged;
    public static Action<MatchResult, int, int> OnMatchFinished;
    public static Action OnMatchReset;
    public static Action OnForceOpponentAction;
    public static Action OnTestMatchStart;
    public static Action OnTestMatchEnd;
    public static Action<bool> OnHandVisibilityChanged;
    public static Action OnBankChecked;
    public static Action OnZapChecked;
    public static Action OnDriveSaved;
    public static Action<Vector3, WorldAreaId> OnPlayerTeleport;
    public static Action<WorldAreaId> OnCurtainOpenedAfterTeleport;
    public static Action OnGameplayStarted;
    public static Action<SOTutorial> OnRequestTutorialByAsset;
    public static Action<SODialogueSequence> OnRequestDialogue;
    public static Action<SODialogueSequence> OnDialogueFinished;
    public static Action<string> OnDebugVisualConfirmation;
    public static Action<Interactable> OnInteractableFocused;
    public static Action<Interactable> OnInteractableBlurred;
}

