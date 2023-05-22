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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Operation binding a node and several objects.
    /// </summary>
    public class MultiBinding : AbstractBinding
    {
        /// <summary>
        /// Collection of bindings to the same node.
        /// </summary>
        /// MultiBinding of MultiBinding is not fully supported.
        public List<AbstractSingleBinding> bindings = new();

        public MultiBinding(ulong boundNodeId) : base(boundNodeId)
        {
        }

        public override BindingDto ToDto()
        {
            MultiBindingDataDto bindingDataDto;

            bindingDataDto = new MultiBindingDataDto() {
                Bindings= bindings.Select(x => x.ToDto().data as AbstractSimpleBindingDataDto).ToArray(),
                priority= priority,
                partialFit= partialFit
            };

            var bindingDto = new BindingDto()
            {
                boundNodeId = boundNodeId,
                data = bindingDataDto
            };

            return bindingDto;
        }

        public override object Clone()
        {
            var binding = (MultiBinding)base.Clone();
            binding.bindings = new List<AbstractSingleBinding>();
            foreach (var bindingInMulti in this.bindings)
            {
                binding.bindings.Add((AbstractSingleBinding)bindingInMulti.Clone());
            }
            return binding;
        }
    }
}