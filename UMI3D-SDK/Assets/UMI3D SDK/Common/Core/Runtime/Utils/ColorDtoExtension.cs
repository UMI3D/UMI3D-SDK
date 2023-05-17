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

public static class Matrix4x4DtoExtension
{
    public static Matrix4x4Dto Dto(this Matrix4x4 m)
    {
        if (m == null) return default;
        var M = new Matrix4x4Dto();
        for (int i = 0; i < 16; i++)
            M[i] = m[i];
        return M;
    }

    public static Matrix4x4 Struct(this Matrix4x4Dto m)
    {
        if (m == null) return default;
        var M = new Matrix4x4();
        for (int i = 0; i < 16; i++)
            M[i] = m[i];
        return M;
    }
}

public static class KeyframeDtoExtension
{
    public static KeyframeDto Dto(this Keyframe frame)
    {
        return new KeyframeDto()
        {
            point = new Vector2Dto() { X = frame.time, Y = frame.value },
            intTangeant = new Vector2Dto() { X = frame.inTangent, Y = frame.inWeight },
            outTangeant = new Vector2Dto() { X = frame.outTangent, Y = frame.outWeight },
        };
    }

    public static Keyframe Struct(this KeyframeDto frame)
    {
        if (frame == null) return default;
        return new Keyframe
        {
            time = frame.point.X,
            value = frame.point.Y,
            inTangent = frame.intTangeant.X,
            inWeight = frame.intTangeant.Y,
            outTangent = frame.outTangeant.X,
            outWeight = frame.outTangeant.Y
        };
    }
}

public static class AnimationCurveDtoExtension
{
    public static AnimationCurveDto Dto(this AnimationCurve curve)
    {
        if (curve == null) return default;
        var c = new AnimationCurveDto();
        foreach (var key in curve.keys)
        {
            c.keys.Add(key.Dto());
        }
        return c;
    }

    public static AnimationCurve Struct(this AnimationCurveDto curve)
    {
        if (curve == null || curve.keys.Count == 0) return new AnimationCurve();

        List<Keyframe> keys = new List<Keyframe>();
        foreach (var k in curve.keys)
            keys.Add(k.Struct());

        return new AnimationCurve(keys.ToArray());
    }
}