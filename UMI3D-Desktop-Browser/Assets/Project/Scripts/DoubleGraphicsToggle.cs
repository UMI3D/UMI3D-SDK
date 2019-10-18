using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoubleGraphicsToggle : Toggle
{
    public GameObject OnGraphic;
    public GameObject OffGraphic;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (OnGraphic && isOn != OnGraphic.activeSelf)
            OnGraphic.SetActive(isOn);
        if(OffGraphic && isOn == OffGraphic.activeSelf)
            OffGraphic.SetActive(!isOn);
    }
#endif
    protected override void Start()
    {
        base.Start();
        onValueChanged.AddListener((b) =>
        {
            if (OnGraphic && b != OnGraphic.activeSelf)
                OnGraphic.SetActive(b);
            if (OffGraphic && b == OffGraphic.activeSelf)
                OffGraphic.SetActive(!b);
        });
        if (OnGraphic && isOn != OnGraphic.activeSelf)
            OnGraphic.SetActive(isOn);
        if (OffGraphic && isOn == OffGraphic.activeSelf)
            OffGraphic.SetActive(!isOn);
    }
}
