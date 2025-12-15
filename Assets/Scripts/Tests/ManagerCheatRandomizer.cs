using UnityEngine;
using System.Collections.Generic;

public class ManagerCheatRandomizer : MonoBehaviour
{
    [Header("Configuração de Randomização")]
    public int minSideValue = 2;
    public int maxSideValue = 9;
    public int minTotalPower = 16;
    public int maxTotalPower = 25;

    private void Start()
    {
        // Delay para rodar após a inicialização padrão
        Invoke(nameof(RandomizeHands), 0.1f);
    }

    public void RandomizeHands()
    {
        if (ManagerGame.Instance == null) return;

        Debug.Log("<color=orange>[CHEAT] Criando novas cartas randomizadas...</color>");

        // Randomiza Player (Interativo = TRUE)
        RandomizeList(ManagerGame.Instance.playerHandButtons, ManagerGame.ID_PLAYER, true);

        // Randomiza Oponente (Interativo = FALSE)
        RandomizeList(ManagerGame.Instance.opponentHandButtons, ManagerGame.ID_OPPONENT, false);

        // CORREÇÃO FINAL: Força o ManagerGame a reaplicar a visibilidade correta
        // Isso garante que, se o Randomizer tiver "acendido" alguma carta por engano no Setup,
        // o ManagerGame vai lá e apaga de novo baseada nas regras atuais.
        ManagerGame.Instance.CheckHandVisibility();
    }

    void RandomizeList(List<CardButton> buttons, int ownerId, bool isInteractable)
    {
        foreach (var btn in buttons)
        {
            SOCardData originalData = btn.GetCardData();
            if (originalData == null) continue;

            // 1. Clone
            SOCardData cloneData = Instantiate(originalData);
            cloneData.name = originalData.name + "_Randomized";

            // 2. Randomiza
            int[] sides = GenerateValidStats();
            cloneData.top = sides[0];
            cloneData.right = sides[1];
            cloneData.bottom = sides[2];
            cloneData.left = sides[3];
            cloneData.power = sides[0] + sides[1] + sides[2] + sides[3];

            // 3. Setup (CORREÇÃO: Usa o bool isInteractable passado por parâmetro)
            btn.Setup(cloneData, ManagerGame.Instance, ownerId, isInteractable);
            
            // Atualiza visual
            btn.cardView.Setup(cloneData, ownerId);
        }
    }

    int[] GenerateValidStats()
    {
        int[] sides = new int[4];
        int total = 0;
        int attempts = 0;

        do
        {
            total = 0;
            for (int i = 0; i < 4; i++)
            {
                sides[i] = Random.Range(minSideValue, maxSideValue + 1);
                total += sides[i];
            }
            attempts++;
        } 
        while ((total < minTotalPower || total > maxTotalPower) && attempts < 100);

        if (total < minTotalPower || total > maxTotalPower)
        {
            sides = new int[] { 4, 4, 4, 4 }; 
        }

        return sides;
    }
}