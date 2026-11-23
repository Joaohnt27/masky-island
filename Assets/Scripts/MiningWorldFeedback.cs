using UnityEngine;

// Mantém a ferramenta antiga como alternativa quando não há animação de mineração.
public class MiningWorldFeedback : MonoBehaviour
{
    [SerializeField] private MiningSpot spot;
    [SerializeField] private Transform player;
    [SerializeField] private Transform pickaxe;
    [SerializeField] private Transform rockVisual;
    private Vector3 rockOrigin;
    private PlayerSpriteAnimation playerAnimation;

    private void Awake()
    {
        if (rockVisual != null) rockOrigin = rockVisual.localPosition;
        if (player != null) playerAnimation = player.GetComponent<PlayerSpriteAnimation>();
        if (pickaxe != null) pickaxe.gameObject.SetActive(false);
    }
    private void LateUpdate()
    {
        bool active = spot != null && spot.IsMining;
        if (pickaxe != null)
        {
            bool separateTool = active && (playerAnimation == null || !playerAnimation.IsMiningAnimation);
            pickaxe.gameObject.SetActive(separateTool);
            if (separateTool && player != null)
            {
                pickaxe.position = player.position + new Vector3(0.35f, 0.12f, -0.5f);
                pickaxe.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(-55, 35, (Mathf.Sin(spot.Progress * Mathf.PI * 12) + 1) * 0.5f));
            }
        }
        if (rockVisual != null)
        {
            float phase = active ? Mathf.Repeat(spot.StrokeTime, 1f) : 0f;
            float impact = phase >= .5f && phase < .75f ? Mathf.Sin((phase - .5f) * Mathf.PI * 16) * .025f : 0f;
            rockVisual.localPosition = rockOrigin + Vector3.right * impact;
        }
    }
    private void OnDisable()
    {
        if (pickaxe != null) pickaxe.gameObject.SetActive(false);
        if (rockVisual != null) rockVisual.localPosition = rockOrigin;
    }
}
