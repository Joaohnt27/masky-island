using UnityEngine;

public class JournalPickup : Interactable
{
    [SerializeField] private DialogueController dialogue;
    [SerializeField] private SpriteRenderer book;
    private Collider hitbox;
    protected override void Start() { base.Start(); hitbox=GetComponent<Collider>(); }
    protected override void Update()
    {
        bool available=GameManager.Instance!=null&&!GameManager.Instance.HasJournal;
        book.enabled=available;
        if(hitbox!=null)hitbox.enabled=available;
        if(available)base.Update();
    }
    public override void Interact()
    {
        var mission=GameManager.Instance;
        if(mission==null||mission.HasJournal||mission.BlocksGameplay)return;
        dialogue.ShowThought(new[] {
            "É um diário... mas todas as páginas estão vazias.",
            "Vou levá-lo comigo e anotar minha primeira tarefa: ir até aquela casa procurar ajuda."
        },()=> {
            mission.CollectJournal();
            mission.LearnJournal();
            GameAudio.Play(GameAudio.Cue.Success);
        });
    }
}
