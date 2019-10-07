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
using umi3d.edk;

namespace umi3d.example
{
    [RequireComponent(typeof(CVEPrimitive))]
    public class HoverExampleColorSwap : MonoBehaviour
    {
        public Hover Hover;
        /// a material for a CVEPrimitive or CVEMesh
        public CVEMaterial HoveredColor;
        public CVEMaterial NonHoveredColor;

        CVEPrimitive target;

        private void Start()
        {
            target = GetComponent<CVEPrimitive>();
            target.Material = NonHoveredColor;
            //the onHoverEnter/Exit event is sent when a user start/end hovering the Hover target.
            Hover.onHoverEnter.AddListener(onHoverEnter);
            Hover.onHoverExit.AddListener(onHoverExit);
        }

        void onHoverEnter(UMI3DUser user)
        {
            target.Material = HoveredColor;
        }

        void onHoverExit(UMI3DUser user)
        {
            target.Material = NonHoveredColor;
        }
    }
}