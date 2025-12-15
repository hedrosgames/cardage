using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ManagerVisualFeedback : MonoBehaviour
{
    public static ManagerVisualFeedback Instance { get; private set; }
    [Header("Prefab Base")]
    public GameObject vfxPrefab;
    [Header("Imagens (Sprites)")]
    public Sprite imgCombo;
    public Sprite imgSame;
    public Sprite imgPlus;
    void Awake()
    {
        Instance = this;
    }
    public void SpawnSame(Vector3 pos) => SpawnVFX(pos, imgSame, "SAME");
    public void SpawnPlus(Vector3 pos) => SpawnVFX(pos, imgPlus, "PLUS");
    public void SpawnCombo(Vector3 pos) => SpawnVFX(pos, imgCombo, "COMBO");
    void SpawnVFX(Vector3 position, Sprite sprite, string typeReport)
    {
        if (vfxPrefab == null || sprite == null) return;
        GameEvents.OnDebugVisualConfirmation?.Invoke(typeReport);
        Vector3 spawnPos = new Vector3(position.x, position.y, -8f);
        GameObject obj = Instantiate(vfxPrefab, spawnPos, Quaternion.identity);
        VisualEffectFloating vfx = obj.GetComponent<VisualEffectFloating>();
        if (vfx != null)
        {
            vfx.Setup(sprite);
        }
    }
}

