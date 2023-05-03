/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.common
{
    /// <summary>
    /// Tagging interface for Serializer subModule. Rather implement generic IUMI3DSerializerSubModule<T>;
    /// </summary>
    public interface IUMI3DSerializerSubModule
    {
    }

    public interface IUMI3DSerializerSubModule<T> : IUMI3DSerializerSubModule
    {
        /// <summary>
        /// Write the object as a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">type of the object to serialize.</typeparam>
        /// <param name="dto">Object to serialize.</param>
        /// <param name="bytable">Object as a bytable.</param>
        public Bytable Write(T dto);

        /// <summary>
        /// Retrieve an object from a <see cref="Bytable"/>.
        /// </summary>
        /// <typeparam name="T">type of the object to deserialize.</typeparam>
        /// <param name="container">Byte container containing the object.</param>
        /// <param name="result">Deserialized object.</param>
        /// <returns>Has the content successfully been read?</returns>
        public bool Read(ByteContainer container, out T result);
    }
}