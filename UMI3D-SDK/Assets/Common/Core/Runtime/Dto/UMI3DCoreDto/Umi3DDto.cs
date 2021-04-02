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
        public static byte[] ToBson(UMI3DDto dto)
        {
            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.TypeNameHandling = TypeNameHandling.All;
                serializer.Serialize(writer, dto);
            }
            return ms.ToArray();
        }

        // Should be used to serialize UMI3D Data Transfer Object to json.
        /// <param name="dto">UMI3D Data Transfer Object to serialize.</param>
        public static string ToJson(UMI3DDto dto)
        {
            return JsonConvert.SerializeObject(dto, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        // Should be used to serialize this object to bson.
        public byte[] ToBson()
        {
            return ToBson(this);
        }

        // Should be used to serialize this object to json.
        public string ToJson()
        {
            return ToJson(this);
        }

        #endregion

        //
        //      DESERIALIZE
        #region deserialize

        // Should be used to deserialize a UMI3D Data Transfer Object from bson.
        /// <param name="dto">a bson serialized UMI3D Data Transfer Object.</param>
        public static UMI3DDto FromBson(byte[] bson)
        {
            MemoryStream ms = new MemoryStream(bson);
            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.TypeNameHandling = TypeNameHandling.All;
                UMI3DDto dto = serializer.Deserialize<UMI3DDto>(reader);
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

        #endregion

    }



}