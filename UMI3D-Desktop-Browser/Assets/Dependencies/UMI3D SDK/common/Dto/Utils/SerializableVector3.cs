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
using System.Xml.Serialization;
using UnityEngine;

namespace umi3d.common
{
    [Serializable]
    public class SerializableVector3 : UMI3DDto
    {
        public float X;
        public float Y;
        public float Z;

        public SerializableVector3() : base()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public SerializableVector3(float x, float y, float z) : base()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return ((Vector3)this).ToString();
        }

        public static implicit operator SerializableVector3(Vector3 v)
        {
            return new SerializableVector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector3(SerializableVector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public float this[int i]
        {
            get { if (i == 0) return X; else if (i == 1) return Y; else if (i == 2) return Z; else throw new ArgumentOutOfRangeException(); }
            set { if (i == 0) X = value; else if (i == 1) Y = value; else if (i == 2) Z = value; else throw new ArgumentOutOfRangeException(); }
        }

    }
}