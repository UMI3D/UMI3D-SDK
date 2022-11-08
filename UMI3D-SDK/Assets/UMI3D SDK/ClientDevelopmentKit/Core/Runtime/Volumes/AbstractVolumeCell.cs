/*
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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Base class for volume cells.
    /// </summary>
    public abstract class AbstractVolumeCell : umi3d.common.IPublisher
    {
        /// <summary>
        /// Get UMI3D id.
        /// </summary>
        /// <returns></returns>
        public abstract ulong Id();

        /// <summary>
        /// Can the volume be entered?
        /// </summary>
        public bool isTraversable = true;

        /// <summary>
        /// Check if a point is inside a cell.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsInside(Vector3 point, Space relativeTo);

        /// <summary>
        /// Return the base of the cell.
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="angleLimit"></param>
        public abstract void GetBase(Action<Mesh> onSuccess, float angleLimit);

        /// <summary>
        /// Get a representation of the cell as a mesh.
        /// </summary>
        /// <returns></returns>
        public abstract Mesh GetMesh();

        void OnUpdate()
        {
            foreach (var sub in subscribed)
                sub.Invoke();
        }

        List<Action> subscribed = new List<Action>();
        protected UnityEvent onUpdate = new UnityEvent();
        public bool Subscribe(Action callback)
        {
            if (subscribed.Contains(callback))
            {
                subscribed.Add(callback);
                return true;
            }
            return false;
        }

        public bool UnSubscribe(Action callback)
        {
            return subscribed.Remove(callback);
        }
    }
}