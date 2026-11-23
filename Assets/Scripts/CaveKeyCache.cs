using System.Collections;
using UnityEngine;
public class CaveKeyCache : Interactable
{
 [SerializeField] private SpriteRenderer hatch;
 [SerializeField] private Sprite closedSprite, openSprite;
 [SerializeField] private GameObject loot;
 [SerializeField] private DialogueController dialogue;
 private Collider hitbox;
 private GameManager mission;
 private bool opened, opening;
 protected override void Start(){base.Start();hitbox=GetComponent<Collider>();mission=GameManager.Instance;opened=mission!=null&&mission.IsCaveCacheOpen;if(mission!=null)mission.OnMissionStateChanged+=Refresh;Refresh();}
 private void Refresh(){if(mission==null)return;if(!mission.IsCaveCacheOpen){StopAllCoroutines();opening=false;opened=false;hatch.transform.localScale=Vector3.one;}
 else if(!opened&&!opening)StartCoroutine(Open());
 hatch.sprite=opened?openSprite:closedSprite;loot.SetActive(opened&&!mission.HasCollectedKeyMaterials);if(hitbox!=null)hitbox.enabled=opened&&!mission.HasCollectedKeyMaterials;}
 private IEnumerator Open(){opening=true;GameAudio.Play(GameAudio.Cue.DoorOpen);float elapsed=0;while(elapsed<.75f){elapsed+=Time.deltaTime;hatch.transform.localScale=new Vector3(1,1-Mathf.Sin(elapsed/.75f*Mathf.PI)*.08f,1);yield return null;}hatch.transform.localScale=Vector3.one;opened=true;opening=false;Refresh();}
 public override void Interact(){var player=FindAnyObjectByType<PlayerClickMovement>();if(!opened||mission==null||mission.HasCollectedKeyMaterials||mission.BlocksGameplay||player==null||!IsInReach(player.transform))return;
 dialogue.ShowThought(new[]{"Os encaixes abriram este esconderijo! Há uma peça de metal e um molde de chave aqui.","Vou guardar os dois na mochila. Posso trabalhar essa peça na bancada da casa."},()=>{if(mission.TryCollectKeyMaterials())GameAudio.Play(GameAudio.Cue.Success);});}
 private void OnDestroy(){if(mission!=null)mission.OnMissionStateChanged-=Refresh;}
}
