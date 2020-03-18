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

namespace umi3d.common
{
    [System.Serializable]
    public class UIRectDto : EmptyObject3DDto
    {
        public SerializableVector2 anchoredPosition;
        public SerializableVector3 anchoredPosition3D;
        public SerializableVector2 anchorMax;
        public SerializableVector2 anchorMin;
        public SerializableVector2 offsetMax;
        public SerializableVector2 offsetMin;
        public SerializableVector2 pivot;
        public SerializableVector2 sizeDelta;
        public bool rectMask;

        public UIRectDto() : base() { }
    }
}