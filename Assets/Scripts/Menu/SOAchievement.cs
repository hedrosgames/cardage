using UnityEngine;
[CreateAssetMenu(menuName = "Config/Achievement")]
public class SOAchievement : ScriptableObject
{
    public string id;
    public string titleKey;
    public string descriptionKey;
    [Header("Editor Info")]
    [TextArea(3, 5)]
    public string editorDescription;
    [Header("Visuals")]
    public Sprite borderSprite;
    public Sprite iconSprite;
    public Sprite lockSprite;
    [HideInInspector] public bool isHidden;
}

