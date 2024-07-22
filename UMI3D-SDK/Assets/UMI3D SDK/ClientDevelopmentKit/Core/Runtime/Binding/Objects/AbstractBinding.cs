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

using umi3d.common.dto.binding;
using UnityEngine;

namespace umi3d.cdk.binding
{
    /// <summary>
    /// Abstract support for client bindings
    /// </summary>
    public abstract class AbstractBinding
    {
        /// <summary>
        /// Transform that is moved by the binding.
        /// </summary>
        protected Transform boundTransform;

        /// <summary>
        /// Binding data.
        /// </summary>
        protected AbstractBindingDataDto data;

        #region DTO Access

        /// <summary>
        /// See <see cref="AbstractBindingDataDto.partialFit"/>.
        /// </summary>
        public virtual bool IsPartiallyFit { get; }

        /// <summary>
        /// See <see cref="AbstractBindingDataDto.priority"/>.
        /// </summary>
        public virtual int Priority { get; }

        /// <summary>
        /// See <see cref="AbstractBindingDataDto.resetWhenRemoved"/>.
        /// </summary>
        public virtual bool ResetWhenRemoved { get; }

        /// <summary>
        /// Transform of the bound node.
        /// </summary>
        public virtual Transform BoundTransform => boundTransform;  

        #endregion DTO Access

        protected AbstractBinding(Transform boundTransform, AbstractBindingDataDto data)
        {
            this.boundTransform = boundTransform;
            this.data = data;
            this.IsPartiallyFit = data.partialFit;
            this.Priority = data.priority;
            this.Priority = data.priority;
            this.ResetWhenRemoved = data.resetWhenRemoved;
        }

        /// <summary>
        /// Apply the computations of the binding.
        /// </summary>
        /// <param name="success"></param>
        public abstract void Apply(out bool success);

        /// <summary>
        /// Put back the bound object at its original world place.
        /// </summary>
        public abstract void Reset();
    }
}