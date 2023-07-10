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

using umi3d.common.binding;
using umi3d.common.dto.binding;

namespace umi3d.common.userCapture.binding
{
    /// <summary>
    /// Serializer for <see cref="BoneBindingDataDto"/> and <see cref="RigBoneBindingDataDto"/>.
    /// Also redefine <see cref="MultiBindingDataDto"/> serialization that use that latter DTOs.
    /// </summary>
    public class UserCaptureBindingSerializerModule : BindingSerializerModule
    {
        public UserCaptureBindingSerializerModule() : base()
        {
            bindingSerializers.Add(typeof(BoneBindingDataDto), new BoneBindingSerializer());
            bindingSerializers.Add(typeof(RigBoneBindingDataDto), new RigBoneBindingSerializer());
        }

        /// <summary>
        /// Indices used to resolve the inheritance when serializing/deserializing.
        /// </summary>
        protected static class UserCaptureBindingSerializationIndices
        {
            public const int BONE_BINDING_INDEX = 2;
            public const int RIGGED_BONE_BINDING_INDEX = 3;
        }

        /// <inheritdoc/>
        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
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
                readable &= UMI3DSerializer.TryRead(container, out int multibindingSerializerIndex);

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
                    case UserCaptureBindingSerializationIndices.BONE_BINDING_INDEX:
                        {
                            readable &= GetSerializer<BoneBindingDataDto>().Read(container, out BoneBindingDataDto dataDto);
                            result = (T)System.Convert.ChangeType(dataDto, typeof(BoneBindingDataDto));
                            return readable;
                        }
                    case UserCaptureBindingSerializationIndices.RIGGED_BONE_BINDING_INDEX:
                        {
                            readable &= GetSerializer<RigBoneBindingDataDto>().Read(container, out RigBoneBindingDataDto dataDto);
                            result = (T)System.Convert.ChangeType(dataDto, typeof(RigBoneBindingDataDto));
                            return readable;
                        }
                    default:
                        result = default;
                        return readable;
                }
            }
            readable = false;
            result = default;
            return readable;
        }

        /// <inheritdoc/>
        public override bool Write<T>(T value, out Bytable bytable, params object[] parameters)
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
                    RigBoneBindingDataDto => UMI3DSerializer.Write(UserCaptureBindingSerializationIndices.RIGGED_BONE_BINDING_INDEX)
                                            + UMI3DSerializer.Write(value as RigBoneBindingDataDto),
                    BoneBindingDataDto => UMI3DSerializer.Write(UserCaptureBindingSerializationIndices.BONE_BINDING_INDEX)
                                            + UMI3DSerializer.Write(value as BoneBindingDataDto),
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
    }
}