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

using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Loader for <see cref="UMI3DEmoteDto"/>. Manages the dtos from the client-side.
    /// </summary>
    public class UMI3DEmoteLoader
    {
        /// <summary>
        /// Load a UMI3DHandPose
        /// </summary>
        /// <param name="dto"></param>
        public static void Load(UMI3DEmoteDto dto)
        {
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();
            UMI3DClientUserTracking.Instance.EmoteChangedEvent.Invoke(dto);
        }

        /// <summary>
        /// Update a property from DTO
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var dto = entity.dto as UMI3DEmoteDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    dto.available = (bool)property.value;
                    UMI3DClientUserTracking.Instance.EmoteChangedEvent.Invoke(dto);
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Load an emote
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyKey"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    value = UMI3DNetworkingHelper.Read<UMI3DEmoteDto>(container);

                    UMI3DClientUserTracking.Instance.EmoteChangedEvent.Invoke(value as UMI3DEmoteDto);
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Update a property from a byted DTO
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var dto = entity.dto as UMI3DEmoteDto;
            if (dto == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    dto.available = UMI3DNetworkingHelper.Read<bool>(container);
                    UMI3DClientUserTracking.Instance.EmoteChangedEvent.Invoke(dto);
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}