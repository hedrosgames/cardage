using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class UISpecialCardCanvas : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("CanvasGroup do grupo principal que terá o fade")]
    public CanvasGroup canvasGroup;
    [Header("Botões")]
    [Tooltip("Os 3 botões que darão cartas ao jogador")]
    public Button[] cardButtons = new Button[3];
    [Header("Cartas")]
    [Tooltip("As 3 cartas que serão dadas ao jogador (uma para cada botão)")]
    public SOCardData[] cardsToGive = new SOCardData[3];
    [Header("Configuração de Fade")]
    [Tooltip("Duração do fade in")]
    public float fadeInDuration = 0.3f;
    [Tooltip("Duração do fade out")]
    public float fadeOutDuration = 0.3f;
    private Coroutine fadeRoutine;
    private SaveClientCard saveClientCard;
    void Awake()
    {
        if (saveClientCard == null)
        {
            saveClientCard = FindFirstObjectByType<SaveClientCard>();
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        SetupButtons();
    }
    void SetupButtons()
    {
        if (cardButtons == null || cardButtons.Length != 3) return;
        if (cardButtons[0] != null)
        {
            cardButtons[0].onClick.RemoveAllListeners();
            cardButtons[0].onClick.AddListener(() => GiveCardToPlayer(0));
        }
        if (cardButtons[1] != null)
        {
            cardButtons[1].onClick.RemoveAllListeners();
            cardButtons[1].onClick.AddListener(() => GiveCardToPlayer(1));
        }
        if (cardButtons[2] != null)
        {
            cardButtons[2].onClick.RemoveAllListeners();
            cardButtons[2].onClick.AddListener(() => GiveCardToPlayer(2));
        }
    }
    public void OpenCanvas()
    {
        if (canvasGroup == null) return;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration));
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    public void CloseCanvas()
    {
        if (canvasGroup == null) return;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeRoutine(canvasGroup.alpha, 0f, fadeOutDuration));
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    public void GiveCardToPlayer(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= 3)
        {
            return;
        }
        if (cardsToGive == null || buttonIndex >= cardsToGive.Length || cardsToGive[buttonIndex] == null)
        {
            return;
        }
        SOCardData cardToAdd = cardsToGive[buttonIndex];
        if (saveClientCard == null)
        {
            saveClientCard = FindFirstObjectByType<SaveClientCard>();
        }
        if (saveClientCard == null || saveClientCard.setup == null || saveClientCard.setup.playerDeck == null)
        {
            return;
        }
        AddCardToDeck(saveClientCard.setup.playerDeck, cardToAdd);
        var saveHelper = Object.FindFirstObjectByType<SaveHelperComponent>();
        if (saveHelper != null)
        {
            saveHelper.SaveCard();
        }
        CloseCanvas();
    }
    private void AddCardToDeck(SODeckData deck, SOCardData card)
    {
        if (deck == null || card == null) return;
        List<SOCardData> cardList = new List<SOCardData>();
        if (deck.cards != null)
        {
            foreach (var existingCard in deck.cards)
            {
                if (existingCard != null)
                {
                    cardList.Add(existingCard);
                }
            }
        }
        cardList.Add(card);
        deck.cards = cardList.ToArray();
    }
    IEnumerator FadeRoutine(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration > 0f ? t / duration : 1f;
            k = Mathf.Clamp01(k);
            canvasGroup.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        canvasGroup.alpha = to;
        fadeRoutine = null;
    }
}

