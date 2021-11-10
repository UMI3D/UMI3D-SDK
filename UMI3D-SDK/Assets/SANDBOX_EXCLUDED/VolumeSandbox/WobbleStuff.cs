using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.edk;
using umi3d.edk.volume;

public class WobbleStuff : MonoBehaviour
{
    public Cylinder cylinder;
    public float frequency = 1;
    public float phase = 0;
    public float amplitude;
    public float frameRate = 30;

    private float defaultValue;
    private UMI3DAsyncProperty<float> property;

    private void Start()
    {
        property = cylinder.height;
        defaultValue = property.GetValue();
        StartCoroutine(Wobble());
    }

    private IEnumerator Wobble()
    {
        while (frameRate > 0)
        {
            property.SetValue(defaultValue + amplitude * Mathf.Sin(2 * 3.14f * frequency * Time.realtimeSinceStartup + phase));
            yield return new WaitForSeconds(1f / frameRate);
        }
    }

}
