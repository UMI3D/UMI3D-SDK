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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Load KHR Light
    /// </summary>
    public class KHR_lights_punctualLoader
    {
        /// <summary>
        /// Create a KHR Light.
        /// </summary>
        /// <param name="ldto">dto to be loaded.</param>
        /// <param name="node">node on which the light will be created.</param>
        public virtual void CreateLight(KHR_lights_punctual ldto, GameObject node)
        {
            Light light = node.GetOrAddComponent<Light>();
            light.shadows = LightShadows.Soft;

            light.intensity = ldto.intensity;
            light.name = ldto.name;
            light.range = ldto.range;
            light.color = ldto.color;
            if (ldto.type == KHR_lights_punctual.LightTypes.Directional.ToString())
                light.type = LightType.Directional;
            else if (ldto.type == KHR_lights_punctual.LightTypes.Point.ToString())
                light.type = LightType.Point;
            else if (ldto.type == KHR_lights_punctual.LightTypes.Spot.ToString())
            {
                light.type = LightType.Spot;
                light.innerSpotAngle = ldto.spot.innerConeAngle;
                light.spotAngle = ldto.spot.outerConeAngle;
            }
        }

        /// <summary>
        /// Update KHR light.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value</param>
        /// <returns></returns>
        public virtual bool SetLightPorperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var dto = (entity.dto as GlTFNodeDto)?.extensions.KHR_lights_punctual;
            var node = (entity as UMI3DNodeInstance);
            Light light = node?.gameObject?.GetComponent<Light>();
            if (property.property == UMI3DPropertyKeys.Light)
            {
                var lightdto = (KHR_lights_punctual)property.value;
                if (light != null && lightdto == null) GameObject.Destroy(light);
                else if (lightdto != null) CreateLight(lightdto, node.gameObject);
                return true;
            }
            if (dto == null || light == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.LightIntensity:
                    light.intensity = dto.intensity = (float)(Double)property.value;
                    break;
                case UMI3DPropertyKeys.LightColor:
                    light.color = dto.color = (SerializableColor)property.value;
                    break;
                case UMI3DPropertyKeys.LightRange:
                    light.range = dto.range = (float)(Double)property.value;
                    break;
                case UMI3DPropertyKeys.LightType:
                    dto.type = (string)property.value;
                    if (dto.type == KHR_lights_punctual.LightTypes.Directional.ToString())
                        light.type = LightType.Directional;
                    else if (dto.type == KHR_lights_punctual.LightTypes.Point.ToString())
                        light.type = LightType.Point;
                    else if (dto.type == KHR_lights_punctual.LightTypes.Spot.ToString())
                    {
                        light.type = LightType.Spot;
                        light.innerSpotAngle = dto.spot.innerConeAngle;
                        light.spotAngle = dto.spot.outerConeAngle;
                    }
                    break;
                case UMI3DPropertyKeys.LightSpot:
                    light.innerSpotAngle = dto.spot.innerConeAngle = ((KHR_lights_punctual.KHR_spot)property.value).innerConeAngle;
                    light.spotAngle = dto.spot.outerConeAngle = ((KHR_lights_punctual.KHR_spot)property.value).outerConeAngle;
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Update KHR light.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value</param>
        /// <returns></returns>
        public virtual bool SetLightPorperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var dto = (entity.dto as GlTFNodeDto)?.extensions.KHR_lights_punctual;
            var node = (entity as UMI3DNodeInstance);
            Light light = node?.gameObject?.GetComponent<Light>();
            if (propertyKey == UMI3DPropertyKeys.Light)
            {
                var lightdto = UMI3DNetworkingHelper.Read<KHR_lights_punctual>(container);
                if (light != null && lightdto == null) GameObject.Destroy(light);
                else if (lightdto != null) CreateLight(lightdto, node.gameObject);
                return true;
            }
            if (dto == null || light == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.LightIntensity:
                    light.intensity = dto.intensity = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.LightColor:
                    light.color = dto.color = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    break;
                case UMI3DPropertyKeys.LightRange:
                    light.range = dto.range = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.LightType:
                    dto.type = UMI3DNetworkingHelper.Read<string>(container);
                    if (dto.type == KHR_lights_punctual.LightTypes.Directional.ToString())
                        light.type = LightType.Directional;
                    else if (dto.type == KHR_lights_punctual.LightTypes.Point.ToString())
                        light.type = LightType.Point;
                    else if (dto.type == KHR_lights_punctual.LightTypes.Spot.ToString())
                    {
                        light.type = LightType.Spot;
                        light.innerSpotAngle = dto.spot.innerConeAngle;
                        light.spotAngle = dto.spot.outerConeAngle;
                    }
                    break;
                case UMI3DPropertyKeys.LightSpot:
                    var value = UMI3DNetworkingHelper.Read<KHR_lights_punctual.KHR_spot>(container);
                    light.innerSpotAngle = dto.spot.innerConeAngle = value.innerConeAngle;
                    light.spotAngle = dto.spot.outerConeAngle = value.outerConeAngle;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public virtual bool ReadLightPorperty(ref object value, uint propertyKey, ByteContainer container)
        {
            if (propertyKey == UMI3DPropertyKeys.Light)
            {
                value = UMI3DNetworkingHelper.Read<KHR_lights_punctual>(container);
                return true;
            }
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.LightIntensity:
                    value = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.LightColor:
                    value = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    break;
                case UMI3DPropertyKeys.LightRange:
                    value = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.LightType:
                    value = UMI3DNetworkingHelper.Read<string>(container);
                    break;
                case UMI3DPropertyKeys.LightSpot:
                    value = UMI3DNetworkingHelper.Read<KHR_lights_punctual.KHR_spot>(container);
                    break;
                default:
                    return false;
            }
            return true;
        }

    }

}