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
using UnityEngine;

namespace umi3d.edk
{

    public class UMI3DKHRLight
    {
        public UMI3DAsyncProperty<Color> objectLightColor;
        public string LightName;
        public UMI3DAsyncProperty<float> objectLightIntensity;
        public UMI3DAsyncProperty<float> objectLightRange;
        public UMI3DAsyncProperty<string> objectLightType;
        public UMI3DAsyncProperty<KHR_lights_punctual.KHR_spot> objectLightSpot;

        public UMI3DKHRLight(ulong objectId, Light light)
        {
            var comparer = new UMI3DAsyncPropertyEquality();
            objectLightIntensity = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.LightIntensity, light.intensity, null, comparer.FloatEquality);
            LightName = light.name;
            objectLightRange = new UMI3DAsyncProperty<float>(objectId, UMI3DPropertyKeys.LightRange, light.range, null, comparer.FloatEquality);
            objectLightColor = new UMI3DAsyncProperty<Color>(objectId, UMI3DPropertyKeys.LightColor, light.color, ToUMI3DSerializable.ToSerializableColor);
            objectLightType = new UMI3DAsyncProperty<string>(objectId, UMI3DPropertyKeys.LightType, null);
            objectLightSpot = new UMI3DAsyncProperty<KHR_lights_punctual.KHR_spot>(objectId, UMI3DPropertyKeys.LightSpot, null);
            switch (light.type)
            {
                case LightType.Directional:
                    objectLightType.SetValue(KHR_lights_punctual.LightTypes.Directional);
                    break;
                case LightType.Point:
                    objectLightType.SetValue(KHR_lights_punctual.LightTypes.Point);
                    break;
                case LightType.Spot:
                    objectLightType.SetValue(KHR_lights_punctual.LightTypes.Spot);
                    objectLightSpot.SetValue(new KHR_lights_punctual.KHR_spot() { innerConeAngle = light.innerSpotAngle, outerConeAngle = light.spotAngle });
                    break;
            }
        }

        /// <inheritdoc/>
        public KHR_lights_punctual ToDto(UMI3DUser user)
        {
            var dto = new KHR_lights_punctual
            {
                intensity = objectLightIntensity.GetValue(user),
                name = LightName,
                range = objectLightRange.GetValue(user),
                color = objectLightColor.GetValue(user),
                type = objectLightType.GetValue(user)
            };
            if (dto.type == null) return null;
            dto.spot = objectLightSpot.GetValue(user);
            return dto;
        }


    }
}