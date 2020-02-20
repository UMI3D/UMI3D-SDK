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
    public class SerializableVector2 : UMI3DDto
    {
        public float X;
        public float Y;

        public SerializableVector2() : base()
        {
            X = 0;
            Y = 0;
        }

        public SerializableVector2(float x, float y) : base()
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return ((Vector2)this).ToString();
        }

        public static implicit operator SerializableVector2(Vector2 v)
        {
            return new SerializableVector2(v.x, v.y);
        }

        public static implicit operator Vector2(SerializableVector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public float this[int i]
        {
            get { if (i == 0) return X; else if (i == 1) return Y; else throw new ArgumentOutOfRangeException(); }
            set { if (i == 0) X = value; else if (i == 1) Y = value; else throw new ArgumentOutOfRangeException(); }
        }
    }
}