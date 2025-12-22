using UnityEngine;
[CreateAssetMenu(menuName = "Config/Audio")]
public class SOAudioConfig : ScriptableObject
{
    [Header("Música")]
    public AudioClip mainMenuMusic;
    public AudioClip worldTheme;
    public AudioClip cardGameTheme;
    [Header("UI SFX")]
    public AudioClip uiClick;
    public AudioClip uiHover;
    public AudioClip uiBack;
    [Header("Gameplay SFX")]
    public AudioClip cardDraw;
    public AudioClip cardPlace;
    public AudioClip cardCapture;
    public AudioClip matchWin;
    public AudioClip matchLose;
    public AudioClip matchDraw;
}

