using UnityEngine;
using System.Collections.Generic;
using Game.World;
using System;
using System.IO;
using UnityEngine.SceneManagement;
public class SaveClientWorld : MonoBehaviour, ISaveClient
{
    public SOSaveDefinition saveDefinition;
    private const string WORLD_SCENE_NAME = "World";
    [System.Serializable]
    class Data
    {
        public float px;
        public float py;
        public float pz;
        public float cx;
        public float cy;
        public float cz;
        public string areaId;
        public string[] tutorialsDone;
    }
    void OnEnable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.RegisterClient(saveDefinition, this);
        }
        GameEvents.OnPlayerTeleport += OnPlayerTeleport;
    }
    void OnDisable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.UnregisterClient(saveDefinition, this);
        }
        GameEvents.OnPlayerTeleport -= OnPlayerTeleport;
    }
    private float savedPlayerX = 0f;
    private float savedPlayerY = 0f;
    private float savedPlayerZ = 0f;
    private float savedCameraX = 0f;
    private float savedCameraY = 0f;
    private float savedCameraZ = 0f;
    private string savedAreaId = string.Empty;
    private const float SAVE_OFFSET_DISTANCE = 0.5f;
    private float RoundToTwoDecimals(float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }
    void OnPlayerTeleport(Vector3 playerPos, WorldAreaId areaId)
    {
        hasTeleported = true;
        Vector3 finalSavePos = playerPos;
        if (areaId != WorldAreaId.None)
        {
            var allDoors = UnityEngine.Object.FindObjectsByType<TeleportDoor>(UnityEngine.FindObjectsSortMode.None);
            TeleportDoor targetDoor = null;
            foreach (var door in allDoors)
            {
                if (door.identification == areaId)
                {
                    targetDoor = door;
                    break;
                }
            }
            if (targetDoor != null)
            {
                Vector3 directionVector = targetDoor.GetDirectionVector();
                if (directionVector != Vector3.zero)
                {
                    finalSavePos = playerPos + (directionVector * SAVE_OFFSET_DISTANCE);
                }
            }
        }
        savedPlayerX = RoundToTwoDecimals(finalSavePos.x);
        savedPlayerY = RoundToTwoDecimals(finalSavePos.y);
        savedPlayerZ = RoundToTwoDecimals(finalSavePos.z);
        savedAreaId = areaId != WorldAreaId.None ? areaId.ToString() : string.Empty;
        StartCoroutine(SaveAfterTeleportDelayed());
    }
    System.Collections.IEnumerator SaveAfterTeleportDelayed()
    {
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.1f);
        var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
        if (cam != null && cam.cameraFollow != null)
        {
            var camPos = cam.cameraFollow.transform.position;
            savedCameraX = RoundToTwoDecimals(camPos.x);
            savedCameraY = RoundToTwoDecimals(camPos.y);
            savedCameraZ = RoundToTwoDecimals(camPos.z);
        }
        if (ManagerSave.Instance != null && saveDefinition != null && !string.IsNullOrEmpty(saveDefinition.id))
        {
            ManagerSave.Instance.SaveSpecific(saveDefinition.id);
        }
    }
    private bool hasTeleported = false;
    public string Save(SOSaveDefinition definition)
    {
        var d = new Data();
        var player = GameObject.FindWithTag("Player");
        
        if (player != null)
        {
            var p = player.transform.position;
            d.px = RoundToTwoDecimals(p.x);
            d.py = RoundToTwoDecimals(p.y);
            d.pz = RoundToTwoDecimals(p.z);
            Debug.Log($"[SaveClientWorld] Salvando posição atual do player: ({d.px}, {d.py}, {d.pz})");
        }
        else if (hasTeleported)
        {
            d.px = savedPlayerX;
            d.py = savedPlayerY;
            d.pz = savedPlayerZ;
            Debug.Log($"[SaveClientWorld] Player não encontrado, usando posição de teleporte: ({d.px}, {d.py}, {d.pz})");
        }

        if (player != null)
        {
            var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
            if (cam != null && cam.cameraFollow != null)
            {
                var camPos = cam.cameraFollow.transform.position;
                d.cx = RoundToTwoDecimals(camPos.x);
                d.cy = RoundToTwoDecimals(camPos.y);
                d.cz = RoundToTwoDecimals(camPos.z);
            }
        }
        else if (hasTeleported)
        {
            d.cx = savedCameraX;
            d.cy = savedCameraY;
            d.cz = savedCameraZ;
        }
        else
        {
            // Fallback para câmera se nem player nem teleporte existem
            var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
            if (cam != null && cam.cameraFollow != null)
            {
                var camPos = cam.cameraFollow.transform.position;
                d.cx = RoundToTwoDecimals(camPos.x);
                d.cy = RoundToTwoDecimals(camPos.y);
                d.cz = RoundToTwoDecimals(camPos.z);
            }
        }

        if (player != null)
        {
            var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
            if (cam != null)
            {
                if (cam.CurrentArea != null && cam.CurrentArea.id != WorldAreaId.None)
                {
                    d.areaId = cam.CurrentArea.id.ToString();
                }
                else if (cam.startAreaId != WorldAreaId.None)
                {
                    d.areaId = cam.startAreaId.ToString();
                }
            }
        }
        else if (!string.IsNullOrEmpty(savedAreaId))
        {
            d.areaId = savedAreaId;
        }

        var tut = UnityEngine.Object.FindFirstObjectByType<ManagerTutorial>();
        if (tut != null)
        {
            List<string> list = tut.GetCompletedTutorialIds();
            if (list != null)
                d.tutorialsDone = list.ToArray();
        }

        string json = JsonUtility.ToJson(d);
        Debug.Log($"[SaveClientWorld] Mundo salvo com sucesso. AreaId: {d.areaId}");
        return json;
    }
    private string CreateFormattedJson(Data d)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{");
        sb.AppendFormat("\"px\":{0:F2},", d.px);
        sb.AppendFormat("\"py\":{0:F2},", d.py);
        sb.AppendFormat("\"pz\":{0:F2},", d.pz);
        sb.AppendFormat("\"cx\":{0:F2},", d.cx);
        sb.AppendFormat("\"cy\":{0:F2},", d.cy);
        sb.AppendFormat("\"cz\":{0:F2},", d.cz);
        if (!string.IsNullOrEmpty(d.areaId))
        {
            sb.AppendFormat("\"areaId\":\"{0}\",", d.areaId);
        }
        else
        {
            sb.Append("\"areaId\":null,");
        }
        if (d.tutorialsDone != null && d.tutorialsDone.Length > 0)
        {
            sb.Append("\"tutorialsDone\":[");
            for (int i = 0; i < d.tutorialsDone.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.AppendFormat("\"{0}\"", d.tutorialsDone[i]);
            }
            sb.Append("]");
        }
        else
        {
            sb.Append("\"tutorialsDone\":null");
        }
        sb.Append("}");
        return sb.ToString();
    }
    private string LoadCurrentJson(SOSaveDefinition definition)
    {
        if (ManagerSave.Instance == null || definition == null) return string.Empty;
        string filePath = Path.Combine(Application.persistentDataPath, "save.json");
        if (!File.Exists(filePath)) return string.Empty;
        try
        {
            string text = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var file = JsonUtility.FromJson<SaveFile>(text);
            if (file == null || file.entries == null) return string.Empty;
            foreach (var entry in file.entries)
            {
                if (entry.key == definition.id)
                {
                    return entry.value ?? string.Empty;
                }
            }
        }
        catch
        {
            return string.Empty;
        }
        return string.Empty;
    }
    [System.Serializable]
    class SaveFile
    {
        public List<Entry> entries = new List<Entry>();
    }
    [System.Serializable]
    class Entry
    {
        public string key;
        public string value;
    }
    public void Load(SOSaveDefinition definition, string json)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != WORLD_SCENE_NAME)
        {
            return;
        }
        if (string.IsNullOrEmpty(json))
        {
            ResetState();
            return;
        }
        var d = JsonUtility.FromJson<Data>(json);
        if (d == null)
        {
            ResetState();
            return;
        }
        savedPlayerX = RoundToTwoDecimals(d.px);
        savedPlayerY = RoundToTwoDecimals(d.py);
        savedPlayerZ = RoundToTwoDecimals(d.pz);
        savedCameraX = RoundToTwoDecimals(d.cx);
        savedCameraY = RoundToTwoDecimals(d.cy);
        savedCameraZ = RoundToTwoDecimals(d.cz);
        savedAreaId = d.areaId ?? string.Empty;
        if ((d.px != 0f || d.py != 0f || d.pz != 0f) || !string.IsNullOrEmpty(d.areaId))
        {
            hasTeleported = true;
        }
        StartCoroutine(LoadDelayed(d));
    }
    System.Collections.IEnumerator LoadDelayed(Data d)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != WORLD_SCENE_NAME)
        {
            yield break;
        }
        yield return null;
        yield return null;
        var tut = UnityEngine.Object.FindFirstObjectByType<ManagerTutorial>();
        if (tut != null && d.tutorialsDone != null)
        {
            tut.SetCompletedTutorialIds(d.tutorialsDone);
        }
        yield return null;
        yield return null;
        GameObject player = null;
        int maxFrames = 60;
        int frameCount = 0;
        while (frameCount < maxFrames)
        {
            yield return null;
            player = GameObject.FindWithTag("Player");
            if (player != null && player.activeInHierarchy)
            {
                break;
            }
            frameCount++;
        }
        if (player != null)
        {
            bool hasPositionData = (d.px != 0f || d.py != 0f || d.pz != 0f) ||
            (d.px == 0f && d.py == 0f && d.pz == 0f && !string.IsNullOrEmpty(d.areaId));
            if (!string.IsNullOrEmpty(d.areaId) || hasPositionData)
            {
                Vector3 savedPos = new Vector3(d.px, d.py, d.pz);
                player.transform.position = savedPos;
            }
        }
        var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
        if (cam != null)
        {
            if (!string.IsNullOrEmpty(d.areaId))
            {
                if (Enum.TryParse(d.areaId, out WorldAreaId parsedId) && parsedId != WorldAreaId.None)
                {
                    cam.startAreaId = parsedId;
                    cam.SetArea(parsedId);
                }
            }
            yield return null;
            if (cam.cameraFollow != null)
            {
                bool hasCameraData = (d.cx != 0f || d.cy != 0f || d.cz != 0f) ||
                (d.cx == 0f && d.cy == 0f && d.cz == 0f && !string.IsNullOrEmpty(d.areaId));
                if (!string.IsNullOrEmpty(d.areaId) || hasCameraData)
                {
                    Vector3 savedCamPos = new Vector3(d.cx, d.cy, d.cz);
                    cam.cameraFollow.transform.position = savedCamPos;
                }
            }
        }
    }
    void ResetState()
    {
        var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
        if (cam != null)
        {
            cam.startAreaId = WorldAreaId.None;
        }
        var tut = UnityEngine.Object.FindFirstObjectByType<ManagerTutorial>();
        if (tut != null)
        {
            tut.SetCompletedTutorialIds(new string[0]);
        }
    }
}

