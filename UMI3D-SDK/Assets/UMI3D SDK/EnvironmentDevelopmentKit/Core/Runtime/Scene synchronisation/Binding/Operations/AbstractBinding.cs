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

using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Operation binding a node and one or several objects.
    /// </summary>
    public abstract class AbstractBinding : Operation
    {
        /// <summary>
        /// Node that is bound to another object.
        /// </summary>
        public ulong boundNodeId;

        /// <summary>
        /// Could the binding allow to be partially applied?
        /// </summary>
        public bool partialFit = true;

        /// <summary>
        /// Priority to apply the binding.
        /// </summary>
        public int priority = 0;

        /// <summary>
        /// Export the object as a DTO for serialization.
        /// </summary>
        /// <returns></returns>
        public abstract BindingDto ToDto();

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.AddBinding)
                    + UMI3DSerializer.Write(ToDto());
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return ToDto();
        }
    }
}