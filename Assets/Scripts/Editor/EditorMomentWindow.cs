using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class EditorMomentWindow : EditorWindow
{
    private GameObject[] cityParents = new GameObject[7];
    private List<MomentObjectInfo>[] objectsByCity = new List<MomentObjectInfo>[7];
    private List<string>[] zonesByCity = new List<string>[7];
    private Vector2 scrollPos;
    private bool showAllObjects = false;
    private int selectedTab = 0;
    private int selectedZoneTab = 0;
    private string[] tabNames = new string[] { "City 1", "City 2", "City 3", "City 4", "City 5", "City 6", "City 7" };
    private string[] momentTags = new string[] { "Moment_1", "Moment_2", "Moment_3", "Moment_4", "Moment_5" };
    private Color[] momentColors = new Color[] 
    { 
        Color.green,      // Moment_1
        Color.cyan,       // Moment_2
        Color.yellow,     // Moment_3
        new Color(1f, 0.5f, 0f), // Moment_4 (laranja)
        Color.magenta     // Moment_5
    };
    private int currentMoment = 1;
    private SaveClientMoment saveMoment;

    [System.Serializable]
    class MomentObjectInfo
    {
        public GameObject obj;
        public string currentTag;
        public bool hasMomentTag;
        public int momentNumber;
        public string fullPath;

        public MomentObjectInfo(GameObject gameObject, string tag)
        {
            obj = gameObject;
            currentTag = tag;
            hasMomentTag = IsMomentTag(tag);
            if (hasMomentTag)
            {
                momentNumber = ExtractMomentNumber(tag);
            }
            fullPath = GetFullPath(gameObject.transform);
        }

        static bool IsMomentTag(string tag)
        {
            return tag.StartsWith("Moment_") && tag.Length > 7;
        }

        static int ExtractMomentNumber(string tag)
        {
            if (IsMomentTag(tag))
            {
                string numberStr = tag.Substring(7);
                if (int.TryParse(numberStr, out int num))
                {
                    return num;
                }
            }
            return 0;
        }

        static string GetFullPath(Transform transform)
        {
            if (transform == null) return "";
            string path = transform.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                string parentName = parent.name;
                if (!parentName.StartsWith("World_City_") && !parentName.StartsWith("Zone_"))
                {
                    path = parentName + "/" + path;
                }
                parent = parent.parent;
            }
            return path;
        }
        
        public string GetZoneName()
        {
            if (obj == null) return "";
            Transform current = obj.transform;
            while (current != null)
            {
                if (current.name.StartsWith("Zone_"))
                {
                    return current.name;
                }
                current = current.parent;
            }
            return "";
        }
    }

    [MenuItem("Tools/Central de Moments")]
    public static void ShowWindow()
    {
        GetWindow<EditorMomentWindow>("Central de Moments");
    }

    private void OnEnable()
    {
        RefreshCityParents();
        RefreshObjectLists();
        FindSaveMoment();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Central de Moments", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Atualizar Lista", GUILayout.Width(120)))
        {
            RefreshCityParents();
            RefreshObjectLists();
            FindSaveMoment();
        }

        bool oldShowAll = showAllObjects;
        showAllObjects = EditorGUILayout.Toggle("Mostrar Todos os Objetos", showAllObjects);
        if (oldShowAll != showAllObjects)
        {
            RefreshObjectLists();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (saveMoment != null && Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Momento Atual:", GUILayout.Width(100));
            currentMoment = EditorGUILayout.IntSlider(currentMoment, 1, 5, GUILayout.Width(200));
            if (GUILayout.Button("Aplicar", GUILayout.Width(80)))
            {
                saveMoment.SetMoment(currentMoment);
            }
            EditorGUILayout.LabelField($"→ {saveMoment.GetMoment()}", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }
        else if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("SaveClientMoment não encontrado na cena.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        int oldSelectedTab = selectedTab;
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        if (oldSelectedTab != selectedTab)
        {
            selectedZoneTab = 0;
        }

        if (selectedTab >= 0 && selectedTab < 7)
        {
            DrawZoneFilter(selectedTab);
            EditorGUILayout.Space();
            DrawCityTab(selectedTab);
        }
    }

    void DrawZoneFilter(int cityIndex)
    {
        if (cityIndex < 0 || cityIndex >= 7) return;
        if (zonesByCity[cityIndex] == null || zonesByCity[cityIndex].Count == 0)
        {
            return;
        }

        List<string> zones = zonesByCity[cityIndex];
        List<string> zoneNamesWithAll = new List<string> { "Todos" };
        foreach (string zone in zones)
        {
            if (zone.StartsWith("Zone_"))
            {
                zoneNamesWithAll.Add(zone);
            }
        }
        
        EditorGUILayout.BeginHorizontal();
        
        float buttonWidth = 80f;
        float availableWidth = EditorGUIUtility.currentViewWidth - 20f;
        int buttonsPerRow = Mathf.Max(1, Mathf.FloorToInt(availableWidth / buttonWidth));
        
        int buttonIndex = 0;
        for (int i = 0; i < zoneNamesWithAll.Count; i++)
        {
            if (buttonIndex > 0 && buttonIndex % buttonsPerRow == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
            
            string displayName = zoneNamesWithAll[i];
            if (displayName.StartsWith("Zone_"))
            {
                int underscoreIndex = displayName.IndexOf('_', 5);
                if (underscoreIndex > 0)
                {
                    string numberPart = displayName.Substring(5, underscoreIndex - 5);
                    displayName = "Zone " + numberPart;
                }
                else
                {
                    string numberPart = displayName.Substring(5);
                    displayName = "Zone " + numberPart;
                }
            }
            
            bool isSelected = (selectedZoneTab == i);
            GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
            if (GUILayout.Button(displayName, EditorStyles.toolbarButton, GUILayout.Width(buttonWidth)))
            {
                selectedZoneTab = i;
            }
            GUI.backgroundColor = Color.white;
            
            buttonIndex++;
        }
        
        EditorGUILayout.EndHorizontal();
    }

    void DrawCityTab(int cityIndex)
    {
        if (cityIndex < 0 || cityIndex >= 7) return;
        if (objectsByCity[cityIndex] == null)
        {
            EditorGUILayout.HelpBox($"World_City_{cityIndex + 1} não encontrado ou não configurado.", MessageType.Info);
            return;
        }

        List<MomentObjectInfo> objects = objectsByCity[cityIndex];
        if (objects.Count == 0)
        {
            EditorGUILayout.HelpBox($"Nenhum objeto encontrado em World_City_{cityIndex + 1}.", MessageType.Info);
            return;
        }

        string selectedZone = "";
        if (zonesByCity[cityIndex] != null && zonesByCity[cityIndex].Count > 0 && selectedZoneTab > 0 && selectedZoneTab <= zonesByCity[cityIndex].Count)
        {
            selectedZone = zonesByCity[cityIndex][selectedZoneTab - 1];
        }

        int filteredCount = 0;
        foreach (MomentObjectInfo info in objects)
        {
            if (info.obj == null) continue;
            bool shouldShow = showAllObjects || info.hasMomentTag;
            if (!shouldShow) continue;
            
            if (!string.IsNullOrEmpty(selectedZone))
            {
                string objZone = info.GetZoneName();
                if (objZone != selectedZone) continue;
            }
            filteredCount++;
        }

        EditorGUILayout.LabelField($"Total: {filteredCount} objetos", EditorStyles.miniLabel);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (MomentObjectInfo info in objects)
        {
            if (info.obj == null) continue;

            bool shouldShow = showAllObjects || info.hasMomentTag;
            if (!shouldShow) continue;
            
            if (!string.IsNullOrEmpty(selectedZone))
            {
                string objZone = info.GetZoneName();
                if (objZone != selectedZone) continue;
            }

            Color originalColor = GUI.color;
            Color backgroundColor = Color.red;
            if (info.hasMomentTag && info.momentNumber >= 1 && info.momentNumber <= 5)
            {
                backgroundColor = momentColors[info.momentNumber - 1];
            }

            GUI.backgroundColor = backgroundColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            GUI.color = originalColor;

            EditorGUILayout.LabelField(info.fullPath, GUILayout.ExpandWidth(true));

            string currentTagDisplay = info.hasMomentTag ? info.currentTag : "Sem Tag";
            EditorGUILayout.LabelField(currentTagDisplay, GUILayout.Width(100));

            int selectedTagIndex = 0;
            if (info.hasMomentTag)
            {
                for (int i = 0; i < momentTags.Length; i++)
                {
                    if (momentTags[i] == info.currentTag)
                    {
                        selectedTagIndex = i + 1;
                        break;
                    }
                }
            }

            Rect popupRect = GUILayoutUtility.GetRect(120, EditorGUIUtility.singleLineHeight, GUILayout.Width(120));
            EditorGUI.BeginChangeCheck();
            int newTagIndex = EditorGUI.Popup(popupRect, selectedTagIndex, GetTagOptions());
            if (EditorGUI.EndChangeCheck() && newTagIndex != selectedTagIndex && newTagIndex >= 0)
            {
                if (newTagIndex == 0)
                {
                    info.obj.tag = "Untagged";
                }
                else
                {
                    info.obj.tag = momentTags[newTagIndex - 1];
                }
                EditorUtility.SetDirty(info.obj);
                RefreshObjectLists();
                break;
            }

            if (GUILayout.Button("Selecionar", GUILayout.Width(80)))
            {
                Selection.activeGameObject = info.obj;
                EditorGUIUtility.PingObject(info.obj);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    string[] GetTagOptions()
    {
        List<string> options = new List<string> { "Sem Tag" };
        options.AddRange(momentTags);
        return options.ToArray();
    }

    void RefreshCityParents()
    {
        ManagerGameFlow gameFlow = FindFirstObjectByType<ManagerGameFlow>();
        if (gameFlow != null && gameFlow.cityParents != null)
        {
            for (int i = 0; i < 7 && i < gameFlow.cityParents.Length; i++)
            {
                cityParents[i] = gameFlow.cityParents[i];
            }
        }
        else
        {
            for (int i = 0; i < 7; i++)
            {
                cityParents[i] = GameObject.Find($"World_City_{i + 1}");
            }
        }
    }

    void RefreshObjectLists()
    {
        for (int i = 0; i < 7; i++)
        {
            objectsByCity[i] = new List<MomentObjectInfo>();
            zonesByCity[i] = new List<string>();
            if (cityParents[i] == null) continue;

            CollectZones(cityParents[i].transform, zonesByCity[i]);
            CollectObjectsRecursive(cityParents[i].transform, objectsByCity[i]);
        }
    }

    void CollectZones(Transform parent, List<string> zones)
    {
        if (parent == null) return;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == null) continue;

            if (child.name.StartsWith("Zone_"))
            {
                if (!zones.Contains(child.name))
                {
                    zones.Add(child.name);
                }
            }

            CollectZones(child, zones);
        }
    }

    void CollectObjectsRecursive(Transform parent, List<MomentObjectInfo> list)
    {
        if (parent == null) return;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == null) continue;

            GameObject obj = child.gameObject;
            string tag = obj.tag;
            bool isMomentTag = tag.StartsWith("Moment_");

            if (isMomentTag || showAllObjects)
            {
                list.Add(new MomentObjectInfo(obj, tag));
            }

            CollectObjectsRecursive(child, list);
        }
    }

    void FindSaveMoment()
    {
        saveMoment = FindFirstObjectByType<SaveClientMoment>();
        if (saveMoment != null && Application.isPlaying)
        {
            currentMoment = saveMoment.GetMoment();
        }
    }

    void Update()
    {
        if (Application.isPlaying && saveMoment != null)
        {
            int newMoment = saveMoment.GetMoment();
            if (newMoment != currentMoment)
            {
                currentMoment = newMoment;
                Repaint();
            }
        }
    }
}

