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
    /// <summary>
    /// A class used to tag the bones to work with the pose setter tool
    /// </summary>
    public class PoseSetterBoneComponent : MonoBehaviour
    {
        /// <summary>
        /// If the bone is the root of the pose
        /// </summary>
        [EditorReadOnly] public bool isRoot;
        /// <summary>
        /// If the bone is selected
        /// </summary>
        [EditorReadOnly] public bool isSelected;
        /// <summary>
        /// If the bone can be saved
        /// </summary>
        [EditorReadOnly] public bool isSavable = true;

        /// <summary>
        /// The type of the bone 
        /// </summary>
        [SerializeField, ConstEnum(typeof(BoneType), typeof(uint))] uint boneType;

        /// <summary>
        /// Accessor for the bone type
        /// </summary>
        public uint BoneType { get => boneType; }
    }
}
#endif
