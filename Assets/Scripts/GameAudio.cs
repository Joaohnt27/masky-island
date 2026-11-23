using System;
using System.Collections.Generic;
using UnityEngine;

// Uma saída persistente para UI e outra para ações, usando as preferências existentes.
public class GameAudio : MonoBehaviour
{
    public enum Cue { Click, Open, Close, Pickaxe, Crystal, Success, Error, Confirm, Travel, Beacon, Footstep, DoorOpen, DoorClose }
    public static GameAudio Instance { get; private set; }
    private readonly Dictionary<Cue, AudioClip> clips = new Dictionary<Cue, AudioClip>();
    private AudioSource uiSource, worldSource;
    private bool paused;
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
    }
    private AudioSource CreateSource(string label)
    {
        var go = new GameObject(label); go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0; source.volume = .7f;
        return source;
    }
    private void Start()
    {
        uiSource.gameObject.AddComponent<SettingsAudioChannel>().SetCategory(SettingsManager.AudioCategory.SoundEffects);
        worldSource.gameObject.AddComponent<SettingsAudioChannel>().SetCategory(SettingsManager.AudioCategory.SoundEffects);
    }
    private void Update()
    {
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
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
