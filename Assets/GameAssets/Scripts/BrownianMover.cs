using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BrownianMover : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float period = 0.1f;
    private Rigidbody2D rb;
    private static WaitForSeconds waitForPeriod = null;
    private void Awake()
    {
        if (speed == 0f) return;
        rb = GetComponent<Rigidbody2D>();
        if (waitForPeriod == null) waitForPeriod = new WaitForSeconds(period);
        StartCoroutine(AddRandomForce());
    }
    private IEnumerator AddRandomForce()
    {
        while (true)
        {
            yield return waitForPeriod;
            rb.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * speed);
        }

    }

}
