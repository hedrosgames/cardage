using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class EditorUIFeelWindow : EditorWindow
{
    class UIElementData
    {
        public GameObject gameObject;
        public Button button;
        public Image image;
        public TMP_Text text;
        public UIFeel feel;
    }
    enum ElementFilter
    {
        All,
        Button,
        Image,
        Text
    }
    enum SortMode
    {
        Name,
        TypeThenName
    }
    Vector2 scroll;
    List<UIElementData> elements = new List<UIElementData>();
    ElementFilter filter = ElementFilter.All;
    bool showWithFeel = true;
    bool showWithoutFeel = true;
    SortMode sortMode = SortMode.TypeThenName;
    string search = "";
    [MenuItem("Central de Configuração/Central de Feel")]
    public static void ShowWindow()
    {
        GetWindow<EditorUIFeelWindow>("UI Feel Manager");
    }
    void OnEnable()
    {
        RefreshList();
    }
    void OnHierarchyChange()
    {
        Repaint();
    }
    void RefreshList()
    {
        elements.Clear();
        var map = new Dictionary<GameObject, UIElementData>();
        foreach (var b in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (!b.gameObject.scene.IsValid()) continue;
            AddOrUpdate(map, b.gameObject).button = b;
        }
        foreach (var img in Resources.FindObjectsOfTypeAll<Image>())
        {
            if (!img.gameObject.scene.IsValid()) continue;
            AddOrUpdate(map, img.gameObject).image = img;
        }
        foreach (var txt in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (!txt.gameObject.scene.IsValid()) continue;
            AddOrUpdate(map, txt.gameObject).text = txt;
        }
        foreach (var kv in map)
        {
            kv.Value.feel = kv.Key.GetComponent<UIFeel>();
            elements.Add(kv.Value);
        }
        SortElements();
    }
    void SortElements()
    {
        elements.Sort((a, b) =>
        {
            if (sortMode == SortMode.Name)
            {
                return string.Compare(a.gameObject.name, b.gameObject.name, System.StringComparison.Ordinal);
            }
            int ta = GetTypeIndex(a);
            int tb = GetTypeIndex(b);
            int tComp = ta.CompareTo(tb);
            if (tComp != 0) return tComp;
            return string.Compare(a.gameObject.name, b.gameObject.name, System.StringComparison.Ordinal);
        });
    }
    int GetTypeIndex(UIElementData d)
    {
        if (d.button != null) return 0;
        if (d.image != null) return 1;
        if (d.text != null) return 2;
        return 3;
    }
    UIElementData AddOrUpdate(Dictionary<GameObject, UIElementData> map, GameObject go)
    {
        UIElementData data;
        if (!map.TryGetValue(go, out data))
        {
            data = new UIElementData { gameObject = go };
            map.Add(go, data);
        }
        return data;
    }
    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Recarregar", GUILayout.Width(100)))
        {
            RefreshList();
        }
        GUILayout.Space(10);
        EditorGUILayout.LabelField($"Elementos: {elements.Count}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filtro tipo", GUILayout.Width(70));
        filter = (ElementFilter)EditorGUILayout.EnumPopup(filter, GUILayout.Width(110));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Mostrar", GUILayout.Width(50));
        showWithFeel = EditorGUILayout.ToggleLeft("Com Feel", showWithFeel, GUILayout.Width(90));
        showWithoutFeel = EditorGUILayout.ToggleLeft("Sem Feel", showWithoutFeel, GUILayout.Width(90));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Ordenar", GUILayout.Width(50));
        sortMode = (SortMode)EditorGUILayout.EnumPopup(sortMode, GUILayout.Width(130));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Busca", GUILayout.Width(45));
        search = EditorGUILayout.TextField(search);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(6);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var e in elements)
        {
            if (!PassesFilter(e)) continue;
            DrawElementRow(e);
        }
        EditorGUILayout.EndScrollView();
    }
    bool PassesFilter(UIElementData d)
    {
        if (d.gameObject == null) return false;
        if (!string.IsNullOrEmpty(search))
        {
            if (!d.gameObject.name.ToLower().Contains(search.ToLower()))
            return false;
        }
        bool isButton = d.button != null;
        bool isImage = d.image != null;
        bool isText = d.text != null;
        switch (filter)
        {
            case ElementFilter.Button:
            if (!isButton) return false;
            break;
            case ElementFilter.Image:
            if (!isImage) return false;
            break;
            case ElementFilter.Text:
            if (!isText) return false;
            break;
        }
        if (!showWithFeel && d.feel != null) return false;
        if (!showWithoutFeel && d.feel == null) return false;
        return true;
    }
    void DrawElementRow(UIElementData data)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(data.gameObject.name, EditorStyles.boldLabel);
        string typeLabel = "";
        if (data.button != null) typeLabel += "[Button] ";
        if (data.image != null) typeLabel += "[Image] ";
        if (data.text != null) typeLabel += "[Text] ";
        EditorGUILayout.LabelField(typeLabel, EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (data.feel != null)
        {
            EditorGUI.BeginChangeCheck();
            var newMask = (UIFeelEffect)EditorGUILayout.EnumFlagsField(data.feel.effects, GUILayout.Width(220));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(data.feel, "Change UIFeel Effects");
                data.feel.effects = newMask;
                EditorUtility.SetDirty(data.feel);
            }
            if (GUILayout.Button("Remover", GUILayout.Width(70)))
            {
                Undo.DestroyObjectImmediate(data.feel);
                data.feel = null;
            }
        }
        else
        {
            if (GUILayout.Button("Adicionar Feel", GUILayout.Width(120)))
            {
                var comp = Undo.AddComponent<UIFeel>(data.gameObject);
                data.feel = comp;
                EditorUtility.SetDirty(data.gameObject);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
