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

using System;
using System.Collections.Generic;
using umi3d.common.dto.binding;

namespace umi3d.common.binding
{
    /// <summary>
    /// Serializer for <see cref="BindingDto"/>, <see cref="MultiBindingDataDto"/> and <see cref="NodeBindingDataDto"/>.
    /// </summary>
    public class BindingSerializerModule : UMI3DSerializerModule
    {
        protected readonly Dictionary<Type, IUMI3DSerializerSubModule> bindingSerializers = new()
        {
            { typeof(BindingDto),           new BindingSerializer()},
            { typeof(MultiBindingDataDto),  new MultiBindingDataSerializer()},
            { typeof(NodeBindingDataDto),   new NodeBindingDataSerializer()}
        };

        /// <summary>
        /// Index that indicate what is the data type of of a binding.
        /// </summary>
        /// Serialization indices are used to support the polymorphism of one of the data field in a binding.
        protected static class BindingSerializationIndices
        {
            public const int MULTI_BINDING_INDEX = 0;
            public const int NODE_BINDING_INDEX = 1;
        }

        /// <summary>
        /// Get correcy serializer in submodules
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected IUMI3DSerializerSubModule<T> GetSerializer<T>()
        {
            return bindingSerializers[typeof(T)] as IUMI3DSerializerSubModule<T>;
        }

        /// <inheritdoc/>
        public virtual bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            readable = true;
            if (bindingSerializers.ContainsKey(typeof(T)))
            {
                readable &= GetSerializer<T>().Read(container, out result);
                if (!readable)
                    result = default;
                return readable;
            }
            //specific case of multibinding, to refacto by improving serializer
            else if (typeof(T) == typeof(AbstractBindingDataDto) || typeof(T) == typeof(AbstractSimpleBindingDataDto))
            {
                readable &= UMI3DSerializer.TryRead(container, out int multibindingSerializerIndex); // not used because can only have node bindig, but must be read

                switch (multibindingSerializerIndex)
                {
                    case BindingSerializationIndices.MULTI_BINDING_INDEX:
                        {
                            readable &= GetSerializer<MultiBindingDataDto>().Read(container, out MultiBindingDataDto dataDto);
                            result = (T)System.Convert.ChangeType(dataDto, typeof(MultiBindingDataDto));
                            return readable;
                        }
                    case BindingSerializationIndices.NODE_BINDING_INDEX:
                        {
                            readable &= GetSerializer<NodeBindingDataDto>().Read(container, out NodeBindingDataDto dataDto);
                            result = (T)System.Convert.ChangeType(dataDto, typeof(NodeBindingDataDto));
                            return readable;
                        }
                }
            }
            readable = false;
            result = default;
            return readable;
        }

        /// <inheritdoc/>
        public virtual bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            if (bindingSerializers.ContainsKey(typeof(T)))
            {
                bytable = GetSerializer<T>().Write(value);
                return true;
            }
            //specific case of multibinding, to refacto by improving serializer
            else if (typeof(T) == typeof(AbstractBindingDataDto) || typeof(T) == typeof(AbstractSimpleBindingDataDto))
            {
                bytable = value switch
                {
                    MultiBindingDataDto => UMI3DSerializer.Write(BindingSerializationIndices.MULTI_BINDING_INDEX)
                                            + UMI3DSerializer.Write(value as MultiBindingDataDto),
                    NodeBindingDataDto => UMI3DSerializer.Write(BindingSerializationIndices.NODE_BINDING_INDEX)
                                            + UMI3DSerializer.Write(value as NodeBindingDataDto),
                    _ => default
                };

                if (bytable == default)
                {
                    UMI3DLogger.LogError($"Object was AbstractSimpleBindingDataDto within a Multibinding but was not identified.", DebugScope.UserCapture | DebugScope.Common);
                    return false;
                }

                return true;
            }
            else
            {
                bytable = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(BindingDto) => true,
                true when typeof(T) == typeof(AbstractSimpleBindingDataDto) => true,
                true when typeof(T) == typeof(MultiBindingDataDto) => true,
                true when typeof(T) == typeof(NodeBindingDataDto) => true,
                _ => null
            };
        }
    }
}