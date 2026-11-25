using UnityEngine;

// Referências aos arquivos originais; evita duplicar MP3s em Resources.
public class GameMusicLibrary : ScriptableObject
{
    public AudioClip menu;
    public AudioClip exploration;
    [Range(0,1)] public float gain = .35f;
}
