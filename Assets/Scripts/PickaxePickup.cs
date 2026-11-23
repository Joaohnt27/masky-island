using UnityEngine;

public class PickaxePickup : Interactable
{
    [SerializeField] private DialogueController dialogue;
    [SerializeField] private SpriteRenderer tool;
    private Collider hitbox;
    protected override void Start() { base.Start(); hitbox=GetComponent<Collider>(); }
    protected override void Update()
    {
        bool visible=GameManager.Instance!=null&&!GameManager.Instance.HasPickaxe;
        tool.enabled=visible;
        if(hitbox!=null)hitbox.enabled=visible;
        if(visible)base.Update();
    }
    public override void Interact()
    {
        var mission=GameManager.Instance;
        if(mission==null||mission.HasPickaxe||mission.BlocksGameplay)return;
        if(!mission.HasMissionStarted||!mission.HasBackpack)
        {
            dialogue.ShowThought(new[]{"Uma picareta! Preciso buscar a mochila na casa para levar meu equipamento."});
            return;
        }
        dialogue.ShowThought(new[]{"Aqui está a picareta que o estranho mencionou. Agora posso procurar cristais nas rochas!"},()=> {
            if(mission.CollectPickaxe())GameAudio.Play(GameAudio.Cue.Success);
        });
    }
}
