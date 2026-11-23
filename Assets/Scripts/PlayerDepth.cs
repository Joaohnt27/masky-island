using UnityEngine;

public class PlayerDepth : MonoBehaviour
{
    [Header("Referência do chão")]
    [SerializeField] private Transform walkableArea;

    [Header("Tamanho do personagem")]
    [SerializeField] private float maxScale = 0.80f;
    [SerializeField] private float minScale = 0.60f;

    [Header("Profundidade")]
    [SerializeField] private float frontDepth = -3.7f;
    [SerializeField] private float backDepth = 3.7f;

    private void Update()
    {
        Vector3 localPosition =
            walkableArea.InverseTransformPoint(transform.position);

        float depth = Mathf.InverseLerp(
            frontDepth,
            backDepth,
            localPosition.z
        );
        
        // cima/fundo = menor
        // baixo/frente = maior
        depth = 1f - depth;

        float scale = Mathf.Lerp(
            minScale,
            maxScale,
            depth
        );

        transform.localScale = Vector3.one * scale;
    }
}