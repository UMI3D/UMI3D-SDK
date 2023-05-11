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
