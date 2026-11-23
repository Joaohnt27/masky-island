using UnityEngine;
using UnityEngine.Video;

// Adicionar às futuras músicas/efeitos e selecionar a categoria no Inspector.
// O volume original é preservado e multiplicado pela preferência da categoria.
public class SettingsAudioChannel : MonoBehaviour
{
    [SerializeField] private SettingsManager.AudioCategory category;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private VideoPlayer videoPlayer;
    private SettingsManager settings;
    private float baseVolume;
    private float[] videoVolumes;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        if (audioSource != null) baseVolume = audioSource.volume;
        if (videoPlayer != null)
        {
            videoVolumes = new float[videoPlayer.controlledAudioTrackCount];
            for (ushort i = 0; i < videoVolumes.Length; i++)
                videoVolumes[i] = videoPlayer.GetDirectAudioVolume(i);
        }
    }

    private void OnEnable()
    {
        settings = SettingsManager.Instance;
        if (settings != null) settings.OnAudioChanged += ApplyVolume;
        if (videoPlayer != null) videoPlayer.prepareCompleted += OnVideoPrepared;
        ApplyVolume();
    }

    public void SetCategory(SettingsManager.AudioCategory value)
    {
        category = value;
        ApplyVolume();
    }

    private void OnVideoPrepared(VideoPlayer player) => ApplyVolume();

    private void ApplyVolume()
    {
        if (settings == null) return;
        float categoryVolume = settings.CategoryVolume(category);
        if (audioSource != null)
            audioSource.volume = baseVolume * categoryVolume *
                (audioSource.ignoreListenerVolume ? settings.MasterVolume : 1f);
        // A saída Direct do vídeo não passa pelo AudioListener.
        if (videoPlayer != null && videoPlayer.audioOutputMode == VideoAudioOutputMode.Direct)
            for (ushort i = 0; i < videoVolumes.Length; i++)
                videoPlayer.SetDirectAudioVolume(i, videoVolumes[i] * categoryVolume * settings.MasterVolume);
    }

    private void OnDisable()
    {
        if (settings != null) settings.OnAudioChanged -= ApplyVolume;
        if (videoPlayer != null) videoPlayer.prepareCompleted -= OnVideoPrepared;
    }
}
