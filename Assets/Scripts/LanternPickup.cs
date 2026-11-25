using UnityEngine;

public class LanternPickup : Interactable
{
    [SerializeField] private DialogueController dialogue;
    [SerializeField] private SpriteRenderer visual;
    private Collider hitbox;
    protected override void Start() { base.Start(); hitbox = GetComponent<Collider>(); }
    protected override void Update()
    {
        var gm = GameManager.Instance;
        bool visible = gm != null && gm.HasMemory(GameManager.MemoryId.LighthouseKeeper) && !gm.HasLantern;
        visual.enabled = visible;
        if (hitbox != null) hitbox.enabled = visible;
        if (visible) base.Update();
    }
    public override void Interact()
    {
        var gm = GameManager.Instance;
        var player = FindAnyObjectByType<PlayerClickMovement>();
        if (gm == null || gm.HasLantern || !gm.HasMemory(GameManager.MemoryId.LighthouseKeeper) ||
            gm.BlocksGameplay || player == null || !player.IsMovementEnabled || !IsInReach(player.transform)) return;
        if (!gm.HasBackpack)
        {
            dialogue.ShowThought(new[] { "Preciso pegar a mochila ao lado da cama para levar o lampião." }); return;
        }
        dialogue.ShowThought(new[] {
            "Aqui está o lampião da minha lembrança! Está apagado, mas ainda tem combustível.",
            "Vou levá-lo comigo. Aperto E para acender e afastar a Sombra quando ela se aproximar. E também apaga a luz."
        }, () => { if (gm.TryCollectLantern()) GameAudio.Play(GameAudio.Cue.Success); });
    }
}
