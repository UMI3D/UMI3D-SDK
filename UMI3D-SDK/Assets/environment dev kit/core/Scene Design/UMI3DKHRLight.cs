using umi3d.common;
using umi3d.edk;
using UnityEngine;

namespace edk
{

    public class UMI3DKHRLight
    {



        public UMI3DKHRLight(string objectId, Light light)
        {
            UMI3DAsyncPropertyEquality comparer = new UMI3DAsyncPropertyEquality();
            objectLightIntensity = new UMI3DAsyncProperty<float>(objectId,KHR_lightsKeys.Intensity, light.intensity,null,comparer.FloatEquality);
            LightName = light.name;
            objectLightRange = new UMI3DAsyncProperty<float>(objectId,KHR_lightsKeys.Range, light.range,null,comparer.FloatEquality);
            objectLightColor = new UMI3DAsyncProperty<Color>(objectId, KHR_lightsKeys.Color, light.color, ToUMI3DSerializable.ToSerializableColor);
            objectLightType = new UMI3DAsyncProperty<string>(objectId, KHR_lightsKeys.type, null);
            objectLightSpot = new UMI3DAsyncProperty<KHR_lights_punctual.KHR_spot>(objectId, KHR_lightsKeys.spot, null);
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

        public KHR_lights_punctual ToDto(UMI3DUser user)
        {
            KHR_lights_punctual dto = new KHR_lights_punctual();
            dto.intensity = objectLightIntensity.GetValue(user);
            dto.name = LightName;
            dto.range = objectLightRange.GetValue(user);
            dto.color = objectLightColor.GetValue(user);
            dto.type = objectLightType.GetValue(user);
            if (dto.type == null) return null;
            dto.spot = objectLightSpot.GetValue(user);
            return dto;
        }

        public UMI3DAsyncProperty<Color> objectLightColor;
        public string LightName;
        public UMI3DAsyncProperty<float> objectLightIntensity;
        public UMI3DAsyncProperty<float> objectLightRange;
        public UMI3DAsyncProperty<string> objectLightType;
        public UMI3DAsyncProperty<KHR_lights_punctual.KHR_spot> objectLightSpot;


    }
}