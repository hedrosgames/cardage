using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class ManagerAchievements : MonoBehaviour
{
    public static ManagerAchievements Instance { get; private set; }
    public SaveClientAchievements saveClient;
    [Header("Detail View")]
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailDescription;
    public Image detailIcon;
    public Image detailBorder;
    [Header("Slots")]
    public List<UIAchievementSlot> slots;
    private UIAchievementSlot currentSelectedSlot;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    IEnumerator Start()
    {
        yield return null;
        if (saveClient == null)
        saveClient = FindFirstObjectByType<SaveClientAchievements>();
        if (saveClient != null)
        {
            saveClient.OnLoadComplete += RefreshAll;
        }
        ClearDetailView();
        RefreshAll();
    }
    void OnDestroy()
    {
        if (saveClient != null)
        saveClient.OnLoadComplete -= RefreshAll;
    }
    public void RefreshAll()
    {
        if (saveClient == null) return;
        foreach (var slot in slots)
        {
            if (slot == null || slot.data == null) continue;
            bool unlocked = saveClient.IsUnlocked(slot.data.id);
            slot.Refresh(unlocked);
        }
        if (currentSelectedSlot != null)
        SelectAchievement(currentSelectedSlot);
    }
    public void UnlockAchievement(string id)
    {
        if (saveClient != null)
        {
            if (!saveClient.IsUnlocked(id))
            {
                saveClient.Unlock(id);
                RefreshAll();
            }
        }
    }
    public void SelectAchievement(UIAchievementSlot slot)
    {
        if (slot == null || slot.data == null) return;
        currentSelectedSlot = slot;
        SOAchievement d = slot.data;
        bool unlocked = false;
        if (saveClient != null) unlocked = saveClient.IsUnlocked(d.id);
        if (detailIcon != null)
        {
            detailIcon.sprite = d.iconSprite;
            detailIcon.enabled = true;
            if (!unlocked && d.isHidden) detailIcon.color = Color.black;
            else detailIcon.color = unlocked ? Color.white : Color.gray;
        }
        if (detailBorder != null)
        {
            detailBorder.enabled = true;
        }
        string t = "???";
        string desc = "Bloqueado";
        if (unlocked || !d.isHidden)
        {
            if (ManagerLocalization.Instance != null)
            {
                t = ManagerLocalization.Instance.GetText(d.titleKey);
                desc = ManagerLocalization.Instance.GetText(d.descriptionKey);
            }
            else
            {
                t = d.titleKey;
                desc = d.descriptionKey;
            }
        }
        if (detailTitle != null) detailTitle.text = t;
        if (detailDescription != null) detailDescription.text = desc;
    }
    void ClearDetailView()
    {
        currentSelectedSlot = null;
        if (detailTitle != null) detailTitle.text = "";
        if (detailDescription != null) detailDescription.text = "Selecione uma conquista";
        if (detailIcon != null) { detailIcon.enabled = false; detailIcon.sprite = null; }
        if (detailBorder != null) { detailBorder.enabled = false; }
    }
}

