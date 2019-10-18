using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleIfHasChildren : MonoBehaviour
{
    public GameObject target;

    // Update is called once per frame
    void Update()
    {
        foreach(Transform t in target.transform)
            if (t.gameObject.activeSelf)
            {
                target.SetActive(true);
                return;
            }
        target.SetActive(false);
    }
}
