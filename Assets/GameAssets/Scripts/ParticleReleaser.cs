using System;
using UnityEngine;

public class ParticleReleaser : MonoBehaviour
{
    [SerializeField] private string particleTag;
    [SerializeField] private Transform particleHolder;

    private void Awake()
    {
        particleTag = GlobalValues.particleTag;
    }

    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log(other.gameObject.layer.ToString());
        if (other.CompareTag(particleTag)) other.transform.parent = particleHolder;
    }
}