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

using umi3d.common;
using UnityEngine;

public static class ColorDtoExtension
{
    public static ColorDto Dto(this Color c)
    {
        if (c == null) return default;
        return new ColorDto()
        {
            R = c.r,
            G = c.g,
            B = c.b,
            A = c.a
        };
    }

    public static Color Struct(this ColorDto c)
    {
        if (c == null) return default;
        return new Color(c.R, c.G, c.B, c.A);
    }
}

public static class Vector2DtoExtension
{
    public static Vector2Dto Dto(this Vector2 v)
    {
        if (v == null) return default;
        return new Vector2Dto() {  X = v.x, Y = v.y };
    }

    public static Vector2 Struct(this Vector2Dto v)
    {
        if (v == null) return default;
        return new Vector2(v.X, v.Y);
    }
}

public static class Vector3DtoExtension
{
    public static Vector3Dto Dto(this Vector3 v)
    {
        if (v == null) return default;
        return new Vector3Dto() { X = v.x, Y = v.y, Z = v.z };
    }

    public static Vector3 Struct(this Vector3Dto v)
    {
        if (v == null) return default;
        return new Vector3(v.X, v.Y, v.Z);
    }
}

public static class Vector4DtoExtension
{
    public static Vector4Dto Dto(this Vector4 v)
    {
        if (v == null) return default;
        return new Vector4Dto() { X = v.x, Y = v.y, Z = v.z, W = v.w };
    }

    public static Vector4 Struct(this Vector4Dto v)
    {
        if (v == null) return default;
        return new Vector4(v.X, v.Y, v.Z,v.W);
    }

    public static Vector4Dto Dto(this Quaternion q)
    {
        if (q == null) return default;
        return new Vector4Dto() { X = q.x, Y = q.y, Z = q.z, W = q.w };
    }

    public static Quaternion Quaternion(this Vector4Dto v4)
    {
        if (v4 == null) return default;
        return new Quaternion(v4.X, v4.Y, v4.Z, v4.W);
    }
}