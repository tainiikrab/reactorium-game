using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleProperties : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private static WaitForSeconds[] randomWaits = { new(0.6f), new(0.5f), new(0.4f), new(0.3f) };
    private static int randomIndex = 0;

    private Color _color;

    public Color Color
    {
        get => _color;
        set
        {
            StartCoroutine(UpdateColor(value));
            _color = value;
        }
    }

    private IEnumerator UpdateColor(Color value)
    {
        yield return randomWaits[randomIndex++ % randomWaits.Length];
        spriteRenderer.color = value;
    }

    private void Awake()
    {
        _color = spriteRenderer.color;
    }
#if UNITY_EDITOR
    [SerializeField] private Color initialColor;
    private void OnValidate()
    {
        if (spriteRenderer != null) spriteRenderer.color = initialColor;
    }
#endif
}