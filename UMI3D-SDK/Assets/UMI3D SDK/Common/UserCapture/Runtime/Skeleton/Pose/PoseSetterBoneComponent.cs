/*
Copy 2019 - 2023 Inetum

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
#if UNITY_EDITOR

using inetum.unityUtils;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture.pose.editor
{
    public class PoseSetterBoneComponent : MonoBehaviour
    {
        [EditorReadOnly] public bool isRoot;
        [EditorReadOnly] public bool isSelected;
        [EditorReadOnly] public bool isSavable = true;

        [SerializeField, ConstEnum(typeof(BoneType), typeof(uint))] uint boneType;

        public uint BoneType { get => boneType; }
    }
}
#endif
