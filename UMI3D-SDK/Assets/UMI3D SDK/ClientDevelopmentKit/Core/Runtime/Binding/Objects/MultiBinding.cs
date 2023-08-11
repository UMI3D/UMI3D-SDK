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
using umi3d.common.dto.binding;
using UnityEngine;

namespace umi3d.cdk.binding
{
    /// <summary>
    /// Client support for multi-binding, several bindings on same bound node.
    /// </summary>
    public class MultiBinding : AbstractBinding
    {
        /// <summary>
        /// Cache of bindings on the node ordered by descending priority.
        /// </summary>
        protected AbstractSimpleBinding[] orderedBindings;

        public List<AbstractSimpleBinding> Bindings => orderedBindings.ToList();

        public MultiBinding(MultiBindingDataDto data, AbstractSimpleBinding[] bindings, Transform boundTransform, bool isOrdered = false) : base(boundTransform, data)
        {
            if (isOrdered)
            {
                this.orderedBindings = bindings;
            }
            else
            {
                this.orderedBindings = bindings.Where(x => x is not null)
                                               .OrderByDescending(x => x.Priority)
                                               .ToArray();
            }
        }

        /// <inheritdoc/>
        public override void Apply(out bool success)
        {
            if (boundTransform is null) // node is destroyed
            {
                success = false;
                return;
            }

            for (int i = 0; i < orderedBindings.Length; i++)
            {
                orderedBindings[i].Apply(out success);
                if (!success)
                    break;

                if (!orderedBindings[i].IsPartiallyFit)
                    break;

                if (i < orderedBindings.Length - 1 && !orderedBindings[i + 1].IsPartiallyFit)
                    break; // continue only if next binding allo to be partially applied
            }

            success = true;
        }
    }
}