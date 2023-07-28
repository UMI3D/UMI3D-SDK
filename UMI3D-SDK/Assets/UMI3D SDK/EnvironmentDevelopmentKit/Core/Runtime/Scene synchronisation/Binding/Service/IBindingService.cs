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

namespace umi3d.edk.binding
{
    /// <summary>
    /// Manage bindings from envrionment side.
    /// </summary>
    public interface IBindingService
    {
        /// <summary>
        /// Are bindings activated on a uer client?
        /// </summary>
        /// <param name="user">User to get the property value. Null to get synchronized one.</param>
        /// <returns>True if bindings are enabled i.e. computed on browsers.</returns>
        bool AreBindingsEnabled(UMI3DUser user = null);

        /// <summary>
        /// Get current bindings by node for a user.
        /// </summary>
        /// <param name="user">User to get the property value. Null to get synchronized one.</param>
        /// <returns>Current bindings indexed by node.</returns>
        IDictionary<ulong, AbstractBinding> GetBindings(UMI3DUser user = null);

        /// <summary>
        /// Add a new Binding. Don't specify users to target all users.
        /// </summary>
        /// <param name="binding">The new binding to add.</param>
        /// <param name="users">Users that should receive the binding. Don't specify users to target all users.</param>
        /// <returns>The associated operations.</returns>
        IReadOnlyList<Operation> AddBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Set a collection of Bindings. Don't specify users to target all users.
        /// </summary>
        /// <param name="bindings">The new bindings to add.</param>
        /// <param name="users">Users that should receive the binding. Don't specify users to target all users.</param>
        /// <returns>The associated operations.</returns>
        IReadOnlyList<Operation> AddBindingRange(IEnumerable<AbstractBinding> bindings, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Remove all bindings on a node for all users. Don't specify users to target all users.
        /// </summary>
        /// <param name="nodeId">Id of the node from which bindings should be removed.</param>
        /// <param name="users">Users that should see the bindings removed. Don't specify users to target all users.</param>
        /// <returns>The associated operations.</returns>
        IReadOnlyList<Operation> RemoveAllBindings(ulong nodeId, IEnumerable<UMI3DUser> users = null, bool syncServerTransform = true);

        /// <summary>
        /// Remove all bindings on a node Binding for a user.
        /// </summary>
        /// <param name="nodeId">Id of the node from which bindings should be removed.</param>
        /// <param name="user">User that should see the bindings removed.</param>
        /// <returns>The associated operations.</returns>
        IReadOnlyList<Operation> RemoveAllBindings(ulong nodeId, UMI3DUser user, bool syncServerTransform = true);

        /// <summary>
        /// Remove all bindings on a node for a user et reattach the objet under a new parent. Don't specify users to target all users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <param name="newparent">a transform intended to be the new parent</param>
        /// <param name="users">Users that should see the bindings removed. Don't specify users to target all users.</param>
        /// <returns>The associated operations.</returns>
        IReadOnlyList<Operation> RemoveAllBindingsAndReattach(AbstractBinding binding, UMI3DAbstractNode newparent, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Remove a Binding for several users. Don't specify users to target all users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <param name="users">Users that should see the binding removed. Don't specify users to target all users.</param>
        /// <returns>The associated operations.</returns>
        IReadOnlyList<Operation> RemoveBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null, bool syncServerTransform = true);

        /// <summary>
        /// Remove a Binding for one users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <param name="users">User that should see the binding removed.</param>
        /// <returns>The associated operations.</returns>
        IReadOnlyList<Operation> RemoveBinding(AbstractBinding binding, UMI3DUser user, bool syncServerTransform = true);

        /// <summary>
        /// Set the activation of Bindings for several users. Don't specify users to target all users.
        /// </summary>
        /// <param name="activated">If true, bindings are computed on the browser.</param>
        /// <param name="users">User to set the property value. Null to get synchronized one.</param>
        /// <returns>The associated operation. Is null if no operation is required.</returns>
        Operation SetBindingsActivation(bool activated, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Set the activation of Bindings for one user.
        /// </summary>
        /// <param name="activated">If true, bindings are computed on the browser.</param>
        /// <param name="user">User to set the property value. </param>
        /// <returns>The associated operation. Is null if no operation is required.</returns>
        Operation SetBindingsActivation(bool activated, UMI3DUser user);
    }
}