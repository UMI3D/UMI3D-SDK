using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractVolumeCell 
{
    public abstract string Id();

    public abstract bool IsInside(Vector3 point);
}
