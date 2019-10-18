using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtElbow : MonoBehaviour
{
    public Transform Elbow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.LookAt(Elbow);
        this.transform.Rotate(0, 0, 90);
    }
}
