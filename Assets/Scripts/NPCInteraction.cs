using UnityEngine;
public class NPCInteraction : Interactable
{
    [SerializeField] private DialogueController dialogueController;
    public override void Interact() => dialogueController?.StartDialogue();
}
