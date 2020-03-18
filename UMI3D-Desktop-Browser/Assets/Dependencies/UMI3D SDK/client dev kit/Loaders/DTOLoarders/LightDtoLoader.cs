/*
Copyright 2019 Gfi Informatique

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
    /// Load light from dto.
    /// </summary>
    public class LightDtoLoader : AbstractObjectDTOLoader<LightDto>
    {
        /// <summary>
        /// Load a Light from dto and pass its value to a given callback.
        /// </summary>
        /// <param name="dto">Data to load from.</param>
        /// <param name="callback">Callback to execute after loading.</param>
        public override void LoadDTO(LightDto dto, Action<GameObject> callback, Action<string> onError)
        {
            GameObject res = new GameObject();
            res.AddComponent<Light>();
            callback(res);
            InitObjectFromDto(res, dto);
        }

        /// <summary>
        /// Update a Light from dto.
        /// </summary>
        /// <param name="go">GameObject to update.</param>
        /// <param name="olddto">Old dto describing the object.</param>
        /// <param name="newdto">Dto to update the object to.</param>
        public override void UpdateFromDTO(GameObject go, LightDto olddto, LightDto newdto)
        {
            Light light = go.GetComponent<Light>();
            UpdateLightType(light, olddto, newdto);
            UpdateColor(light, olddto, newdto);
            UpdateIntensity(light, olddto, newdto);
            UpdateRanges(light, olddto, newdto);
            UpdateShadows(light, olddto, newdto);
            base.UpdateFromDTO(go, olddto, newdto);
        }

        /// <summary>
        /// Update the light color (internal use).
        /// </summary>
        /// <param name="light">Light to update</param>
        /// <param name="olddto">Previous dto describing the light</param>
        /// <param name="newdto">Dto to update the light to</param>
        void UpdateColor(Light light, LightDto olddto, LightDto newdto)
        {
            if (olddto == null || olddto.Color != newdto.Color)
            {
                light.color = newdto.Color;
            }
        }

        /// <summary>
        /// Update the light type (internal use).
        /// </summary>
        /// <param name="light">Light to update</param>
        /// <param name="olddto">Previous dto describing the light</param>
        /// <param name="newdto">Dto to update the light to</param>
        void UpdateLightType(Light light, LightDto olddto, LightDto newdto)
        {
            if (olddto == null || olddto.Type != newdto.Type)
            {
                light.type = newdto.Type.Convert();
            }
        }

        /// <summary>
        /// Update the light range (internal use).
        /// </summary>
        /// <param name="light">Light to update</param>
        /// <param name="olddto">Previous dto describing the light</param>
        /// <param name="newdto">Dto to update the light to</param>
        void UpdateRanges(Light light, LightDto olddto, LightDto newdto)
        {
            if (olddto == null || olddto.Range != newdto.Range)
                light.range = newdto.Range;

            if (olddto == null || olddto.SpotAngle != newdto.SpotAngle)
                light.spotAngle = 360f * 0.5f * newdto.SpotAngle / (float)Math.PI;
        }

        /// <summary>
        /// Update the light intensity (internal use).
        /// </summary>
        /// <param name="light">Light to update</param>
        /// <param name="olddto">Previous dto describing the light</param>
        /// <param name="newdto">Dto to update the light to</param>
        void UpdateIntensity(Light light, LightDto olddto, LightDto newdto)
        {
            if (olddto == null || olddto.Intensity != newdto.Intensity)
            {
                light.intensity = newdto.Intensity;
                if (!light.enabled)
                    RenderSettings.ambientIntensity = newdto.Intensity;
            }

            if (olddto == null || olddto.BounceIntensity != newdto.BounceIntensity)
                light.bounceIntensity = newdto.BounceIntensity;
        }

        /// <summary>
        /// Update the light shadows (internal use).
        /// </summary>
        /// <param name="light">Light to update</param>
        /// <param name="olddto">Previous dto describing the light</param>
        /// <param name="newdto">Dto to update the light to</param>
        void UpdateShadows(Light light, LightDto olddto, LightDto newdto)
        {
            if (olddto == null || olddto.ShadowsIntensity != newdto.ShadowsIntensity)
                light.shadowStrength = newdto.ShadowsIntensity;

            if (olddto == null || olddto.ShadowsBiais != newdto.ShadowsBiais)
                light.shadowBias = newdto.ShadowsBiais;

            if (olddto == null || olddto.ShadowsNormalBiais != newdto.ShadowsNormalBiais)
                light.shadowNormalBias = newdto.ShadowsNormalBiais;

            if (olddto == null || olddto.ShadowsNearPlane != newdto.ShadowsNearPlane)
                light.shadowNearPlane = newdto.ShadowsNearPlane;

            if (olddto == null || olddto.ShadowsType != newdto.ShadowsType)
                light.shadows = newdto.ShadowsType.Convert();
        }
    }
}