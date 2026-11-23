using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private string sceneName = "NPCScene";
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private Transform approachPoint;
    [SerializeField] private string actionLabel = "Seguir caminho";
    [SerializeField] private AnimatedDoor animatedDoor;
    public Vector3 ApproachPosition => approachPoint != null ? approachPoint.position : transform.position;
    public string ActionLabel => actionLabel;

    public string SceneName => sceneName;
    public float InteractionDistance => interactionDistance;
    public bool ReadyToEnter => animatedDoor == null || animatedDoor.IsFullyOpen;
}
