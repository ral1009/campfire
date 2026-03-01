using UnityEngine;
using System;

public class Bobbing : MonoBehaviour
{
    public float startY;
    public float amplitude = 50;
    public float bobSpeed = 3f;
    public float random;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startY = transform.position.y;
        random = UnityEngine.Random.value;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(transform.position.x, startY + (float)Math.Sin(Time.time * bobSpeed + random) * amplitude);
    }
}
