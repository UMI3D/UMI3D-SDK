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
        protected uint boneType;

        public uint BoneType => boneType;

        public bool isActif = true;

        public bool isOverrider = true;

        protected void Awake()
        {
            CreateDistantController();
        }

        protected virtual void Update()
        {
            UpdateDistantController();
        }

        protected virtual void CreateDistantController()
        {
            distantController = new DistantController()
            {
                boneType = boneType,
                position = transform.position,
                rotation = transform.rotation,
                isActive = isActif,
                isOverrider = isOverrider
            };
        }

        protected virtual void UpdateDistantController()
        {
            distantController.position = transform.position;
            distantController.rotation = transform.rotation;
            distantController.isActive = isActif;
            distantController.isOverrider = isOverrider;
        }

        public DistantController distantController { get; protected set; }

    }
}