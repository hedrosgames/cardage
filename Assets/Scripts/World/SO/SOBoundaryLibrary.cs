using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(menuName = "Config/Boundary Library")]
public class SOBoundaryLibrary : ScriptableObject
{
    [System.Serializable]
    public class BoundaryData
    {
        public string idName;
        public Vector2 minLimit;
        public Vector2 maxLimit;
    }
    public List<BoundaryData> boundaries = new List<BoundaryData>();
    public bool GetBoundary(string enumName, out Vector2 min, out Vector2 max)
    {
        var data = boundaries.FirstOrDefault(b => b.idName == enumName);
        if (data != null)
        {
            min = data.minLimit;
            max = data.maxLimit;
            return true;
        }
        min = Vector2.zero;
        max = Vector2.zero;
        return false;
    }
}

