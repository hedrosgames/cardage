using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class PhoneApp_Drive : MonoBehaviour
{
    [Header("UI - Status")]
    public TextMeshProUGUI statusText;
    [Header("UI - Itens")]
    [Tooltip("Container onde os botões de itens serão criados")]
    public Transform itemsContainer;
    [Tooltip("Prefab do botão de item (deve ter Image, Button e TextMeshProUGUI)")]
    public GameObject itemButtonPrefab;
    [Header("Configuração - Primeira Vez")]
    public SOZoneFlag flagIntroDone;
    public SODialogueSequence introDialogue;
    private SaveClientZone _saveZone;
    private ManagerDialogue _managerDialogue;
    private ManagerItems _managerItems;
    private List<GameObject> itemButtonInstances = new List<GameObject>();
    private SaveClientZone SaveZone => _saveZone ??= FindFirstObjectByType<SaveClientZone>();
    private ManagerDialogue DialogueManager => _managerDialogue ??= FindFirstObjectByType<ManagerDialogue>();
    private ManagerItems ManagerItems => _managerItems ??= FindFirstObjectByType<ManagerItems>();
    public void OnAppOpen()
    {
        if (statusText != null) statusText.text = "Aguardando...";
        StopAllCoroutines();
        StartCoroutine(CheckSequenceRoutine());
    }
    void OnEnable()
    {
        RefreshItemsUI();
    }
    IEnumerator CheckSequenceRoutine()
    {
        yield return null;
        bool isFirstTime = false;
        if (SaveZone != null && flagIntroDone != null)
        {
            if (!SaveZone.HasFlag(flagIntroDone)) isFirstTime = true;
        }
        if (isFirstTime)
        {
            SaveZone.SetFlag(flagIntroDone, 1);
            SaveEvents.RaiseSave();
            if (DialogueManager != null && introDialogue != null)
            {
                GameEvents.OnRequestDialogue?.Invoke(introDialogue);
                GameEvents.OnDialogueFinished += OnDialogueFinished;
            }
            else
            {
                PerformSave();
            }
        }
        else
        {
            PerformSave();
        }
    }
    void OnDialogueFinished(SODialogueSequence seq)
    {
        if (seq == introDialogue)
        {
            if (DialogueManager != null)
            GameEvents.OnDialogueFinished += OnDialogueFinished;
            PerformSave();
        }
    }
    void PerformSave()
    {
        StartCoroutine(SaveProcessRoutine());
    }
    IEnumerator SaveProcessRoutine()
    {
        if (statusText != null) statusText.text = "Sincronizando...";
        yield return new WaitForSeconds(0.5f);
        if (ManagerSave.Instance != null)
        {
            ManagerSave.Instance.SaveAll();
        }
        if (statusText != null) statusText.text = "Backup Concluído.";
        GameEvents.OnDriveSaved?.Invoke();
    }
    void RefreshItemsUI()
    {
        if (ManagerItems == null || itemsContainer == null) return;
        foreach (var button in itemButtonInstances)
        {
            if (button != null) Destroy(button);
        }
        itemButtonInstances.Clear();
        List<SOItemData> ownedItems = ManagerItems.GetOwnedItems();
        if (ownedItems.Count == 0)
        {
            if (statusText != null) statusText.text = "Nenhum item encontrado.";
            return;
        }
        foreach (var item in ownedItems)
        {
            if (item == null) continue;
            CreateItemButton(item);
        }
        if (statusText != null && ownedItems.Count > 0)
        {
            statusText.text = $"{ownedItems.Count} item(ns) encontrado(s)";
        }
    }
    void CreateItemButton(SOItemData item)
    {
        if (itemButtonPrefab == null || itemsContainer == null) return;
        GameObject buttonObj = Instantiate(itemButtonPrefab, itemsContainer);
        itemButtonInstances.Add(buttonObj);
        Button button = buttonObj.GetComponent<Button>();
        if (button == null) button = buttonObj.GetComponentInChildren<Button>();
        Image iconImage = buttonObj.GetComponentInChildren<Image>();
        TextMeshProUGUI nameText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (iconImage != null && item.itemIcon != null)
        {
            iconImage.sprite = item.itemIcon;
        }
        if (nameText != null)
        {
            nameText.text = item.itemName;
        }
        bool isActive = ManagerItems.IsItemActive(item);
        UpdateItemButtonVisual(buttonObj, isActive);
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ToggleItem(item, buttonObj));
        }
    }
    void UpdateItemButtonVisual(GameObject buttonObj, bool isActive)
    {
        if (buttonObj == null) return;
        Image bgImage = buttonObj.GetComponent<Image>();
        if (bgImage == null) bgImage = buttonObj.GetComponentInChildren<Image>();
        if (bgImage != null)
        {
            bgImage.color = isActive ? Color.green : Color.white;
        }
    }
    void ToggleItem(SOItemData item, GameObject buttonObj)
    {
        if (item == null || ManagerItems == null) return;
        bool currentState = ManagerItems.IsItemActive(item);
        bool newState = !currentState;
        ManagerItems.SetItemActive(item, newState);
        UpdateItemButtonVisual(buttonObj, newState);
        if (SaveZone != null)
        {
            SaveEvents.RaiseSave();
        }
    }
}

