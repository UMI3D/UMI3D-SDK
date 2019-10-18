using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillate : MonoBehaviour
{
    public float period = 10;
    public Vector3 amplitude;

    private Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = this.transform.position;
    }

    float clock = 0;
    // Update is called once per frame
    void Update()
    {
        this.transform.position = originalPosition + amplitude * Mathf.Sin(2 * 3.14f * clock / period);
        clock += Time.deltaTime;
    }
}
