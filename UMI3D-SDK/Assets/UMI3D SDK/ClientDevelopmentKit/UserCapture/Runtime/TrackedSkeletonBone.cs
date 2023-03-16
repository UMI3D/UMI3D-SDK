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

using inetum.unityUtils;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Representation for each tracked bone.
    /// </summary>
    public class TrackedSkeletonBone : MonoBehaviour
    {
        /// <summary>
        /// Bone type in UMI3D standards. See <see cref="BoneDto"/>.
        /// </summary>
        [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Bone type in UMI3D standards.")]
        public uint boneType;

        /// <summary>
        /// Convert this bone to a dto.
        /// </summary>
        /// <param name="Anchor">Frame of reference</param>
        /// <returns></returns>
        public BonePoseDto ToBonePoseDto()
        {
            return boneType == BoneType.None ? null : new BonePoseDto(boneType, this.transform.position, new Vector4().FromQuaternion(this.transform.localRotation));
        }
    }

    public class TrackedSkeletonBoneController : TrackedSkeletonBone, IController
    {
        public Vector3 position
        {
            get
            {
                return this.transform.position;
            }

            set
            {
                this.transform.position = value;
            }
        }

        public Quaternion rotation
        {
            get
            {
                return this.transform.localRotation;
            }
            set
            {
                this.transform.rotation = value;
            }
        }

        public new uint boneType { get; set; }
        public bool isActif { get; set; }

        public void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
