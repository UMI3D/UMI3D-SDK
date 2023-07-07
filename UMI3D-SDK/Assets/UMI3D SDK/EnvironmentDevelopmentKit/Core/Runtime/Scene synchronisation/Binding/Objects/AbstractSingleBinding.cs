/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.edk.binding
{
    /// <summary>
    /// Operation binding a node and only one object.
    /// </summary>
    public abstract class AbstractSingleBinding : AbstractBinding
    {
        /// <summary>
        /// Should the parent object translation movements be applied to the bound point?
        /// </summary>
        public bool syncPosition = false;

        /// <summary>
        /// Should the parent object rotation movements be applied to the bound point? <br/>
        /// When combined with position synchronisation, resulting movement is relative to an anchor.
        /// </summary>
        public bool syncRotation = false;

        /// <summary>
        /// Should the parent object scaling be applied to the bound point?
        /// </summary>
        public bool syncScale = false;

        /// <summary>
        /// Offset between the parent position and the bound object target position.
        /// </summary>
        public Vector3 offsetPosition = Vector3.zero;

        /// <summary>
        /// Offset between the parent rotation and the bound object target rotation.
        /// </summary>
        public Quaternion offsetRotation = Quaternion.identity;

        /// <summary>
        /// Offset between the parent scale and the bound object target scale.
        /// </summary>
        public Vector3 offsetScale = Vector3.one;

        /// <summary>
        /// Anchor for rotation when position and rotation synchronisation are active. <br/>
        /// Value is given in the parent object referential.
        /// </summary>
        public Vector3 anchor = Vector3.zero;

        protected AbstractSingleBinding(ulong boundNodeId) : base(boundNodeId)
        {
        }
    }
}