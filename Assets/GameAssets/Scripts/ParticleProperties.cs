using UnityEngine;

public class ParticleProperties : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Color Color
    {
        get => spriteRenderer.color;
        set => spriteRenderer.color = value;
    }


#if UNITY_EDITOR
    [SerializeField] private Color initialColor;
    private void OnValidate()
    {
        if (spriteRenderer != null) spriteRenderer.color = initialColor;
    }
#endif
}