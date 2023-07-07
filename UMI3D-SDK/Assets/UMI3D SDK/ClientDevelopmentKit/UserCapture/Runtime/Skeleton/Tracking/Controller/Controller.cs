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

using inetum.unityUtils;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    public class Controller : MonoBehaviour, IController
    {
        [SerializeField, ConstEnum(typeof(BoneType), typeof(uint))]
        private uint _boneType;

        public uint boneType => _boneType;

        public Vector3 position => transform.position;

        public Quaternion rotation => transform.rotation;

        public bool isActif { get; set; }

        public void Destroy()
        {
            GameObject.Destroy(gameObject);
        }
    }
}