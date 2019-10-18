/*
Copyright 2019 Gfi Informatique

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
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
