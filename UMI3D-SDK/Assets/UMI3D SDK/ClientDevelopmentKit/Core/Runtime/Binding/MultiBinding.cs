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

using UnityEngine;

namespace umi3d.cdk
{
    public class MultiBinding : AbstractBinding
    {
        protected AbstractSimpleBinding[] orderedBindings;
        protected bool[] partialFits;

        public MultiBinding(AbstractSimpleBinding[] orderedBindings, bool[] partialFits, Transform boundTransform)
        {
            this.orderedBindings = orderedBindings;
            this.partialFits = partialFits;
            this.boundTransform = boundTransform;
        }

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

                if (i < orderedBindings.Length - 1 && !partialFits[i + 1])
                    break; // continue only if next binding allo to be partially applied
            }

            success = true;
        }
    }
}