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
    public class RadiusAvailabilityFilter : AvailabilityFilter
    {
        public float radius;

        // display the object for a user only if it is un radius range
        public override bool Accept(UMI3DUser user)
        {
            Vector3 userPos = user.avatar.viewpoint.gameObject.transform.position;
            Vector3 center = transform.position;

            return (Vector2.Distance(new Vector2(userPos.x, userPos.z), new Vector2(center.x, center.z)) <= radius);
        }
    }
}