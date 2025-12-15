using UnityEngine;
[CreateAssetMenu(menuName = "Game/Dialogue")]
public class SODialogueSequence : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea] public string text;
        public Sprite portrait;
    }
    public DialogueLine[] lines;
}

