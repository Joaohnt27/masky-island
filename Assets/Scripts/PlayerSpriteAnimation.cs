using UnityEngine;

// Usa deslocamento real: funciona com WASD e aproximação por clique, sem alterar a movimentação.
[RequireComponent(typeof(PlayerClickMovement), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerSpriteAnimation : MonoBehaviour
{
    public enum Direction { Front, Back, Left, Right }

    [SerializeField, Min(0f)] private float movementThreshold = .015f;
    [SerializeField, Min(.1f)] private float teleportDistance = 1f;
    [SerializeField] private MiningSpot miningSpot;
    private PlayerClickMovement movement;
    private Animator animator;
    private Vector3 previousPosition;
    private bool initialized;
    private int currentState;
    public Direction Facing { get; private set; } = Direction.Front;
    public bool IsWalking { get; private set; }
    public bool IsMiningAnimation => isActiveAndEnabled && miningSpot != null && miningSpot.IsMining && !miningSpot.IsPreparing &&
        animator != null && animator.runtimeAnimatorController != null &&
        animator.HasState(0, miningSpot.FacesLeft ? MineLeft : MineRight);
    private static readonly int MineLeft = Animator.StringToHash("Base Layer.MineLeft");
    private static readonly int MineRight = Animator.StringToHash("Base Layer.MineRight");

    private static readonly int[] IdleStates = {
        Animator.StringToHash("Base Layer.IdleFront"), Animator.StringToHash("Base Layer.IdleBack"),
        Animator.StringToHash("Base Layer.IdleLeft"), Animator.StringToHash("Base Layer.IdleRight")
    };
    private static readonly int[] WalkStates = {
        Animator.StringToHash("Base Layer.WalkFront"), Animator.StringToHash("Base Layer.WalkBack"),
        Animator.StringToHash("Base Layer.WalkLeft"), Animator.StringToHash("Base Layer.WalkRight")
    };

    private void Awake()
    {
        movement = GetComponent<PlayerClickMovement>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        initialized = false;
        currentState = 0;
        IsWalking = false;
    }

    private void LateUpdate()
    {
        Vector2 delta = transform.position - previousPosition;
        previousPosition = transform.position;
        // A primeira amostra ignora o posicionamento de chegada feito por StoryLocation.Start.
        IsWalking = initialized && Time.deltaTime > 0f && movement.IsMovementEnabled &&
            delta.sqrMagnitude > Mathf.Pow(movementThreshold * Time.deltaTime, 2) &&
            delta.sqrMagnitude < teleportDistance * teleportDistance;
        initialized = true;

        if (IsWalking)
        {
            // Histerese evita alternância de perfil em movimentos diagonais.
            bool horizontal = Facing == Direction.Left || Facing == Direction.Right;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) * 1.15f) horizontal = true;
            else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) * 1.15f) horizontal = false;
            Facing = horizontal ? (delta.x < 0 ? Direction.Left : Direction.Right)
                : (delta.y > 0 ? Direction.Back : Direction.Front);
        }

        if (animator.runtimeAnimatorController == null || Time.deltaTime <= 0f ||
            (GameManager.Instance != null && GameManager.Instance.IsPaused)) return;
        if (IsMiningAnimation)
        {
            IsWalking = false;
            Facing = miningSpot.FacesLeft ? Direction.Left : Direction.Right;
            currentState = miningSpot.FacesLeft ? MineLeft : MineRight;
            // O progresso da tentativa é o relógio do corpo, ferramenta e impacto sonoro.
            animator.speed = 0f;
            animator.Play(currentState, 0, Mathf.Repeat(miningSpot.StrokeTime, 1f));
            animator.Update(0f);
            return;
        }
        animator.speed = 1f;
        int state = (IsWalking ? WalkStates : IdleStates)[(int)Facing];
        if (state == currentState) return;
        currentState = state;
        animator.Play(state, 0, 0f);
        animator.Update(0f);
    }

    private void OnDisable() { if (animator != null) animator.speed = 1f; }
}
