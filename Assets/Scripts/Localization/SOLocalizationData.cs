using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Config/Localization Data")]
public class SOLocalizationData : ScriptableObject
{
    [System.Serializable]
    public class LanguageEntry
    {
        public string languageCode;
        public string[] keys;
        public string[] texts;
    }
    [System.Serializable]
    public class Sheet
    {
        public string sheetName;
        public List<LanguageEntry> entries = new List<LanguageEntry>();
    }
    public List<Sheet> sheets = new List<Sheet>();
    [HideInInspector] public List<LanguageEntry> entries_DEPRECATED = new List<LanguageEntry>();
}

