/*
Copyright 2019 - 2022 Inetum
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

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Emote configuration file packaged in a dto
    /// </summary>
    [System.Serializable]
    public class UMI3DEmotesConfigDto : AbstractEntityDto, IEntity
    {
        /// <summary>
        /// List of available emotes dto
        /// </summary>
        public List<UMI3DEmoteDto> emotes = new List<UMI3DEmoteDto>();

        /// <summary>
        /// Should the emotes be available by default to users ? When set to true, all emotes are available at start, no matter the configuration
        /// </summary>
        public bool allAvailableByDefault = true;
    }
}

