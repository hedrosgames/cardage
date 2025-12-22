using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Game/Rules/CardEffects/RuleStealthEffect")]
public class RuleStealthEffect : SOCapture
{
    private Dictionary<CardSlot, bool> flippedCards = new Dictionary<CardSlot, bool>();
    public override void OnMatchStart()
    {
        flippedCards.Clear();
        GameEvents.OnCardPlayed += CheckStealth;
        GameEvents.OnMatchReset += HandleMatchReset;
    }
    private void OnDestroy()
    {
        GameEvents.OnCardPlayed -= CheckStealth;
        GameEvents.OnMatchReset -= HandleMatchReset;
    }
    private void CheckStealth(CardSlot slot, SOCardData card, int ownerId)
    {
        if (card == null || card.generalRules == null) return;
        if (!card.generalRules.Contains(CardGeneralRule.Stealth)) return;
    }
    public override void OnCardPlayed(CardSlot origin, SOCardData card, int ownerId, ManagerBoard board, List<CardSlot> captured)
    {
    }
    public override void OnCardsCaptured(List<CardSlot> captured, int ownerId, ManagerBoard board)
    {
        if (board == null || CardEffectManager.Instance == null) return;
        CardSlot lastPlayedSlot = CardEffectManager.Instance.GetLastPlayedSlot();
        if (lastPlayedSlot == null || !lastPlayedSlot.IsOccupied) return;
        SOCardData card = lastPlayedSlot.currentCardView.cardData;
        if (card == null || card.generalRules == null) return;
        if (card.generalRules.Contains(CardGeneralRule.Stealth))
        {
            bool capturedSomething = captured != null && captured.Count > 0;
            if (!capturedSomething)
            {
                flippedCards[lastPlayedSlot] = true;
                HideCardNumbers(lastPlayedSlot);
            }
            else
            {
                flippedCards[lastPlayedSlot] = false;
                ShowCardNumbers(lastPlayedSlot);
            }
        }
    }
    private void HideCardNumbers(CardSlot slot)
    {
        if (slot == null || slot.currentCardView == null) return;
        if (slot.currentCardView.topText != null) slot.currentCardView.topText.text = "?";
        if (slot.currentCardView.rightText != null) slot.currentCardView.rightText.text = "?";
        if (slot.currentCardView.bottomText != null) slot.currentCardView.bottomText.text = "?";
        if (slot.currentCardView.leftText != null) slot.currentCardView.leftText.text = "?";
        if (slot.currentCardView.backImage != null) slot.currentCardView.backImage.gameObject.SetActive(true);
        if (slot.currentCardView.artImage != null) slot.currentCardView.artImage.enabled = false;
    }
    private void ShowCardNumbers(CardSlot slot)
    {
        if (slot == null || slot.currentCardView == null || slot.currentCardView.cardData == null) return;
        if (slot.currentCardView.topText != null) slot.currentCardView.topText.text = slot.currentCardView.cardData.top.ToString();
        if (slot.currentCardView.rightText != null) slot.currentCardView.rightText.text = slot.currentCardView.cardData.right.ToString();
        if (slot.currentCardView.bottomText != null) slot.currentCardView.bottomText.text = slot.currentCardView.cardData.bottom.ToString();
        if (slot.currentCardView.leftText != null) slot.currentCardView.leftText.text = slot.currentCardView.cardData.left.ToString();
        if (slot.currentCardView.backImage != null) slot.currentCardView.backImage.gameObject.SetActive(false);
        if (slot.currentCardView.artImage != null) slot.currentCardView.artImage.enabled = true;
    }
    private void HandleMatchReset()
    {
        flippedCards.Clear();
    }
    public bool IsCardFlipped(CardSlot slot)
    {
        return slot != null && flippedCards.ContainsKey(slot) && flippedCards[slot];
    }
}

