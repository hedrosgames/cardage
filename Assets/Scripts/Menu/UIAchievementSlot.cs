using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class UIAchievementSlot : MonoBehaviour
{
    public SOAchievement data;
    private Image borderImage;
    private Image iconImage;
    private GameObject lockOverlay;
    private Button btn;
    private bool isUnlocked;
    public bool IsUnlocked => isUnlocked;
    void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
        borderImage = GetComponent<Image>();
        Transform iconTr = transform.Find("imgIconAchiev") ?? transform.Find("imgIcon") ?? transform.Find("Icon");
        if (iconTr == null && transform.childCount > 0) iconTr = transform.GetChild(0);
        if (iconTr != null) iconImage = iconTr.GetComponent<Image>();
        Transform lockTr = transform.Find("imgLockAchiev") ?? transform.Find("imgLock") ?? transform.Find("Lock");
        if (lockTr == null && transform.childCount > 1) lockTr = transform.GetChild(1);
        if (lockTr != null) lockOverlay = lockTr.gameObject;
    }
    void Start()
    {
        if (data != null)
        {
            if (borderImage != null && data.borderSprite != null)
            borderImage.sprite = data.borderSprite;
            if (iconImage != null && data.iconSprite != null)
            iconImage.sprite = data.iconSprite;
            if (lockOverlay != null)
            {
                Image lockImg = lockOverlay.GetComponent<Image>();
                if (lockImg != null && data.lockSprite != null)
                lockImg.sprite = data.lockSprite;
            }
        }
    }
    void OnEnable()
    {
        if (ManagerAchievements.Instance != null)
        {
        }
        Refresh(isUnlocked);
    }
    public void Refresh(bool unlocked)
    {
        isUnlocked = unlocked;
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!unlocked);
        }
        if (iconImage != null)
        {
            if (!unlocked && data != null && data.isHidden)
            {
                iconImage.color = Color.clear;
            }
            else
            {
                iconImage.color = Color.white;
            }
        }
    }
    void OnClick()
    {
        if (ManagerAchievements.Instance != null)
        {
            ManagerAchievements.Instance.SelectAchievement(this);
        }
    }
}

