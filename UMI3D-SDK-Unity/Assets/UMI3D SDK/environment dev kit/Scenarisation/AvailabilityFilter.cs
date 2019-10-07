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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Filter to restrain UMI3D interactions' availability.
    /// </summary>
    public abstract class AvailabilityFilter : MonoBehaviour
    {
        /// <summary>
        /// Restrains the availability of UMI3D interactions component for this gameObject.
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>return true if this filter accept the interaction to be available for the user.</returns>
        public abstract bool Accept(UMI3DUser user);
    }
}
