using System;
using UnityEngine;

public class ParticleReleaser : MonoBehaviour
{
    [SerializeField] private String layer;
    [SerializeField] private Transform particleHolder;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log(other.gameObject.layer.ToString());
        if (other.gameObject.layer == LayerMask.NameToLayer(layer));
        {
            other.transform.parent = particleHolder;
        }
    }
}
