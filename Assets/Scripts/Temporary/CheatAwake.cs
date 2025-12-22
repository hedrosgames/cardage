using UnityEngine;
using System.Collections.Generic;
[DefaultExecutionOrder(-10000)]
public class CheatAwake : MonoBehaviour
{
    [Header("Configuração")]
    public List<GameObject> objectsToWake;
    private void Awake()
    {
        WakeUpObjects();
    }
    private void WakeUpObjects()
    {
        if (objectsToWake == null || objectsToWake.Count == 0) return;
        foreach (var obj in objectsToWake)
        {
            if (obj != null)
            {
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                }
            }
        }
    }
}

