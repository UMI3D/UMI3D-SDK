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
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    public class Tracker : MonoBehaviour
    {
        [EditorReadOnly,SerializeField, ConstEnum(typeof(common.userCapture.BoneType), typeof(uint))]
        private uint boneType;

        public uint Bonetype => boneType;

        public bool isActif = true;

        public bool isOverrider = true;

        private void Awake()
        {
            distantController = new DistantController()
            {
                boneType = boneType,
                position = transform.position,
                rotation = transform.rotation,
                isActif = isActif,
                isOverrider = isOverrider
            };
        }

        public void Update()
        {
            distantController.position = transform.position;
            distantController.rotation = transform.rotation;
            distantController.isActif = isActif;
            distantController.isOverrider = isOverrider;
        }

        public DistantController distantController { get; protected set; }

    }
}