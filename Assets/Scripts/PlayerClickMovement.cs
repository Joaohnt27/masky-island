using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerClickMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool keyboardMovement;
    [SerializeField] private Vector2 walkMin = new Vector2(-8, -4);
    [SerializeField] private Vector2 walkMax = new Vector2(8, -.3f);
    [SerializeField] private TMPro.TMP_Text hoverText;
    private Interactable targetInteraction;

    [Header("Colisão")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Vector2 playerCollisionSize = new Vector2(0.4f, 0.8f);

    private Vector3 targetPosition;
    private bool isMoving;
    private readonly HashSet<Object> movementLocks = new HashSet<Object>();
    public bool IsMovementEnabled => movementLocks.Count == 0 && !SceneFade.IsRunning &&
        (GameManager.Instance == null || !GameManager.Instance.BlocksGameplay);

    public void AcquireMovementLock(Object owner)
    {
        if (owner == null)
            return;
        movementLocks.Add(owner);
        isMoving = false;
        targetDoor = null;
        targetMiningSpot = null;
        targetInteraction = null;
        targetPosition = transform.position;
    }

    public void ReleaseMovementLock(Object owner) => movementLocks.Remove(owner);

    // Interação que o jogador quer realizar
    private DoorInteraction targetDoor;
    public DoorInteraction TargetDoor => targetDoor;
    public DoorInteraction HoveredDoor { get; private set; }
    private MiningSpot targetMiningSpot;

    public void RequestMining(MiningSpot spot, Collider clicked = null)
    {
        if (!IsMovementEnabled || spot == null || GameManager.Instance == null ||
            (!spot.CanDiscover && GameManager.Instance.CrystalCount < 3) || !spot.SelectTarget(clicked)) return;
        targetDoor = null;
        targetInteraction = null;
        targetMiningSpot = spot;
        targetPosition = spot.ApproachPosition;
        isMoving = true;
    }

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        UpdateHover();
        if (!IsMovementEnabled)
            return;
        if (keyboardMovement && MoveWithKeyboard()) return;
        HandleClick();
        MovePlayer();
        CheckDoorInteraction();
        CheckMiningInteraction();
        if (targetInteraction != null && !isMoving && targetInteraction.IsInReach(transform))
        {
            var interaction = targetInteraction;
            targetInteraction = null;
            interaction.Interact();
        }
    }

    private bool MoveWithKeyboard()
    {
        var keys = Keyboard.current;
        if (keys == null) return false;
        Vector2 input = new Vector2((keys.dKey.isPressed ? 1 : 0) - (keys.aKey.isPressed ? 1 : 0),
            (keys.wKey.isPressed ? 1 : 0) - (keys.sKey.isPressed ? 1 : 0));
        if (input == Vector2.zero) return false;
        targetDoor = null; targetMiningSpot = null; targetInteraction = null; isMoving = false;
        Vector3 next = transform.position + (Vector3)(input.normalized * moveSpeed * Time.deltaTime);
        next.x = Mathf.Clamp(next.x, walkMin.x, walkMax.x);
        next.y = Mathf.Clamp(next.y, walkMin.y, walkMax.y);
        if (!Physics.CheckBox(next, new Vector3(playerCollisionSize.x, playerCollisionSize.y, .2f), Quaternion.identity, obstacleLayer))
            transform.position = next;
        return true;
    }

    private void UpdateHover()
    {
        HoveredDoor = null;
        if (hoverText != null) hoverText.text = "";
        if (!IsMovementEnabled || Mouse.current == null || Camera.main == null ||
            (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())) return;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out var hit)) return;
        var interaction = hit.collider.GetComponentInParent<Interactable>();
        var door = hit.collider.GetComponentInParent<DoorInteraction>();
        HoveredDoor = door;
        var rock = hit.collider.GetComponentInParent<MiningSpot>();
        string label = interaction != null ? interaction.ActionLabel : door != null ? door.ActionLabel :
            rock != null ? rock.IsSocket(hit.collider) ? "Encaixar cristais"
                : (rock.CanDiscover ? "Minerar veio" : "Examinar os cristais") : "";
        if (hoverText != null && label.Length > 0) hoverText.text = "CLIQUE  •  " + label;
    }

    private void HandleClick()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Camera.main == null ||
            (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
            return;

        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        targetInteraction = null;
        Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
        if (interactable != null)
        {
            targetDoor = null; targetMiningSpot = null;
            targetInteraction = interactable;
            targetPosition = interactable.ApproachPosition;
            isMoving = true;
            return;
        }

        MiningSpot rock = hit.collider.GetComponentInParent<MiningSpot>();
        if (rock != null)
        {
            if (rock.IsSocket(hit.collider) && (GameManager.Instance == null || GameManager.Instance.CrystalCount < 3)) return;
            RequestMining(rock, hit.collider);
            return;
        }

        // =========================================
        // CLICOU NA PORTA
        // =========================================

        DoorInteraction door = hit.collider.GetComponentInParent<DoorInteraction>();

        if (door != null)
        {
            if (string.IsNullOrWhiteSpace(door.SceneName) ||
                !Application.CanStreamedLevelBeLoaded(door.SceneName))
                return;
            targetDoor = door;
            targetMiningSpot = null;

            Vector3 direction = transform.position - door.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                direction.Normalize();

                targetPosition =
                    door.transform.position +
                    direction * door.InteractionDistance;
            }

            isMoving = true;
            if (keyboardMovement) targetPosition = door.ApproachPosition;

            return;
        }

        // =========================================
        // CLICOU NO CHÃO
        // =========================================

        if (hit.collider.gameObject.name == "WalkableArea")
        {
            if (keyboardMovement) return;
            targetDoor = null;
            targetMiningSpot = null;

            targetPosition = hit.point;
            isMoving = true;
        }
    }

    private void MovePlayer()
    {
        if (!isMoving)
            return;

        Vector3 direction = targetPosition - transform.position;

        if (direction.magnitude < 0.05f)
        {
            transform.position = targetPosition;
            isMoving = false;
            return;
        }

        direction.Normalize();

        Vector3 nextPosition = Vector3.MoveTowards(transform.position,
            targetPosition, moveSpeed * Time.deltaTime);

        bool blocked = Physics.CheckBox(
            nextPosition,
            new Vector3(
                playerCollisionSize.x,
                playerCollisionSize.y,
                0.2f
            ),
            Quaternion.identity,
            obstacleLayer
        );

        if (blocked)
        {
            isMoving = false;
            targetDoor = null;
            targetMiningSpot = null;
            return;
        }

        transform.position = nextPosition;
    }

    private void CheckDoorInteraction()
    {
        if (targetDoor == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            targetDoor.ApproachPosition
        );

        if (distance <= targetDoor.InteractionDistance)
        {
            isMoving = false;

            if (targetDoor.ReadyToEnter && SceneFade.TryLoad(targetDoor.SceneName))
                targetDoor = null;
        }
    }

    private void CheckMiningInteraction()
    {
        if (targetMiningSpot == null) return;
        if (!targetMiningSpot.CanDiscover && (GameManager.Instance == null || GameManager.Instance.CrystalCount < 3))
        { targetMiningSpot = null; return; }
        if (Vector3.Distance(transform.position, targetPosition) > 0.08f) return;
        MiningSpot spot = targetMiningSpot;
        targetMiningSpot = null;
        isMoving = false;
        if (GameManager.Instance != null && GameManager.Instance.CrystalCount >= 3)
            FindAnyObjectByType<MiningUI>()?.OpenSockets();
        else spot.StartDiscovery();
    }
}
