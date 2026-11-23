using UnityEngine;

public class BackpackPickup : Interactable
{
    [SerializeField] private DialogueController dialogue;
    [SerializeField] private SpriteRenderer backpack;
    private Collider hitbox;
    protected override void Start() { base.Start(); hitbox=GetComponent<Collider>(); }
    protected override void Update()
    {
        bool visible=GameManager.Instance!=null&&!GameManager.Instance.HasBackpack;
        backpack.enabled=visible;
        if(hitbox!=null)hitbox.enabled=visible;
        if(visible)base.Update();
    }
    public override void Interact()
    {
        var mission=GameManager.Instance;
        if(mission==null||mission.HasBackpack||mission.BlocksGameplay)return;
        if(!mission.HasMissionStarted)
        {
            dialogue.ShowThought(new[]{"Uma mochila... deve pertencer a quem mora aqui. Melhor falar com o dono antes de pegar."});
            return;
        }
        dialogue.ShowThought(new[]{"É a mochila que o estranho me ofereceu. Agora tenho onde guardar os cristais!"},()=> {
            if(mission.CollectBackpack())GameAudio.Play(GameAudio.Cue.Success);
        });
    }
}
