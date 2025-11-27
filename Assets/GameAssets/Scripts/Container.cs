using System;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    private string particleTag;
    private List<ParticleProperties> particles;
    private Color averageColor;

    private void Awake()
    {
        particleTag = GlobalValues.particleTag;
        particles = new List<ParticleProperties>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(particleTag)) return;

        if (other.TryGetComponent<ParticleProperties>(out var particle))
        {
            particles.Add(particle);
            UpdateAverageColor();
        }
        else
        {
            Debug.Log("ParticleProperties not found");
        }
    }

    private void UpdateAverageColor()
    {
        if (particles.Count == 0) return;

        float r = 0f, g = 0f, b = 0f, a = 0f;

        foreach (var p in particles)
        {
            var c = p.Color;
            r += c.r;
            g += c.g;
            b += c.b;
            a += c.a;
        }

        averageColor = new Color(r / particles.Count, g / particles.Count, b / particles.Count, a / particles.Count);

        foreach (var p in particles) p.Color = averageColor;
    }
}