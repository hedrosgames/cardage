using UnityEngine;
[CreateAssetMenu(fileName = "GameFlowFlag", menuName = "Save/GameFlowFlag")]
public class SOGameFlowFlag : ScriptableObject
{
    public string id;
    public string description;
    void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = name;
        }
    }
}

