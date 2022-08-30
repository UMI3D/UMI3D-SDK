﻿/*
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

using System.Collections.Generic;

namespace umi3d.common
{
    /// <summary>
    /// <see cref="AbstractOperationDto"/> to control the list of the streamed bones.
    /// </summary>
    /// Use this operation to reduce the number of bones to track, if some are not necessary.
    /// This will lighthen the load on the networking system, but parts of the user's skeletton won't correspond to the user movements anymore.
    public class SetStreamedBonesDto : AbstractOperationDto
    {
        /// <summary>
        /// List of streamed bones using their boneType id in <see cref="userCapture.BoneType"/>
        /// </summary>
        public List<uint> streamedBones;
    }
}
