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
using UnityEngine;

public class StepMotion : MonoBehaviour
{
    public float amplitude = 0.1f;

    /// <summary>
    /// This have on purpose to be call by a OnTrigger Event.
    /// Move the transform up.
    /// </summary>
    public void MoveUp()
    {
        this.transform.position += amplitude * Vector3.up;
    }

    /// <summary>
    /// This have on purpose to be call by a OnRelease Event.
    /// Move the transform down.
    /// </summary>
    public void MoveDown()
    {
        this.transform.position -= amplitude * Vector3.up;
    }

}
