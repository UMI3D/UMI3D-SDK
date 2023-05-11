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
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Serializable implementation of a vector with 3 float coordinates.
    /// </summary>
    [Serializable]
    public class Vector3Dto : UMI3DDto, IEquatable<Vector3Dto>
    {
        public float X;
        public float Y;
        public float Z;

        public static Vector3Dto one { get => new Vector3Dto() { X = 1, Y = 1, Z = 1 }; }
        public static Vector3Dto zero { get => new Vector3Dto() { X = 0, Y = 0, Z = 0 }; }

        public Vector3Dto() : base()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Vector3Dto(float x, float y, float z) : base()
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ((Vector3)this).ToString();
        }

        public bool Equals(Vector3Dto other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public static implicit operator Vector3Dto(Vector3 v)
        {
            if (v == null) return default;
            return new Vector3Dto(v.x, v.y, v.z);
        }

        public static implicit operator Vector3(Vector3Dto v)
        {
            if (v == null) return default;
            return new Vector3(v.X, v.Y, v.Z);
        }

        public float this[int i]
        {
            get { if (i == 0) return X; else if (i == 1) return Y; else if (i == 2) return Z; else throw new ArgumentOutOfRangeException(); }
            set { if (i == 0) X = value; else if (i == 1) Y = value; else if (i == 2) Z = value; else throw new ArgumentOutOfRangeException(); }
        }
    }
}