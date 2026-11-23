using UnityEngine;

public class LighthouseInscription : Interactable
{
    [SerializeField] private LighthouseGate gate;
    [SerializeField] private PlayerClickMovement movement;
    public override void Interact()
    {
        if(movement==null||!IsInReach(movement.transform))return;
        gate.Interact();
        if(gate.IsOpen)gate.ReadClue();
    }
}
