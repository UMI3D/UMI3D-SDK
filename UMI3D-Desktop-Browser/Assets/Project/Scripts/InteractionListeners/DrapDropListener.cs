using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrapDropListener : MonoBehaviour {
    public System.Action onDragStart;

    public void OnDragStart()
    {
        if (onDragStart != null)
            onDragStart();
    }
}
