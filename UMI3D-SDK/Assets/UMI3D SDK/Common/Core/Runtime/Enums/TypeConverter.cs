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

using UnityEngine;

namespace umi3d.common
{
    public static class PrimitiveTypeConverter
    {

        public static PrimitiveType Convert(this MeshPrimitive MeshPrimitive)
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

        public static MeshPrimitive Convert(this PrimitiveType PrimitiveType)
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

    public static class ShadowTypeConverter
    {

        public static ShadowType Convert(this LightShadows shadowType)
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

        public static LightShadows Convert(this ShadowType shadowType)
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

    public static class AmbientTypeConverter
    {

        public static AmbientType Convert(this UnityEngine.Rendering.AmbientMode AmbientMode)
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

        public static UnityEngine.Rendering.AmbientMode Convert(this AmbientType AmbientType)
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

    public static class ImageTypeConverter
    {

        public static ImageType Convert(this UnityEngine.UI.Image.Type Type)
        {
            switch (Type)
            {
                case UnityEngine.UI.Image.Type.Simple:
                    return ImageType.Simple;
                case UnityEngine.UI.Image.Type.Sliced:
                    return ImageType.Sliced;
                case UnityEngine.UI.Image.Type.Tiled:
                    return ImageType.Tiled;
                case UnityEngine.UI.Image.Type.Filled:
                    return ImageType.Filled;
            }
            return ImageType.Simple;
        }

        public static UnityEngine.UI.Image.Type Convert(this ImageType Type)
        {
            switch (Type)
            {
                case ImageType.Simple:
                    return UnityEngine.UI.Image.Type.Simple;
                case ImageType.Sliced:
                    return UnityEngine.UI.Image.Type.Sliced;
                case ImageType.Tiled:
                    return UnityEngine.UI.Image.Type.Tiled;
                case ImageType.Filled:
                    return UnityEngine.UI.Image.Type.Filled;
            }
            return UnityEngine.UI.Image.Type.Simple;
        }
    }
    public static class TextAnchortTypeConverter
    {

        public static TextAnchorType Convert(this TextAnchor Type)
        {
            switch (Type)
            {
                case TextAnchor.UpperLeft:
                    return TextAnchorType.UpperLeft;
                case TextAnchor.UpperCenter:
                    return TextAnchorType.UpperCenter;
                case TextAnchor.UpperRight:
                    return TextAnchorType.UpperRight;
                case TextAnchor.MiddleLeft:
                    return TextAnchorType.MiddleLeft;
                case TextAnchor.MiddleCenter:
                    return TextAnchorType.MiddleCenter;
                case TextAnchor.MiddleRight:
                    return TextAnchorType.MiddleRight;
                case TextAnchor.LowerLeft:
                    return TextAnchorType.LowerLeft;
                case TextAnchor.LowerCenter:
                    return TextAnchorType.LowerCenter;
                case TextAnchor.LowerRight:
                    return TextAnchorType.LowerRight;
            }
            return TextAnchorType.MiddleCenter;
        }

        public static TextAnchor Convert(this TextAnchorType Type)
        {
            switch (Type)
            {
                case TextAnchorType.UpperLeft:
                    return TextAnchor.UpperLeft;
                case TextAnchorType.UpperCenter:
                    return TextAnchor.UpperCenter;
                case TextAnchorType.UpperRight:
                    return TextAnchor.UpperRight;
                case TextAnchorType.MiddleLeft:
                    return TextAnchor.MiddleLeft;
                case TextAnchorType.MiddleCenter:
                    return TextAnchor.MiddleCenter;
                case TextAnchorType.MiddleRight:
                    return TextAnchor.MiddleRight;
                case TextAnchorType.LowerLeft:
                    return TextAnchor.LowerLeft;
                case TextAnchorType.LowerCenter:
                    return TextAnchor.LowerCenter;
                case TextAnchorType.LowerRight:
                    return TextAnchor.LowerRight;
            }
            return TextAnchor.MiddleCenter;
        }
    }
    public static class FontStyleTypeConverter
    {

        public static FontStyleType Convert(this FontStyle Type)
        {
            switch (Type)
            {
                case FontStyle.Normal:
                    return FontStyleType.Normal;
                case FontStyle.Bold:
                    return FontStyleType.Bold;
                case FontStyle.Italic:
                    return FontStyleType.Italic;
                case FontStyle.BoldAndItalic:
                    return FontStyleType.BoldAndItalic;
            }
            return FontStyleType.Normal;
        }

        public static FontStyle Convert(this FontStyleType Type)
        {
            switch (Type)
            {
                case FontStyleType.Normal:
                    return FontStyle.Normal;
                case FontStyleType.Bold:
                    return FontStyle.Bold;
                case FontStyleType.Italic:
                    return FontStyle.Italic;
                case FontStyleType.BoldAndItalic:
                    return FontStyle.BoldAndItalic;
            }
            return FontStyle.Normal;
        }
    }

    public static class HorizontalWrapTypeConverter
    {

        public static HorizontalWrapType Convert(this HorizontalWrapMode Type)
        {
            switch (Type)
            {
                case HorizontalWrapMode.Wrap:
                    return HorizontalWrapType.Wrap;
                case HorizontalWrapMode.Overflow:
                    return HorizontalWrapType.Overflow;
            }
            return HorizontalWrapType.Wrap;
        }

        public static HorizontalWrapMode Convert(this HorizontalWrapType Type)
        {
            switch (Type)
            {
                case HorizontalWrapType.Wrap:
                    return HorizontalWrapMode.Wrap;
                case HorizontalWrapType.Overflow:
                    return HorizontalWrapMode.Overflow;
            }
            return HorizontalWrapMode.Wrap;
        }
    }

    public static class VerticalWrapTypeConverter
    {

        public static VerticalWrapType Convert(this VerticalWrapMode Type)
        {
            switch (Type)
            {
                case VerticalWrapMode.Truncate:
                    return VerticalWrapType.Truncate;
                case VerticalWrapMode.Overflow:
                    return VerticalWrapType.Overflow;
            }
            return VerticalWrapType.Truncate;
        }

        public static VerticalWrapMode Convert(this VerticalWrapType Type)
        {
            switch (Type)
            {
                case VerticalWrapType.Truncate:
                    return VerticalWrapMode.Truncate;
                case VerticalWrapType.Overflow:
                    return VerticalWrapMode.Overflow;
            }
            return VerticalWrapMode.Truncate;
        }
    }

}