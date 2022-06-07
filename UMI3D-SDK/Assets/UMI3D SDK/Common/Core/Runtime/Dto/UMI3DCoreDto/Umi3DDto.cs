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
    /// Base classe of all data tranfer object
    /// </summary>
    [Serializable]
    public class UMI3DDto
    {

        //
        //  SERIALIZE
        #region serialize

        // Should be used to serialize UMI3D Data Transfer Object to bson.
        /// <param name="dto">UMI3D Data Transfer Object to serialize.</param>
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

        // Should be used to serialize UMI3D Data Transfer Object to json.
        /// <param name="dto">UMI3D Data Transfer Object to serialize.</param>
        public static string ToJson(UMI3DDto dto, TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            return JsonConvert.SerializeObject(dto, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = typeNameHandling
            });
        }

        // Should be used to serialize this object to bson.
        public byte[] ToBson(TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            return ToBson(this, typeNameHandling);
        }

        // Should be used to serialize this object to json.
        public string ToJson(TypeNameHandling typeNameHandling = TypeNameHandling.All)
        {
            return ToJson(this, typeNameHandling);
        }

        #endregion

        //
        //      DESERIALIZE
        #region deserialize

        // Should be used to deserialize a UMI3D Data Transfer Object from bson.
        /// <param name="dto">a bson serialized UMI3D Data Transfer Object.</param>
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

        // Should be used to deserialize a UMI3D Data Transfer Object from json.
        /// <param name="dto">a json serialized UMI3D Data Transfer Object.</param>
        public static UMI3DDto FromJson(string json)
        {
            return JsonConvert.DeserializeObject<UMI3DDto>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }


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