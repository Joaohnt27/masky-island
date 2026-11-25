using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Uma saída persistente para UI e outra para ações, usando as preferências existentes.
public class GameAudio : MonoBehaviour
{
    public enum Cue { Click, Open, Close, Pickaxe, Crystal, Success, Error, Confirm, Travel, Beacon, Footstep, DoorOpen, DoorClose, Storm }
    public static GameAudio Instance { get; private set; }
    private readonly Dictionary<Cue, AudioClip> clips = new Dictionary<Cue, AudioClip>();
    private AudioSource uiSource, worldSource;
    private bool paused;
    private AudioSource musicSource;
    private GameMusicLibrary music;
    private MainMenuController menu;
    private string sceneName;
    private float musicFade;
    public AudioClip CurrentMusic => musicSource != null ? musicSource.clip : null;
    public float MusicOutputVolume => musicSource != null ? musicSource.volume : 0f;

    public Cue LastCue { get; private set; }
    public int PlayedCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null) new GameObject("GameAudio").AddComponent<GameAudio>();
    }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
        foreach (Cue cue in Enum.GetValues(typeof(Cue))) clips[cue] = Resources.Load<AudioClip>("CartoonSfx/" + cue);
        uiSource = CreateSource("Interface effects");
        worldSource = CreateSource("World effects");
        music = Resources.Load<GameMusicLibrary>("GameMusic");
        musicSource = CreateSource("Music");musicSource.loop = true;musicSource.volume = 0;
        SceneManager.sceneLoaded += OnMusicSceneLoaded;
    }
    private AudioSource CreateSource(string label)
    {
        var go = new GameObject(label); go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0; source.volume = .7f;
        return source;
    }
    private void Start()
    {
        OnMusicSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);
        uiSource.gameObject.AddComponent<SettingsAudioChannel>().SetCategory(SettingsManager.AudioCategory.SoundEffects);
        worldSource.gameObject.AddComponent<SettingsAudioChannel>().SetCategory(SettingsManager.AudioCategory.SoundEffects);
    }
    private void Update()
    {
        UpdateMusic();
        bool next = GameManager.Instance != null && GameManager.Instance.IsPaused;
        if (next == paused) return;
        paused = next;
        if (paused) worldSource.Pause(); else worldSource.UnPause();
    }
    public static void Play(Cue cue, float gain = 1)
    {
        var audio = Instance;
        if (audio == null || !audio.clips.TryGetValue(cue, out var clip) || clip == null) return;
        bool ui = cue == Cue.Click || cue == Cue.Open || cue == Cue.Close || cue == Cue.Travel;
        if (!ui && GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        audio.LastCue = cue; audio.PlayedCount++;
        (ui ? audio.uiSource : audio.worldSource).PlayOneShot(clip, Mathf.Clamp01(gain));
    }
    private void OnMusicSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneName = scene.name;
        menu = sceneName == "MainMenu" ? FindAnyObjectByType<MainMenuController>() : null;
    }
    private void UpdateMusic()
    {
        if (music == null) return;
        bool video = menu != null && menu.transitionPanel != null && menu.transitionPanel.activeInHierarchy;
        AudioClip desired = video ? null : sceneName == "MainMenu" ? music.menu :
            GameManager.IsExplorationScene(sceneName) ? music.exploration : null;
        // A mesma faixa continua entre áreas; somente mudanças de trilha reiniciam o áudio.
        bool changing = musicSource.clip != desired;
        musicFade = Mathf.MoveTowards(musicFade, changing ? 0 : desired != null ? 1 : 0, Time.unscaledDeltaTime / .65f);
        if (changing && musicFade <= 0) {
            musicSource.Stop();musicSource.clip = desired;
            if (desired != null) musicSource.Play();
        }
        float category = SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 1;
        float pauseGain = GameManager.Instance != null && GameManager.Instance.IsPaused ? .45f : 1;
        musicSource.volume = music.gain * category * musicFade * pauseGain;
    }
    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnMusicSceneLoaded;
        if (Instance == this) Instance = null;
    }
}
