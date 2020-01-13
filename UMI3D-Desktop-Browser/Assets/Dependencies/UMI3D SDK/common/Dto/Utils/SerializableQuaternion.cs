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
    public class SerializableQuaternion : UMI3DDto
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public SerializableQuaternion() : base()
        {
            X = 0;
            Y = 0;
            Z = 0;
            W = 1;
        }

        public SerializableQuaternion(float x, float y, float z, float w) : base()
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return ((Quaternion)this).ToString();
        }


        public static implicit operator SerializableQuaternion(Quaternion v)
        {
            return new SerializableQuaternion(v.x, v.y, v.z, v.w);
        }

        public static implicit operator Quaternion(SerializableQuaternion v)
        {
            return new Quaternion(v.X, v.Y, v.Z, v.W);
        }

    }
}