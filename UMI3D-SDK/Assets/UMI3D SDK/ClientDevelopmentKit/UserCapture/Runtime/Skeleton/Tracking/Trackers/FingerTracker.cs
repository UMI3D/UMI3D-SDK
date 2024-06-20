/*
Copyright 2019 - 2024 Inetum
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

using inetum.unityUtils;

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    public class FingerTracker : MonoBehaviour, ITracker
    {
        [EditorReadOnly, SerializeField, ConstEnum(typeof(common.userCapture.BoneType), typeof(uint))]
        protected uint boneType;
        public uint BoneType => boneType;

        public bool isActive { get; set; } = true;

        public bool isOverrider { get; set; } = true;

        public VirtualController controller;

        public IController Controller
        {
            get
            {
                UpdateController();
                return controller;
            }
        }

        [SerializeField] private bool isRight;
        [SerializeField] private bool isThumb;

        [SerializeField] private Vector3 offset;

        protected virtual void UpdateController()
        {
            var directionMul = isRight ? -1 : 1;
            controller.position = transform.position;
            controller.rotation = (isThumb
                ? new Quaternion(-transform.localRotation.z, directionMul * -transform.localRotation.x,
                                 transform.localRotation.y, transform.localRotation.w)
                : new Quaternion(transform.localRotation.z, transform.localRotation.y,
                                 directionMul * transform.localRotation.x, transform.localRotation.w)) * Quaternion.Euler(offset);
            controller.isActive = isActive;
            controller.isOverrider = isOverrider;
        }
    }
}