using UnityEngine;
using System;

public class FlagDestroyOnAwake : MonoBehaviour
{
    [Header("Configuração da Flag")]
    [Tooltip("Flag que será verificada. Se esta flag existir (valor > 0), o objeto será destruído.")]
    public SOZoneFlag flagToCheck;

    [Header("Opções")]
    [Tooltip("Se verdadeiro, desativa o GameObject ao invés de destruí-lo")]
    public bool disableInsteadOfDestroy = false;

    [Tooltip("Se verdadeiro, verifica no Start também (redundância útil para inicialização)")]
    public bool alsoCheckOnStart = true;

    private SaveClientZone saveZone;

    private void Awake()
    {
        saveZone = FindFirstObjectByType<SaveClientZone>();
    }

    // --- NOVA FUNÇÃO ---
    // Executa toda vez que o objeto é ativado (SetActive true) ou na inicialização
    private void OnEnable()
    {
        CheckFlagAndDestroy();
    }

    private void Start()
    {
        if (saveZone != null)
        {
            if (alsoCheckOnStart)
            {
                CheckFlagAndDestroy();
            }
            saveZone.OnLoadComplete += OnSaveLoaded;
        }
        else
        {
            // Tenta buscar novamente caso Awake tenha falhado ou ordem de execução
            saveZone = FindFirstObjectByType<SaveClientZone>();
            if (saveZone != null)
            {
                saveZone.OnLoadComplete += OnSaveLoaded;
            }
            
            if (alsoCheckOnStart)
            {
                CheckFlagAndDestroy();
            }
        }
    }

    private void OnDestroy()
    {
        if (saveZone != null)
        {
            saveZone.OnLoadComplete -= OnSaveLoaded;
        }
    }

    private void OnSaveLoaded()
    {
        CheckFlagAndDestroy();
    }

    private void CheckFlagAndDestroy()
    {
        if (flagToCheck == null) return;

        // Tenta encontrar o SaveZone se ainda não tiver
        if (saveZone == null)
        {
            saveZone = FindFirstObjectByType<SaveClientZone>();
            if (saveZone == null) return;
        }

        // Se a flag existe, executa a ação (Destruir ou Desativar)
        if (saveZone.HasFlag(flagToCheck))
        {
            if (disableInsteadOfDestroy)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}