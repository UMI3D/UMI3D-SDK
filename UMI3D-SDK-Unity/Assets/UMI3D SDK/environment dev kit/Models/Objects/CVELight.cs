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

namespace umi3d.edk
{
    public class CVELight : AbstractObject3D<LightDto>
    {
        #region CVE description
        public LightTypes _type = LightTypes.Directional;
        public Color color = Color.grey;
        public float intensity = 0.4f;
        public float bounceIntensity = 0.4f;
        public float range = 10f;
        [RangeAttribute(0, (float)Math.PI)]
        public float spotAngle = 0.2f;
        public ShadowType shadowsType = 0;
        [RangeAttribute(0, 1f)]
        public float shadowsIntensity = 0.8f;
        [RangeAttribute(0, 2f)]
        public float shadowsBiais = 0.495f;
        [RangeAttribute(0, 3f)]
        public float shadowsNormalBiais = 0.4f;
        [RangeAttribute(0, 10f)]
        public float shadowsNearPlane = 0.2f;

        public UMI3DAsyncProperty<LightTypes> lightType;
        public UMI3DAsyncProperty<Color> lightColor;
        public UMI3DAsyncProperty<float> lightIntensity;
        public UMI3DAsyncProperty<float> lightBounceIntensity;
        public UMI3DAsyncProperty<float> lightRange;
        public UMI3DAsyncProperty<float> lightSpotAngle;
        public UMI3DAsyncProperty<ShadowType> lightShadowType;
        public UMI3DAsyncProperty<float> lightShadowIntensity;
        public UMI3DAsyncProperty<float> lightShadowBiais;
        public UMI3DAsyncProperty<float> lightShadowNormalBiais;
        public UMI3DAsyncProperty<float> lightShadowNearPlane;

        protected override void initDefinition()
        {
            base.initDefinition();

            lightType = new UMI3DAsyncProperty<LightTypes>(PropertiesHandler, _type);
            lightType.OnValueChanged += (LightTypes value) => _type = value;

            lightColor = new UMI3DAsyncProperty<Color>(PropertiesHandler, this.color);
            lightColor.OnValueChanged += (Color value) => color = value;

            lightIntensity = new UMI3DAsyncProperty<float>(PropertiesHandler, intensity);
            lightIntensity.OnValueChanged += (float value) => intensity = value;

            lightBounceIntensity = new UMI3DAsyncProperty<float>(PropertiesHandler, bounceIntensity);
            lightBounceIntensity.OnValueChanged += (float value) => bounceIntensity = value;

            lightRange = new UMI3DAsyncProperty<float>(PropertiesHandler, range);
            lightRange.OnValueChanged += (float value) => range = value;

            lightSpotAngle = new UMI3DAsyncProperty<float>(PropertiesHandler, spotAngle);
            lightSpotAngle.OnValueChanged += (float value) => spotAngle = value;

            lightShadowType = new UMI3DAsyncProperty<ShadowType>(PropertiesHandler, shadowsType);
            lightShadowType.OnValueChanged += (ShadowType value) => shadowsType = value;

            lightShadowBiais = new UMI3DAsyncProperty<float>(PropertiesHandler, shadowsBiais);
            lightShadowBiais.OnValueChanged += (float value) => shadowsBiais = value;

            lightShadowIntensity = new UMI3DAsyncProperty<float>(PropertiesHandler, shadowsIntensity);
            lightShadowIntensity.OnValueChanged += (float value) => shadowsIntensity = value;

            lightShadowNormalBiais = new UMI3DAsyncProperty<float>(PropertiesHandler, shadowsNormalBiais);
            lightShadowNormalBiais.OnValueChanged += (float value) => shadowsNormalBiais = value;

            lightShadowNearPlane = new UMI3DAsyncProperty<float>(PropertiesHandler, shadowsNearPlane);
            lightShadowNearPlane.OnValueChanged += (float value) => shadowsNearPlane = value;
        }

        protected override void SyncProperties()
        {
            base.SyncProperties();
            if (inited)
            {
                lightType.SetValue(_type);
                lightColor.SetValue(color);
                lightIntensity.SetValue(intensity);
                lightBounceIntensity.SetValue(bounceIntensity);
                lightRange.SetValue(range);
                lightSpotAngle.SetValue(spotAngle);
                lightShadowType.SetValue(shadowsType);
                lightShadowBiais.SetValue(shadowsBiais);
                lightShadowIntensity.SetValue(shadowsIntensity);
                lightShadowNormalBiais.SetValue(shadowsNormalBiais);
                lightShadowNearPlane.SetValue(shadowsNearPlane);
            }
            SyncEnvironmentLight();

        }

        public void SyncEnvironmentLight()
        {
            Init_Preview();
            if (!preview) return;
            Light light = null;
            switch (_type)
            {
                case LightTypes.Directional:
                    if (!preview.activeSelf) preview.SetActive(true);
                    light = preview.GetComponent<Light>();
                    if (light != null)
                        light.type = LightType.Directional;
                    break;
                case LightTypes.Point:
                    if (!preview.activeSelf) preview.SetActive(true);
                    light = preview.GetComponent<Light>();
                    if (light != null)
                        light.type = LightType.Point;
                    break;
                case LightTypes.Spot:
                    if (!preview.activeSelf) preview.SetActive(true);
                    light = preview.GetComponent<Light>();
                    if (light != null)
                        light.type = LightType.Spot;
                    break;
            }
            if (light != null)
            { 
                light.color = color;
                light.intensity = intensity;
                light.bounceIntensity = bounceIntensity;
                light.range = range;
                light.spotAngle = 360f * 0.5f * spotAngle / (float)Math.PI;
                light.shadowBias = shadowsBiais;
                light.shadowStrength = shadowsIntensity;
                light.shadowNormalBias = shadowsNormalBiais;
                light.shadowNearPlane = shadowsNearPlane;
                light.shadows = shadowsType.Convert();
                light.enabled = this.enabled;
            }
        }

        public override LightDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);
            dto.Type = lightType.GetValue(user);
            dto.Color = lightColor.GetValue(user);
            dto.Intensity = lightIntensity.GetValue(user);
            dto.BounceIntensity = lightBounceIntensity.GetValue(user);
            dto.Range = lightRange.GetValue(user);
            dto.SpotAngle = lightSpotAngle.GetValue(user);
            dto.ShadowsType = lightShadowType.GetValue(user);
            dto.ShadowsBiais = lightShadowBiais.GetValue(user);
            dto.ShadowsIntensity = lightShadowIntensity.GetValue(user);
            dto.ShadowsNormalBiais = lightShadowNormalBiais.GetValue(user);
            dto.ShadowsNearPlane = lightShadowNearPlane.GetValue(user);
            return dto;
        }

        public override LightDto CreateDto()
        {
            return new LightDto();
        }
        #endregion

        #region Unity implementation

        void Init_Preview()
        {
            if (!preview)
            {
                preview = new GameObject();
                preview.name = "Light_preview";
                preview.hideFlags = HideFlags.NotEditable;
                Light l = preview.AddComponent<Light>();
#if UNITY_EDITOR
                l.lightmapBakeType = LightmapBakeType.Realtime;
#endif
                l.type = _type.Convert();
                preview.transform.SetParent(transform, false);
            }
        }

        protected override void Start()
        {
            base.Start();
            Init_Preview();
        }
        #endregion


    }
}