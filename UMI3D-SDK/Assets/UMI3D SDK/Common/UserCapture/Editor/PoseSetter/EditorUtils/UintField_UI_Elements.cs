/*
Copy 2019 - 2023 Inetum

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

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace inetum.unityUtils
{
    public class UintField_UI_Elements : IntegerField
    {
        public class Uxmlfactory : UxmlFactory<UintField_UI_Elements, UintField_UI_Elements.UxmlTraits> { }

        public void Init()
        {
            this.RegisterValueChangedCallback(data =>
            {
                if (data != null)
                {
                    if (data.newValue < 0)
                    {
                        value = data.previousValue;
                    }
                }
            });
        }
    }
}

#endif