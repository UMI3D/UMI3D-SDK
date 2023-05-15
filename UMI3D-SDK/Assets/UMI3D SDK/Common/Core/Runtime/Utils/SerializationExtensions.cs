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

using UnityEngine;

namespace umi3d.common.utils.serialization
{
    /// <summary>
    /// Useful extension methods related to UMI3D serialization.
    /// </summary>
    public static partial class SerializationExtensions
    {
        /// <summary>
        /// Serialize an object in UMI3D standard if possible.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object ToSerializable(this object o)
        {
            switch (o)
            {
                case Quaternion q:
                    return q.Dto();
                case Vector2 v:
                    return v.Dto();
                case Vector3 v:
                    return v.Dto();
                case Color c:
                    return c.Dto();
                case Matrix4x4 m:
                    return m.Dto();
                default:
                    return o;
            }
        }

        /// <summary>
        /// Deserialize an object from UMI3D standard if possible.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object Deserialize(this object o)
        {
            switch (o)
            {
                case Vector4Dto q:
                    return q.Quaternion();
                case Vector2Dto v:
                    return v.Struct();
                case Vector3Dto v:
                    return v.Struct();
                case ColorDto c:
                    return c.Struct();
                case Matrix4x4Dto m:
                    return m.Struct();
                default:
                    return o;
            }
        }
    }
}
