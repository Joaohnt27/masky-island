using UnityEngine;

// Usa o hover e o destino já calculados pelo movimento, sem um segundo raycast.
[RequireComponent(typeof(DoorInteraction))]
public class AnimatedDoor : MonoBehaviour
{
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private SpriteRenderer portal;
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(.05f)] private float openingDuration = .5f;
    [SerializeField, Min(.05f)] private float closingDuration = .4f;
    private DoorInteraction door;
    private float progress;
    private bool wantsOpen;
    private float nextSoundTime;
    public float Progress => progress;
    public bool IsFullyOpen => frames == null || frames.Length < 2 || portal == null || progress >= 1f;

    private void Awake() { door = GetComponent<DoorInteraction>(); Draw(); }

    private void LateUpdate()
    {
        if (Time.deltaTime <= 0f || SceneFade.IsRunning ||
            (GameManager.Instance != null && GameManager.Instance.IsPaused)) return;
        bool open = player != null && player.IsMovementEnabled &&
            (player.HoveredDoor == door || player.TargetDoor == door);
        if (open != wantsOpen)
        {
            wantsOpen = open;
            if (Time.unscaledTime >= nextSoundTime && (open || progress > 0f))
            {
                GameAudio.Play(open ? GameAudio.Cue.DoorOpen : GameAudio.Cue.DoorClose, .65f);
                nextSoundTime = Time.unscaledTime + .25f;
            }
        }
        progress = Mathf.MoveTowards(progress, open ? 1f : 0f,
            Time.deltaTime / Mathf.Max(.05f, open ? openingDuration : closingDuration));
        Draw();
    }

    private void Draw()
    {
        if (portal != null && frames != null && frames.Length > 0)
            portal.sprite = frames[Mathf.RoundToInt(progress * (frames.Length - 1))];
    }

    private void OnDisable() { progress = 0; wantsOpen = false; Draw(); }
}
