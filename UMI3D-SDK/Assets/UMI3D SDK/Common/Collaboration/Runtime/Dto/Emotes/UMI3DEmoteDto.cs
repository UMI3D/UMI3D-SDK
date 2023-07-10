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

namespace umi3d.common.collaboration.dto.emotes
{
    /// <summary>
    /// Emote data, including a reference to the icon resource, packaged in a DTO.
    /// </summary>
    /// An emote is a short animation that is played to convey a specific communication, often an emotion.
    /// Emotes are used on non-immersive devices to allow the user to communicate non-verbally.
    [System.Serializable]
    public class UMI3DEmoteDto : AbstractEntityDto, IEntity
    {
        /// <summary>
        /// Emote label that is displayed to users.
        /// </summary>
        public string label { get; set; } = "";

        /// <summary>
        /// If the user can see and play the emote
        /// </summary>
        public bool available { get; set; }

        /// <summary>
        /// Icon ressource details
        /// </summary>
        public ResourceDto iconResource { get; set; }

        /// <summary>
        /// Emote animation in the bundled animator
        /// </summary>
        public ulong animationId { get; set; }
    }
}