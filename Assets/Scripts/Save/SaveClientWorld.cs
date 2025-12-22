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
        TryRegister();
        GameEvents.OnPlayerTeleport += OnPlayerTeleport;
    }
    void Start()
    {
        TryRegister();
    }
    void TryRegister()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.RegisterClient(saveDefinition, this);
        }
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
    public string Save(SOSaveDefinition definition)
    {
        TryRegister();
        var d = new Data();
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var p = player.transform.position;
            savedPlayerX = RoundToTwoDecimals(p.x);
            savedPlayerY = RoundToTwoDecimals(p.y);
            savedPlayerZ = RoundToTwoDecimals(p.z);
            var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
            if (cam != null && cam.cameraFollow != null)
            {
                var camPos = cam.cameraFollow.transform.position;
                savedCameraX = RoundToTwoDecimals(camPos.x);
                savedCameraY = RoundToTwoDecimals(camPos.y);
                savedCameraZ = RoundToTwoDecimals(camPos.z);
            }
            if (cam != null)
            {
                if (cam.CurrentArea != null && cam.CurrentArea.id != WorldAreaId.None)
                {
                    savedAreaId = cam.CurrentArea.id.ToString();
                }
            }
        }
        d.px = savedPlayerX;
        d.py = savedPlayerY;
        d.pz = savedPlayerZ;
        d.cx = savedCameraX;
        d.cy = savedCameraY;
        d.cz = savedCameraZ;
        d.areaId = savedAreaId;
        var tut = UnityEngine.Object.FindFirstObjectByType<ManagerTutorial>();
        if (tut != null)
        {
            List<string> list = tut.GetCompletedTutorialIds();
            if (list != null)
            d.tutorialsDone = list.ToArray();
        }
        return JsonUtility.ToJson(d);
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

