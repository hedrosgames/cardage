using UnityEngine;
[CreateAssetMenu(menuName = "Config/ZoneFlag")]
public class SOZoneFlag : ScriptableObject
{
    public string id;
    [TextArea(3, 10)] public string description;
    private void OnValidate()
    {
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(id))
        {
            id = name.ToLower().Replace(" ", "_");
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
}

