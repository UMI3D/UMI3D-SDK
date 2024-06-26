﻿/*
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


#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture.pose.editor
{
    /// <summary>
    /// Wrapper for skeleton being edited in the <see cref="PoseEditorWindow"/>.
    /// </summary>
    public class HandClosureSkeleton
    {
        public GameObject root;

        /// <summary>
        /// Pose setter bone components collection defining each bone.
        /// </summary>
        public List<PoseSetterBoneComponent> boneComponents = new();

        /// <summary>
        /// Animator controlling the current had closure.
        /// </summary>
        public Animator handClosureAnimator;
    }
}
#endif