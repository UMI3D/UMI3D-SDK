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

namespace umi3d.cdk.binding
{
    /// <summary>
    /// Handle binding management and computations.
    /// </summary>
    public interface IBindingBrowserService
    {
        /// <summary>
        /// Are bindings computed for this client?
        /// </summary>
        public bool AreBindingsActivated { get; }

        /// <summary>
        /// Currently computed bindings per UMI3D node id.
        /// </summary>
        public IReadOnlyDictionary<ulong, AbstractBinding> Bindings { get; }

        /// <summary>
        /// Add a binding that already has been loaded.
        /// </summary>
        /// <param name="boundNodeId">Id of the node that will apply the binding on itself.</param>
        /// <param name="binding">Binding to add</param>
        public void AddBinding(ulong boundNodeId, AbstractBinding binding);

        /// <summary>
        /// Remove a binding to compute.
        /// </summary>
        /// <param name="boundNodeid">Id of the node that is bound.</param>
        public void RemoveBinding(ulong boundNodeid);

        /// <summary>
        /// Enable/disable bindings computation for this client.
        /// </summary>
        /// <param name="isEnabled">If true, bindings are allowed to be computed on browser.</param>
        public void UpdateBindingsActivation(bool isEnabled);
    }
}