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
using System.Collections.Generic;
using UnityEngine;

public class TooManyEvents : MonoBehaviour
{
    [SerializeField]
    List<StepMotionHorizontal> cubes = new List<StepMotionHorizontal>();
    public umi3d.edk.AbstractCVETool tool;

    private void Start()
    {
        foreach(var cube in cubes)
        {
            umi3d.edk.UMI3DEvent _event = gameObject.AddComponent<umi3d.edk.UMI3DEvent>();
            _event.Hold = true;
            _event.onHold.AddListener((a,b) => { cube.Move(); cube.GetComponent<BooleanVisibleFilter>().display.SetValue(a,false); });
            _event.onRelease.AddListener((a, b) => { cube.ResetPosition(); cube.GetComponent<BooleanVisibleFilter>().display.Sync(a,true); });
            _event.tool = tool;
            _event.InteractionName = cube.name;
        }
    }

    /// <summary>
    /// Reset the position of all cubes
    /// </summary>
    public void ResetCubes()
    {
        foreach (var cube in cubes)
        {
             cube.ResetPosition();
        }
    }
}
