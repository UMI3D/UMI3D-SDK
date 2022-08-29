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

using System;
using System.Collections.Generic;

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Abstract class to represent the root node of one user's representation.
    /// </summary>
    /// An avatar is the virtual representation of the user in the virtual environment. 
    /// UMI3D avatars a constructed upon a skeleton composed of bones (see <seealso cref="BoneDto"/>).
    /// The visual representation could be achieved either by mapping an avatar to the skeletton globally, or just
    /// by binding model parts to each desired bone of the skeleton. (see <see cref="BoneBindingDto"/>).
    [Serializable]
    public class UMI3DAvatarNodeDto : UMI3DNodeDto
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public ulong userId;

        /// <summary>
        /// The user's size
        /// </summary>
        public SerializableVector3 userSize;

        /// <summary>
        /// A bool to enable or disable bindings
        /// </summary>
        public bool activeBindings;

        /// <summary>
        /// A list of bindings between the user's bones and their representations.
        /// </summary>
        public List<BoneBindingDto> bindings;

        /// <summary>
        /// List of availables hand poses for the avatar.
        /// </summary>
        /// Note that this field is empty for other avatars than the target user's one.
        public List<UMI3DHandPoseDto> handPoses;

        /// <summary>
        /// List of availables body poses for the avatar.
        /// </summary>
        /// Note that this field is empty for other avatars than the target user's one.
        public List<UMI3DBodyPoseDto> bodyPoses;

        /// <summary>
        /// Emotes configuration to manage available emotes
        /// </summary>
        public UMI3DEmotesConfigDto emotesConfigDto;
    }
}
