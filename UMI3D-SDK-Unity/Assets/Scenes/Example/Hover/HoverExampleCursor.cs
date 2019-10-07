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
    public class HoverExampleCursor : MonoBehaviour
    {
        public Hover Hover;
        public Transform cursor;

        private void Start()
        {
            //the onhover event is sent when a user is hovering the Hover target, the hovering position has changed and the Hover.TrackHoveredPosition is true.
            Hover.onHover.AddListener(onHover);
            cursor.position = transform.position;
        }


        /// <summary>
        /// we simply move the cursor to the last hovering position.
        /// </summary>
        /// <param name="user">the user that is hovering the target</param>
        /// <param name="globalPosition">the world position of the hovering</param>
        /// <param name="normal">the normal of the surface that is hovered</param>
        void onHover(UMI3DUser user, Vector3 globalPosition, Vector3 normal)
        {
            cursor.position = globalPosition;
        }
    }
}