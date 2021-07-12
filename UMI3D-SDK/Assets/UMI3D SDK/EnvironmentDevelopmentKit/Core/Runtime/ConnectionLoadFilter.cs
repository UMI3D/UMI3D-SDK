﻿/*
Copyright 2019 - 2021 Inetum

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

namespace umi3d.edk
{
    /// <summary>
    /// Filter to restrain UMI3D scene object's visibility.
    /// </summary>
    public abstract class ConnectionLoadFilter : MonoBehaviour, UMI3DUserFilter
    {

        protected virtual void OnEnable()
        {
            foreach (var e in GetComponents<UMI3DMediaEntity>())
            {
                e.AddConnectionFilter(this);
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var e in GetComponents<UMI3DMediaEntity>())
            {
                e.RemoveConnectionFilter(this);
            }
        }

        /// <summary>
        /// Restrains the visibility of the UMI3D scene object for this gameObject.
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns>return true if this filter accept the object to be visible for the user.</returns>
        public abstract bool Accept(UMI3DUser user);
    }
}
