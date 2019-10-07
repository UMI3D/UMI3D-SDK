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
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.common
{
    static public class PrimitiveTypeConverter
    {

        static public PrimitiveType Convert(this MeshPrimitive MeshPrimitive)
        {
            switch (MeshPrimitive)
            {
                case MeshPrimitive.Capsule:
                    return PrimitiveType.Capsule;
                case MeshPrimitive.Sphere:
                    return PrimitiveType.Sphere;
                case MeshPrimitive.Cube:
                    return PrimitiveType.Cube;
                case MeshPrimitive.Plane:
                    return PrimitiveType.Plane;
                case MeshPrimitive.Cylinder:
                    return PrimitiveType.Cylinder;
                default:
                    return PrimitiveType.Cube;
            }
        }

        static public MeshPrimitive Convert(this PrimitiveType PrimitiveType)
        {
            switch (PrimitiveType)
            {
                case PrimitiveType.Capsule:
                    return MeshPrimitive.Capsule;
                case PrimitiveType.Sphere:
                    return MeshPrimitive.Sphere;
                case PrimitiveType.Cube:
                    return MeshPrimitive.Cube;
                case PrimitiveType.Plane:
                    return MeshPrimitive.Plane;
                case PrimitiveType.Cylinder:
                    return MeshPrimitive.Cylinder;
                default:
                    return MeshPrimitive.Cube;
            }
        }
    }

    static public class LightTypeConverter
    {
        
        static public LightType Convert(this LightTypes LightTypes)
        {
            switch (LightTypes)
            {
                case LightTypes.Directional:
                    return LightType.Directional;
                case LightTypes.Point:
                    return LightType.Point;
                case LightTypes.Spot:
                    return LightType.Spot;
            }
            return 0;
        }

        static public LightTypes Convert(this LightType LightType)
        {
            switch (LightType)
            {
                case LightType.Directional:
                    return LightTypes.Directional;
                case LightType.Point:
                    return LightTypes.Point;
                case LightType.Spot:
                    return LightTypes.Spot;
            }
            return 0;
        }
    }

    static public class ShadowTypeConverter
    {

        static public ShadowType Convert(this LightShadows shadowType)
        {
            switch (shadowType)
            {
                case LightShadows.None:
                    return ShadowType.None;
                case LightShadows.Hard:
                    return ShadowType.Hard;
                case LightShadows.Soft:
                    return ShadowType.Soft;
            }
            return 0;
        }

        static public LightShadows Convert(this ShadowType shadowType)
        {
            switch (shadowType)
            {
                case ShadowType.None:
                    return LightShadows.None;
                case ShadowType.Soft:
                    return LightShadows.Soft;
                case ShadowType.Hard:
                    return LightShadows.Hard;
            }
            return 0;
        }
    }

    static public class AmbientTypeConverter
    {

        static public AmbientType Convert(this UnityEngine.Rendering.AmbientMode AmbientMode)
        {
            switch (AmbientMode)
            {
                case UnityEngine.Rendering.AmbientMode.Skybox:
                    return AmbientType.Skybox;
                case UnityEngine.Rendering.AmbientMode.Flat:
                    return AmbientType.Flat;
                case UnityEngine.Rendering.AmbientMode.Trilight:
                    return AmbientType.Gradient;
            }
            return 0;
        }

        static public UnityEngine.Rendering.AmbientMode Convert(this AmbientType AmbientType)
        {
            switch (AmbientType)
            {
                case AmbientType.Skybox:
                    return UnityEngine.Rendering.AmbientMode.Skybox;
                case AmbientType.Flat:
                    return UnityEngine.Rendering.AmbientMode.Flat;
                case AmbientType.Gradient:
                    return UnityEngine.Rendering.AmbientMode.Trilight;
            }
            return 0;
        }
    }

}