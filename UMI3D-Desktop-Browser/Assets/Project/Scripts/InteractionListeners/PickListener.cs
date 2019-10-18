using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickListener : MonoBehaviour {
    public System.Action<Vector3> onPick;
    public void OnPick(Vector3 hit)
    {
        if (onPick != null)
            onPick(hit);
    }

    private void Update()
    {
        if (transform is RectTransform)
        {
            var c = GetComponent<BoxCollider>();
            if (c == null)
                c = gameObject.AddComponent<BoxCollider>();
            var s = (transform as RectTransform).rect.size;
            var p = (transform as RectTransform).pivot;
            c.size = new Vector3(s.x, s.y, 0f);
            c.center = new Vector3(s.x * (.5f - p.x), s.y * (.5f - p.y));
        }
    }

    private void OnDestroy()
    {
        if (transform is RectTransform)
        {
            var c = GetComponent<BoxCollider>();
            if (c != null)
                Destroy(c);
        }
    }
}
