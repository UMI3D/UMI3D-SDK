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

namespace umi3d.cdk.userCapture
{
    public class UMI3DClientUserTrackingBone : MonoBehaviour
    {
        public static Dictionary<uint, UMI3DClientUserTrackingBone> instances = new Dictionary<uint, UMI3DClientUserTrackingBone>();

        [ConstEnum(typeof(BoneType), typeof(uint))]
        public uint boneType;

        public bool isTracked;

        /// <summary>
        /// Convert this bone to a dto.
        /// </summary>
        /// <param name="Anchor">Frame of reference</param>
        /// <returns></returns>
        public BoneDto ToDto()
        {
            return boneType == BoneType.None ? null : new BoneDto()
            {
                boneType = boneType,
                rotation = this.transform.localRotation,
            };
        }

        protected virtual void OnDestroy()
        {
            instances.Remove(boneType);
        }

        private void OnDisable()
        {
            instances.Remove(boneType);
        }

        private void OnEnable()
        {
            if (boneType == BoneType.None) return;
            if (instances.ContainsKey(boneType))
            {
                if (gameObject.GetComponents<UMI3DClientUserTrackingBone>().Count() > 1)
                    throw new System.Exception("There can be only one bone per gameobject !");
                else
                    throw new System.Exception("Internal error");
            }
            instances.Add(boneType, this);
        }
    }
}
