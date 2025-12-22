using UnityEngine;
using System;
public class FlagDestroyOnAwake : MonoBehaviour
{
    [Header("Configuração da Flag")]
    public SOZoneFlag zoneFlagToCheck;
    public SOGameFlowFlag flowFlagToCheck;
    [Header("Opções")]
    public bool disableInsteadOfDestroy = false;
    public bool alsoCheckOnStart = true;
    private SaveClientZone saveZone;
    private SaveClientGameFlow saveGameFlow;
    private void Awake()
    {
        FindClients();
    }
    private void FindClients()
    {
        if (saveZone == null) saveZone = FindFirstObjectByType<SaveClientZone>();
        if (saveGameFlow == null) saveGameFlow = FindFirstObjectByType<SaveClientGameFlow>();
    }
    private void OnEnable()
    {
        CheckFlagAndDestroy();
    }
    private void Start()
    {
        FindClients();
        if (saveZone != null) saveZone.OnLoadComplete += OnSaveLoaded;
        if (saveGameFlow != null) saveGameFlow.OnLoadComplete += OnSaveLoaded;
        if (alsoCheckOnStart)
        {
            CheckFlagAndDestroy();
        }
    }
    private void OnDisable()
    {
        if (saveZone != null) saveZone.OnLoadComplete -= OnSaveLoaded;
        if (saveGameFlow != null) saveGameFlow.OnLoadComplete -= OnSaveLoaded;
    }
    private void OnSaveLoaded()
    {
        CheckFlagAndDestroy();
    }
    private void CheckFlagAndDestroy()
    {
        if (zoneFlagToCheck == null && flowFlagToCheck == null) return;
        FindClients();
        bool shouldDestroy = false;
        if (zoneFlagToCheck != null && saveZone != null && saveZone.HasFlag(zoneFlagToCheck))
        {
            shouldDestroy = true;
        }
        if (!shouldDestroy && flowFlagToCheck != null && saveGameFlow != null && saveGameFlow.HasFlag(flowFlagToCheck))
        {
            shouldDestroy = true;
        }
        if (shouldDestroy)
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

