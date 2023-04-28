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

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;

namespace umi3d.common
{
    /// <summary>
    /// Base class of all data tranfer objects used in exchanges between the server and clients using UMI3D.
    /// </summary>
    public partial class UMI3DDto
    {

        //
        //      DESERIALIZE
        #region deserialize

        /// <summary>
        /// Should be used to deserialize a UMI3D Data Transfer Object from BSON.
        /// </summary>
        /// <param name="bson">A bson that is a serialized UMI3D Data Transfer Object.</param>
        /// <param name="typeNameHandling">JSON handling parameter to include or not type information when serializing.</param>
        /// <returns>The DTO contained in the BSON.</returns>
        public static UMI3DDto FromBson(byte[] bson, TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            var ms = new MemoryStream(bson);
            using (var reader = new BsonReader(ms))
            {
                var serializer = new JsonSerializer
                {
                    TypeNameHandling = typeNameHandling
                };
                UMI3DDto dto = serializer.Deserialize<UMI3DDto>(reader);
                return dto;
            }
        }

        /// <summary>
        /// Should be used to deserialize a DTO of type <see cref="T"/> from BSON.
        /// </summary>
        /// <param name="bson">A bson that is a serialized UMI3D Data Transfer Object.</param>
        /// <param name="typeNameHandling">JSON handling parameter to include or not type information when serializing.</param>
        /// <returns>The DTO contained in the BSON.</returns>
        public static T FromBson<T>(byte[] bson, TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            var ms = new MemoryStream(bson);
            using (var reader = new BsonReader(ms))
            {
                var serializer = new JsonSerializer
                {
                    TypeNameHandling = typeNameHandling
                };
                T dto = serializer.Deserialize<T>(reader);
                return dto;
            }
        }

        /// <summary>
        /// Should be used to deserialize a UMI3D Data Transfer Object from JSON.
        /// </summary>
        /// <param name="json">A bson that is a serialized UMI3D Data Transfer Object.</param>
        /// <returns>The DTO contained in the JSON.</returns>
        public static UMI3DDto FromJson(string json)
        {
            return JsonConvert.DeserializeObject<UMI3DDto>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        /// <summary>
        /// Should be used to deserialize a DTO of type <see cref="T"/> from JSON.
        /// </summary>
        /// <param name="json">A bson that is a serialized UMI3D Data Transfer Object.</param>
        /// <param name="typeNameHandling">JSON handling parameter to include or not type information when serializing.</param>
        /// <returns>The DTO contained in the JSON.</returns>
        public static T FromJson<T>(string json, TypeNameHandling typeNameHandling = TypeNameHandling.All, System.Collections.Generic.IList<JsonConverter> converters = null)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = typeNameHandling,
                Converters = converters,
            });
        }
        #endregion

    }

}