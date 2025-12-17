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
    }
    void OnDisable()
    {
        if (ManagerSave.Instance != null && saveDefinition != null)
        {
            ManagerSave.Instance.UnregisterClient(saveDefinition, this);
        }
    }
    public string Save(SOSaveDefinition definition)
    {
        var d = new Data();
        var player = GameObject.FindWithTag("Player");
        var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
        if (player == null)
        {
            if (ManagerSave.Instance != null && definition != null)
            {
                string currentJson = LoadCurrentJson(definition);
                if (!string.IsNullOrEmpty(currentJson))
                {
                    var currentData = JsonUtility.FromJson<Data>(currentJson);
                    if (currentData != null)
                    {
                        d.px = currentData.px;
                        d.py = currentData.py;
                        d.pz = currentData.pz;
                        d.cx = currentData.cx;
                        d.cy = currentData.cy;
                        d.cz = currentData.cz;
                        d.areaId = currentData.areaId;
                    }
                }
            }
        }
        else
        {
            var p = player.transform.position;
            d.px = Mathf.Round(p.x * 100f) / 100f;
            d.py = Mathf.Round(p.y * 100f) / 100f;
            d.pz = Mathf.Round(p.z * 100f) / 100f;
            if (cam != null)
            {
                if (cam.CurrentArea != null)
                {
                    d.areaId = cam.CurrentArea.id.ToString();
                }
                else
                {
                    d.areaId = cam.startAreaId.ToString();
                }
                if (cam.cameraFollow != null)
                {
                    var camPos = cam.cameraFollow.transform.position;
                    d.cx = camPos.x;
                    d.cy = camPos.y;
                    d.cz = camPos.z;
                }
            }
        }
        var tut = UnityEngine.Object.FindFirstObjectByType<ManagerTutorial>();
        if (tut != null)
        {
            List<string> list = tut.GetCompletedTutorialIds();
            if (list != null)
            d.tutorialsDone = list.ToArray();
        }
        return JsonUtility.ToJson(d);
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
        StartCoroutine(LoadDelayed(d));
    }
    System.Collections.IEnumerator LoadDelayed(Data d)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != WORLD_SCENE_NAME)
        {
            yield break;
        }
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
        if (player == null || !player.activeInHierarchy)
        {
            yield return new WaitForSeconds(0.2f);
            player = GameObject.FindWithTag("Player");
            if (player != null && !player.activeInHierarchy)
            {
                player.SetActive(true);
            }
        }
        var cam = UnityEngine.Object.FindFirstObjectByType<ManagerCamera>();
        var tut = UnityEngine.Object.FindFirstObjectByType<ManagerTutorial>();
        var playerControl = player != null ? player.GetComponent<PlayerControl>() : null;
        if (playerControl != null) playerControl.SetControl(false);
        if (player != null)
        {
            Vector3 savedPos = new Vector3(d.px, d.py, d.pz);
            player.transform.position = savedPos;
        }
        else
        {
        }
        if (tut != null && d.tutorialsDone != null)
        {
            tut.SetCompletedTutorialIds(d.tutorialsDone);
        }
        if (playerControl != null) playerControl.SetControl(true);
        yield return null;
        RestoreCamera(cam, d);
    }
    void RestoreCamera(ManagerCamera cam, Data d)
    {
        if (cam == null) return;
        if (!string.IsNullOrEmpty(d.areaId))
        {
            if (Enum.TryParse(d.areaId, out WorldAreaId parsedId) && parsedId != WorldAreaId.None)
            {
                cam.startAreaId = parsedId;
                cam.SetArea(parsedId);
            }
        }
        if (cam.cameraFollow != null && (d.cx != 0 || d.cy != 0 || d.cz != 0))
        {
            Vector3 savedCamPos = new Vector3(d.cx, d.cy, d.cz);
            cam.cameraFollow.transform.position = savedCamPos;
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

