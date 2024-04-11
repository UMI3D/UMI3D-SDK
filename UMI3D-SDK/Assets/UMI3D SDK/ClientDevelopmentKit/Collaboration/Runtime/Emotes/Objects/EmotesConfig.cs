/*
Copyright 2019 - 2024 Inetum

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
using System.Linq;
using umi3d.common.collaboration.dto.emotes;

namespace umi3d.cdk.collaboration.emotes
{
    public class EmotesConfig : IBrowserEntity
    {
        public EmotesConfig(UMI3DEmotesConfigDto dto, IEnumerable<Emote> emotes)
        {
            this.dto = dto ?? throw new System.ArgumentNullException(nameof(dto));
            Emotes = emotes?.ToList() ?? throw new System.ArgumentNullException(nameof(emotes));
            AllAvailableByDefault = dto.allAvailableByDefault;
        }

        private readonly UMI3DEmotesConfigDto dto;

        public ulong Id => dto.id;

        public IReadOnlyList<Emote> Emotes { get; internal set; }

        public bool AllAvailableByDefault { get; private set; }
    }
}
