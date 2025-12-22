using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class ManagerAudio : MonoBehaviour
{
    public static ManagerAudio Instance { get; private set; }
    [Header("ConfiguraÃ§Ã£o")]
    public SOAudioConfig audioConfig;
    public AudioMixer mainMixer;
    public SaveClientSettings settingsClient;
    private AudioSource musicSource;
    private AudioSource sfxSource;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        if (mainMixer != null)
        {
            var musicGroups = mainMixer.FindMatchingGroups("Music");
            if (musicGroups.Length > 0) musicSource.outputAudioMixerGroup = musicGroups[0];
            var sfxGroups = mainMixer.FindMatchingGroups("SFX");
            if (sfxGroups.Length > 0) sfxSource.outputAudioMixerGroup = sfxGroups[0];
        }
        SceneManager.activeSceneChanged += OnSceneChanged;
    }
    void Start()
    {
        if (settingsClient == null)
        settingsClient = FindFirstObjectByType<SaveClientSettings>();
        if (settingsClient != null)
        {
            SetMusicVolume(settingsClient.musicVolume);
            SetSFXVolume(settingsClient.sfxVolume);
        }
        CheckSceneMusic(SceneManager.GetActiveScene());
    }
    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
    void OnEnable()
    {
        GameEvents.OnCardPlayed += OnCardPlayed;
        GameEvents.OnCardsCaptured += OnCardsCaptured;
        GameEvents.OnMatchFinished += OnMatchFinished;
    }
    void OnDisable()
    {
        GameEvents.OnCardPlayed -= OnCardPlayed;
        GameEvents.OnCardsCaptured -= OnCardsCaptured;
        GameEvents.OnMatchFinished -= OnMatchFinished;
    }
    public void SetMusicVolume(float value)
    {
        if (mainMixer == null) return;
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        mainMixer.SetFloat("MusicVol", db);
    }
    public void SetSFXVolume(float value)
    {
        if (mainMixer == null) return;
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        mainMixer.SetFloat("SFXVol", db);
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }
    void OnSceneChanged(Scene current, Scene next)
    {
        CheckSceneMusic(next);
    }
    void CheckSceneMusic(Scene scene)
    {
        if (audioConfig == null) return;
        AudioClip targetClip = null;
        if (scene.name == "MainMenu") targetClip = audioConfig.mainMenuMusic;
        else if (scene.name == "World") targetClip = audioConfig.worldTheme;
        else if (scene.name == "CardGame") targetClip = audioConfig.cardGameTheme;
        if (targetClip != null)
        PlayMusic(targetClip);
    }
    void OnCardPlayed(CardSlot slot, SOCardData card, int ownerId)
    {
        if (audioConfig) PlaySFX(audioConfig.cardPlace);
    }
    void OnCardsCaptured(List<CardSlot> slots, int ownerId)
    {
        if (audioConfig) PlaySFX(audioConfig.cardCapture);
    }
    void OnMatchFinished(MatchResult result, int pScore, int oScore)
    {
        if (audioConfig == null) return;
        if (result == MatchResult.PlayerWin)
        PlaySFX(audioConfig.matchWin);
        else if (result == MatchResult.OpponentWin)
        PlaySFX(audioConfig.matchLose);
        else
        PlaySFX(audioConfig.matchDraw);
    }
}

