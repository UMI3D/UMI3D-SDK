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

using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    ///  A Skeleton node is a subskeleton with a Unity Animator
    /// that is packaged in a bundle. It is loaded the same way as a Mesh.
    /// </summary>
    public class UMI3DSkeletonNode : UMI3DModel
    {
        [Tooltip("Has this skeleton emotes attached on?")]
        public bool hasEmotes = false;

        [Tooltip("Emotes configuration scriptable object.")]
        public UMI3DEmotesConfig emoteConfig;

        protected override UMI3DNodeDto CreateDto()
        {
            return new UMI3DSkeletonNodeDto();
        }
    }
}