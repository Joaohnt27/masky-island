using System;
using System.Collections.Generic;
using UnityEngine;

// Preferências locais, independentes da missão e das interfaces de cada cena.
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    public enum AudioCategory { Music, SoundEffects }

    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public bool Fullscreen { get; private set; }
    public int ResolutionIndex { get; private set; }
    public int QualityLevel { get; private set; }
    public IReadOnlyList<Vector2Int> Resolutions => resolutions;
    public event Action OnAudioChanged;

    private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
    private bool dirty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
            new GameObject("SettingsManager").AddComponent<SettingsManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        MasterVolume = ReadVolume("MasterVolume");
        MusicVolume = ReadVolume("MusicVolume");
        SFXVolume = ReadVolume("SFXVolume");
        AudioListener.volume = MasterVolume;
        RefreshResolutions();
        Fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) != 0;
        QualityLevel = Mathf.Clamp(PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel()),
            0, Mathf.Max(0, QualitySettings.names.Length - 1));
        int width = PlayerPrefs.GetInt("ResolutionWidth", Screen.width);
        int height = PlayerPrefs.GetInt("ResolutionHeight", Screen.height);
        int savedIndex = resolutions.IndexOf(new Vector2Int(width, height));
        if (savedIndex >= 0)
            ResolutionIndex = savedIndex;
        if (PlayerPrefs.HasKey("QualityLevel"))
            QualitySettings.SetQualityLevel(QualityLevel, true);
        if (PlayerPrefs.HasKey("ResolutionWidth") || PlayerPrefs.HasKey("Fullscreen"))
            ApplyScreen();
    }

    private static float ReadVolume(string key)
    {
        float value = PlayerPrefs.GetFloat(key, 1f);
        return float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Clamp01(value);
    }

    public void RefreshResolutions()
    {
        resolutions.Clear();
        foreach (Resolution resolution in Screen.resolutions)
        {
            var size = new Vector2Int(resolution.width, resolution.height);
            if (!resolutions.Contains(size))
                resolutions.Add(size);
        }
        // A janela atual também é uma opção válida, inclusive quando o Editor não lista modos.
        var current = new Vector2Int(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));
        if (!resolutions.Contains(current))
            resolutions.Add(current);
        ResolutionIndex = resolutions.IndexOf(current);
    }

    public float CategoryVolume(AudioCategory category) =>
        category == AudioCategory.Music ? MusicVolume : SFXVolume;

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        AudioListener.volume = MasterVolume;
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        AudioChanged();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        AudioChanged();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
        AudioChanged();
    }

    private void AudioChanged()
    {
        dirty = true;
        OnAudioChanged?.Invoke();
    }

    public void ApplyVideo(int resolutionIndex, bool fullscreen, int qualityLevel)
    {
        ResolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Count - 1);
        Fullscreen = fullscreen;
        QualityLevel = Mathf.Clamp(qualityLevel, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        QualitySettings.SetQualityLevel(QualityLevel, true);
        ApplyScreen();
        Vector2Int size = resolutions[ResolutionIndex];
        PlayerPrefs.SetInt("ResolutionWidth", size.x);
        PlayerPrefs.SetInt("ResolutionHeight", size.y);
        PlayerPrefs.SetInt("Fullscreen", Fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", QualityLevel);
        dirty = true;
        Save();
    }

    private void ApplyScreen()
    {
        Vector2Int size = resolutions[ResolutionIndex];
        Screen.SetResolution(size.x, size.y,
            Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
    }

    public void Save()
    {
        if (!dirty)
            return;
        PlayerPrefs.Save();
        dirty = false;
    }

    private void OnApplicationPause(bool paused) { if (paused) Save(); }
    private void OnApplicationQuit() => Save();
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
