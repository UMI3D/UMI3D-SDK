using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.edk;
using umi3d.edk.volume;

public class WobbleStuff : MonoBehaviour
{
    public Box box;
    public float frequency = 1;
    public float phase = 0;
    public Vector3 amplitude;

    private Vector3 defaultValue;
    private UMI3DAsyncProperty<Vector3> property;

    private void Start()
    {
        property = box.center;
        defaultValue = property.GetValue();
    }

    private void Update()
    {
        property.SetValue(defaultValue + amplitude * Mathf.Sin(2 * 3.14f * frequency + phase));
    }

}
