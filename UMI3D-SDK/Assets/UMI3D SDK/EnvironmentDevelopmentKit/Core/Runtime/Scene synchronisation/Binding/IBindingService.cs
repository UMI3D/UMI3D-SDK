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

namespace umi3d.edk
{
    public interface IBindingService
    {
        /// <summary>
        /// Are bindings activated on a uer client?
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool AreBindingsEnabled(UMI3DUser user);

        /// <summary>
        /// Get current bindings by node for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Dictionary<ulong, AbstractBinding> GetBindings(UMI3DUser user);

        /// <summary>
        /// Add a new Binding.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The associated operation</returns>
        List<Operation> AddBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Set a collection of Bindings.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="bindings">the list of bindings</param>
        /// <returns>The associated SetEntityProperty</returns>
        List<Operation> AddBindingRange(IEnumerable<AbstractBinding> bindings, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Remove all bindings on a node for all users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        List<Operation> RemoveAllBindings(ulong nodeId, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Remove all bindings on a node Binding for a user.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        List<Operation> RemoveAllBindings(ulong nodeId, UMI3DUser user);

        /// <summary>
        /// Remove all bindings on a node for a user et reattach the objet under a new parent.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        /// <param name="newparent">a transform intended to be the new parent</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        List<Operation> RemoveAllBindingsAndReattach(AbstractBinding binding, UMI3DAbstractNode newparent, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Remove a Binding for several users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        List<Operation> RemoveBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Remove a Binding for one users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        List<Operation> RemoveBinding(AbstractBinding binding, UMI3DUser user);

        /// <summary>
        /// Set the activation of Bindings for several users.
        /// </summary>
        /// <param name="activated">the activation value</param>
        /// <returns>The associated operation. Is null if no operation is required.</returns>
        Operation UpdateBindingActivation(bool activated, IEnumerable<UMI3DUser> users = null);

        /// <summary>
        /// Set the activation of Bindings for one user.
        /// </summary>
        /// <param name="activated">the activation value</param>
        /// <returns>The associated operation. Is null if no operation is required.</returns>
        Operation UpdateBindingActivation(bool activated, UMI3DUser user);
    }
}