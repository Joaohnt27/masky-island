using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [Header("Interação")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private Transform approachPoint;
    [SerializeField] private string actionLabel = "Examinar";
    public string ActionLabel => actionLabel;
    public Vector3 ApproachPosition => approachPoint != null ? approachPoint.position : transform.position;
    public bool IsInReach(Transform actor) => actor != null && Vector3.Distance(actor.position, ApproachPosition) <= interactionDistance;

    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;

    private bool canInteract;
    private PlayerClickMovement playerMovement;

    protected virtual void Start()
    {
        if (player != null)
            playerMovement = player.GetComponent<PlayerClickMovement>();
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    protected virtual void Update()
    {
        CheckDistance();
        CheckInteraction();
    }

    private void CheckDistance()
{
    if (player == null)
        return;

    float distance = Vector3.Distance(
        ApproachPosition,
        player.position
    );

    canInteract = distance <= interactionDistance &&
        (playerMovement == null || playerMovement.IsMovementEnabled);

    if (interactionPrompt != null)
        interactionPrompt.SetActive(canInteract);
}

    private void CheckInteraction()
    {
        if (!canInteract)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public virtual void Interact()
    {
        Debug.Log("Interagiu com " + gameObject.name);
    }
}
