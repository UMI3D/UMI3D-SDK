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
using System;
using System.IO;

namespace umi3d.common
{
    /// <summary>
    /// Base class of all data tranfer objects used in exchanges between the server and clients using UMI3D.
    /// </summary>
    [Serializable]
    public class UMI3DDto
    {

        //
        //  SERIALIZE
        #region serialize


        /// <summary>
        /// Should be used to serialize UMI3D Data Transfer Object to BSON.
        /// </summary>
        /// <param name="dto">UMI3D Data Transfer Object to serialize.</param>
        /// <param name="typeNameHandling">JSON handling parameter to include or not type information when serializing.</param>
        /// <returns>A BSON corresponding to the DTO parameter.</returns>
        public static byte[] ToBson(UMI3DDto dto, TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer
                {
                    TypeNameHandling = typeNameHandling
                };
                serializer.Serialize(writer, dto);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Should be used to serialize UMI3D Data Transfer Object to JSON.
        /// </summary>
        /// <param name="dto">UMI3D Data Transfer Object to serialize.</param>
        /// <param name="typeNameHandling">JSON handling parameter to include or not type information when serializing.</param>
        /// <returns>A JSON corresponding to the DTO parameter</returns>
        public static string ToJson(UMI3DDto dto, TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            return JsonConvert.SerializeObject(dto, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = typeNameHandling
            });
        }

        /// <summary>
        /// Should be used to serialize this object to BSON.
        /// </summary>
        /// <param name="typeNameHandling">JSON handling parameter to include or not type information when serializing.</param>
        /// <returns>A bson corresponding to this object./returns>
        public byte[] ToBson(TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            return ToBson(this, typeNameHandling);
        }

        /// <summary>
        /// Should be used to serialize this object to JSON.
        /// </summary>
        /// <param name="typeNameHandling">JSON handling parameter to include or not type information when serializing.</param>
        /// <returns>A JSON corresponding to this object.</returns>
        public string ToJson(TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            return ToJson(this, typeNameHandling);
        }

        #endregion

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