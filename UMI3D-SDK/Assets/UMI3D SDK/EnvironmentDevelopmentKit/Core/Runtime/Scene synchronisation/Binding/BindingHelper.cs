/*
Copyright 2019 - 2021 Inetum

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

using inetum.unityUtils;

using System.Collections.Generic;
using System.Linq;

using umi3d.common;

namespace umi3d.edk
{
    public interface IBindingHelper
    {
        /// <summary>
        /// Are bindings activated on a uer client?
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool AreBindingsActivated(UMI3DUser user);

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
        List<Operation> AddBinding(AbstractBinding binding);

        /// <summary>
        /// Set a collection of Bindings.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="bindings">the list of bindings</param>
        /// <returns>The associated SetEntityProperty</returns>
        List<Operation> AddBindingRange(IEnumerable<AbstractBinding> bindings);

        /// <summary>
        /// Remove all bindings on a node for all users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        Operation RemoveAllBindings(ulong nodeId);

        /// <summary>
        /// Remove all bindings on a node for several users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        Operation RemoveAllBindings(ulong nodeId, IEnumerable<UMI3DUser> users);

        /// <summary>
        /// Remove all bindings on a node Binding for a user.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        Operation RemoveAllBindings(ulong nodeId, UMI3DUser user);

        /// <summary>
        /// Remove all bindings on a node for a user et reattach the objet under a new parent.
        /// </summary>
        /// <param name="obj">the avatar node</param>
        /// <param name="binding">the new binding value</param>
        /// <param name="newparent">a transform intended to be the new parent</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        Operation RemoveAllBindingsAndReattach(AbstractBinding binding, UMI3DAbstractNode newparent, IEnumerable<UMI3DUser> users);

        /// <summary>
        /// Remove a Binding for all current users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        List<Operation> RemoveBinding(AbstractBinding binding);

        /// <summary>
        /// Remove a Binding for several users.
        /// </summary>
        /// <param name="binding">the new binding value</param>
        /// <returns>The list of associated SetEntityProperty.</returns>
        List<Operation> RemoveBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users);

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
        Operation UpdateBindingActivation(bool activated, IEnumerable<UMI3DUser> users);

        /// <summary>
        /// Set the activation of Bindings for one user.
        /// </summary>
        /// <param name="activated">the activation value</param>
        /// <returns>The associated operation. Is null if no operation is required.</returns>
        Operation UpdateBindingActivation(bool activated, UMI3DUser user);
    }

    /// <summary>
    ///  optinnal
    /// </summary>
    public class BindingHelper : Singleton<BindingHelper>, IBindingHelper
    {
        #region DI

        private readonly UMI3DServer umi3dServerService;

        public BindingHelper() : base()
        {
            umi3dServerService = UMI3DServer.Instance;
        }

        public BindingHelper(UMI3DServer umi3dServerService) : base()
        {
            this.umi3dServerService = UMI3DServer.Instance;
        }

        #endregion DI

        #region BindingServerCache

        protected Dictionary<UMI3DUser, Dictionary<ulong, AbstractBinding>> Bindings = new();
        protected Dictionary<UMI3DUser, bool> AreBindingsActivatedDict = new();

        public bool AreBindingsActivated(UMI3DUser user)
        {
            if (!Bindings.ContainsKey(user))
                return umi3dServerService.UserSet().Contains(user);
            return AreBindingsActivatedDict[user];
        }

        public Dictionary<ulong, AbstractBinding> GetBindings(UMI3DUser user)
        {
            if (!Bindings.ContainsKey(user))
                return new Dictionary<ulong, AbstractBinding>();
            return Bindings[user];
        }

        #endregion BindingServerCache

        #region UpdateActivation

        public Operation UpdateBindingActivation(bool activated, UMI3DUser user)
        {
            return UpdateBindingActivation(activated, new UMI3DUser[] { user });
        }

        public Operation UpdateBindingActivation(bool activated, IEnumerable<UMI3DUser> users)
        {
            HashSet<UMI3DUser> targetUsers = new();
            foreach (UMI3DUser user in users)
            {
                if (!AreBindingsActivatedDict.ContainsKey(user))
                    AreBindingsActivatedDict.Add(user, activated);
                else if (AreBindingsActivatedDict[user] == activated)
                    continue;

                AreBindingsActivatedDict[user] = activated;
                targetUsers.Add(user);
            }
            return new UpdateBindingsActivation(activated) { users = targetUsers };
        }

        #endregion UpdateActivation

        #region AddBinding

        public List<Operation> AddBinding(AbstractBinding binding)
        {
            if (binding.users is null || binding.users.Count == 0) // prevent api users to forget users
                binding.users = umi3dServerService.UserSet();

            foreach (UMI3DUser user in binding.users)
            {
                if (!Bindings.ContainsKey(user))
                    Bindings.Add(user, new());
            }

            return AddOrUpgradeBinding(binding);
        }

        public List<Operation> AddBindingRange(IEnumerable<AbstractBinding> bindings)
        {
            var allUsers = bindings.SelectMany(b => b.users);

            if (allUsers is null || allUsers.Count() == 0) // prevent api users to forget users
                allUsers = umi3dServerService.UserSet();

            foreach (UMI3DUser user in allUsers)
            {
                if (!Bindings.ContainsKey(user))
                    Bindings.Add(user, new());
            }

            List<Operation> operations = new();
            foreach (AbstractBinding binding in bindings)
            {
                operations.AddRange(AddOrUpgradeBinding(binding));
            }

            return operations;
        }

        /// <summary>
        /// Add a new binding or upgrade the existing binding to a multi-binding.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        private List<Operation> AddOrUpgradeBinding(AbstractBinding binding)
        {
            List<Operation> operations = new();

            List<UMI3DUser> usersToRemoveFromOriginalBinding = new();
            foreach (UMI3DUser user in binding.users)
            {
                if (!Bindings[user].ContainsKey(binding.boundNodeId))
                {
                    Bindings[user][binding.boundNodeId] = binding;
                }
                else
                {
                    UMI3DLogger.LogWarning($"Overwritting binding on node {binding.boundNodeId} " +
                                                 $"for user {user.Id()}." +
                                                 $"{(Bindings[user][binding.boundNodeId] is not MultiBinding ? "Upgraded to multibinding." : "")}", DebugScope.EDK | DebugScope.Core);

                    usersToRemoveFromOriginalBinding.Add(user); // user in this list will receive their own binding
                    Bindings[user][binding.boundNodeId].users.Remove(user); // user will no longer use previous binding
                    var multiBindingResult = new MultiBinding(binding.boundNodeId) { users = new() { user } };

                    var newBinding = (AbstractBinding)binding.Clone(); // ensure that the multibinding will use clean binding
                    newBinding.users = new() { user };

                    var oldBinding = (AbstractBinding)Bindings[user][binding.boundNodeId].Clone(); // ensure that the multibinding will use clean binding
                    oldBinding.users = new() { user };

                    if (oldBinding is AbstractSingleBinding oldSingleBinding)
                    {
                        multiBindingResult.bindings.Add(oldSingleBinding); // old binding

                        if (newBinding is AbstractSingleBinding newSingleBinding)
                            multiBindingResult.bindings.Add(newSingleBinding); // new binding
                        else if (newBinding is MultiBinding newMultiBinding)
                            multiBindingResult.bindings.AddRange(newMultiBinding.bindings);
                    }
                    else if (oldBinding is MultiBinding oldMultiBinding)
                    {
                        foreach (var singleBindingInOldMulti in oldMultiBinding.bindings)
                        {
                            singleBindingInOldMulti.users = new HashSet<UMI3DUser>() { user };
                            multiBindingResult.bindings.Add(singleBindingInOldMulti);
                        }

                        if (newBinding is AbstractSingleBinding newSingleBinding)
                            multiBindingResult.bindings.Add(newSingleBinding);
                        else if (newBinding is MultiBinding newMultiBinding)
                            multiBindingResult.bindings.AddRange(newMultiBinding.bindings);
                    }

                    Bindings[user][binding.boundNodeId] = multiBindingResult;
                    operations.Add(multiBindingResult); // own binding for a user
                }
            }

            foreach (var user in usersToRemoveFromOriginalBinding) //removed users receive their own binding
                binding.users.Remove(user);

            operations.Add(binding);
            return operations;
        }

        #endregion AddBinding

        #region RemoveBinding

        public List<Operation> RemoveBinding(AbstractBinding binding)
        {
            return RemoveBinding(binding, binding.users);
        }

        public List<Operation> RemoveBinding(AbstractBinding binding, UMI3DUser user)
        {
            return RemoveBinding(binding, new UMI3DUser[] { user });
        }

        public List<Operation> RemoveBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users)
        {
            List<Operation> operations = new();
            foreach (UMI3DUser user in users)
            {
                if (!Bindings.ContainsKey(user) || !Bindings[user].ContainsKey(binding.boundNodeId))
                    continue;

                if (Bindings[user][binding.boundNodeId] is MultiBinding multiBinding && binding is AbstractSingleBinding singleBinding)
                {
                    multiBinding.bindings.Remove(singleBinding);

                    if (multiBinding.bindings.Count == 1) //down binding to normal binding
                        Bindings[user][binding.boundNodeId] = multiBinding.bindings[0];

                    operations.Add(Bindings[user][binding.boundNodeId]);
                }
                else
                {
                    operations.Add(new RemoveBinding() { users = new HashSet<UMI3DUser> { user }, boundNodeId = binding.boundNodeId });
                    Bindings[user].Remove(binding.boundNodeId);
                }
            }
            return operations;
        }

        public Operation RemoveAllBindings(ulong nodeId)
        {
            var users = Bindings
                            .Where(x => x.Value.ContainsKey(nodeId))
                            .Select(x => x.Key);
            return RemoveAllBindings(nodeId, users);
        }

        public Operation RemoveAllBindings(ulong nodeId, UMI3DUser user)
        {
            return RemoveAllBindings(nodeId, new UMI3DUser[] { user });
        }

        public Operation RemoveAllBindings(ulong nodeId, IEnumerable<UMI3DUser> users)
        {
            HashSet<UMI3DUser> targetUsers = new(users);
            foreach (UMI3DUser user in users)
            {
                if (!Bindings.ContainsKey(user) || !Bindings[user].ContainsKey(nodeId))
                {
                    targetUsers.Remove(user);
                    continue;
                }

                Bindings[user].Remove(nodeId);
            }

            return new RemoveBinding() { users = targetUsers, boundNodeId = nodeId };
        }

        public Operation RemoveAllBindingsAndReattach(AbstractBinding binding, UMI3DAbstractNode newparent, IEnumerable<UMI3DUser> users)
        {
            Operation operation = RemoveAllBindings(binding.boundNodeId, users);

            var node = UMI3DEnvironment.Instance._GetEntityInstance<UMI3DAbstractNode>(binding.boundNodeId);
            node.transform.SetParent(newparent.transform, true);

            foreach (UMI3DUser user in operation.users)
            {
                operation += node.objectParentId.SetValue(user, newparent.GetComponent<UMI3DAbstractNode>())
                             + node.objectPosition.SetValue(user, node.transform.localPosition);
            }

            return operation;
        }

        #endregion RemoveBinding
    }
}