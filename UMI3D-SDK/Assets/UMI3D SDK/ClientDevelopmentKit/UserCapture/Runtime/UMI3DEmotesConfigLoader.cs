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
using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    public class UMI3DEmotesConfigLoader
    {
        /// <summary>
        /// Load a UMI3D Emote config
        /// </summary>
        /// <param name="dto"></param>
        public static void Load(UMI3DEmotesConfigDto dto)
        {
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();
            foreach (var emoteDto in dto.emotes)
                UMI3DEnvironmentLoader.RegisterEntityInstance(emoteDto.id, emoteDto, null).NotifyLoaded();
            UMI3DClientUserTracking.Instance.EmotesLoadedEvent.Invoke(dto);
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var dto = entity.dto as UMI3DEmotesConfigDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    dto.emotes = property.value as List<UMI3DEmoteDto>;
                    UMI3DClientUserTracking.Instance.EmotesLoadedEvent.Invoke(dto);
                    break;

                default:
                    return false;
            }
            return true;
        }

        public static bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    value = UMI3DNetworkingHelper.Read<UMI3DEmotesConfigDto>(container);

                    UMI3DClientUserTracking.Instance.EmotesLoadedEvent.Invoke(value as UMI3DEmotesConfigDto);
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var dto = entity.dto as UMI3DEmotesConfigDto;
            if (dto == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    dto.emotes = UMI3DNetworkingHelper.Read<List<UMI3DEmoteDto>>(container);
                    UMI3DClientUserTracking.Instance.EmotesLoadedEvent.Invoke(dto);
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}