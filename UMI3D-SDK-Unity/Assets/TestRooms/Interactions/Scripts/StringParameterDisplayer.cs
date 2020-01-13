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
using umi3d.edk;
using UnityEngine;
using UnityEngine.UI;

public class StringParameterDisplayer : MonoBehaviour
{
    public Text text;

    /// <summary>
    /// This have on purpose to be call by a OnChanged Event.
    /// Display the value of the parameter.
    /// </summary>
    /// <param name="user">The user who changed the value</param>
    /// <param name="value">The new value</param>
    public void Display(UMI3DUser user, string value)
    {
        text.text = value;
    }
}
