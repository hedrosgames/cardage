using UnityEngine;
using Game.World;
[CreateAssetMenu(menuName = "Config/Camera")]
public class SOCameraConfig : ScriptableObject
{
    [Header("Referência Obrigatória")]
    public SOBoundaryLibrary boundaryLibrary;
    [Header("Configuração Global")]
    public Vector2 globalFollowOffset;
    [System.Serializable]
    public class AreaSettings
    {
        [HideInInspector] public string name;
        public WorldAreaId id;
        [Header("Comportamento")]
        public bool followPlayer;
        public Vector3 fixedPosition;
        [Header("Boundary ID")]
        [Tooltip("Selecione qual limite esta área deve respeitar (Definido na Library).")]
        public BoundaryId boundaryId;
    }
    public AreaSettings[] areas;
    public AreaSettings GetArea(WorldAreaId id)
    {
        if (id == WorldAreaId.None) return null;
        for (int i = 0; i < areas.Length; i++)
        {
            if (areas[i] != null && areas[i].id == id) return areas[i];
        }
        return null;
    }
    private void OnValidate()
    {
        if (areas == null) return;
        for (int i = 0; i < areas.Length; i++)
        {
            if (areas[i] != null)
            areas[i].name = areas[i].id != WorldAreaId.None ? areas[i].id.ToString() : "[Sem ID]";
        }
    }
}

