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
    /// <summary>
    /// Manage bindings from envrionment side
    /// </summary>
    public class BindingManager : Singleton<BindingManager>, IBindingService
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.EDK | DebugScope.Core;

        /// <summary>
        /// Bindings, indexed by id of the bound node.
        /// </summary>
        public readonly UMI3DAsyncDictionnaryProperty<ulong, AbstractBinding> bindings = new(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.Bindings, new());

        /// <summary>
        /// Are bindings computations enabled on the browser of the user?
        /// </summary>
        public readonly UMI3DAsyncProperty<bool> areBindingsEnabled = new(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.ActiveBindings, true);

        #region DI

        private readonly UMI3DServer umi3dServerService;
        //private readonly UMI3DEnvironment umi3dEnvironmentService;

        public BindingManager() : base()
        {
            umi3dServerService = UMI3DServer.Instance;
            Init();
        }

        public BindingManager(UMI3DServer umi3dServerService) : base()
        {
            this.umi3dServerService = umi3dServerService;
            Init();
        }

        #endregion DI

        protected void Init()
        {
            umi3dServerService.OnUserJoin.AddListener(DispatchBindings);
        }

        protected virtual void DispatchBindings(UMI3DUser user)
        {
            if (bindings.GetValue().Count > 0)
            {
                Transaction t = new() { reliable = true };
                foreach (var (_, binding) in bindings.GetValue())
                    t.AddIfNotNull(binding.GetLoadEntity());
                t.Dispatch();
            }
        }

        public virtual bool AreBindingsEnabled(UMI3DUser user = null)
        {
            return areBindingsEnabled.GetValue(user);
        }

        public virtual Dictionary<ulong, AbstractBinding> GetBindings(UMI3DUser user = null)
        {
            return bindings.GetValue(user);
        }

        #region UpdateActivation

        /// <inheritdoc/>
        public virtual Operation SetBindingsActivation(bool activated, UMI3DUser user)
        {
            return areBindingsEnabled.SetValue(user, activated);
        }

        /// <inheritdoc/>
        public virtual Operation SetBindingsActivation(bool activated, IEnumerable<UMI3DUser> users = null)
        {
            if (users is null)
                return areBindingsEnabled.SetValue(activated);

            List<Operation> ops = new();
            foreach (var user in users)
                ops.Add(areBindingsEnabled.SetValue(user, activated));
            return ops.Aggregate((x, y) => x + y);
        }

        #endregion UpdateActivation

        #region AddBinding

        /// <inheritdoc/>
        public virtual List<Operation> AddBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null)
        {
            return AddOrUpgradeBinding(binding, users);
        }

        /// <inheritdoc/>
        public virtual List<Operation> AddBindingRange(IEnumerable<AbstractBinding> bindings, IEnumerable<UMI3DUser> users = null)
        {
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
        private List<Operation> AddOrUpgradeBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null)
        {
            if (binding is null)
            {
                UMI3DLogger.LogWarning($"Impossible to add binding. Binding is null.", DEBUG_SCOPE);
                return null;
            }

            List<Operation> operations = new();

            if (!bindings.isAsync) // all users have same value
            {
                HashSet<UMI3DUser> targetUsers = users is not null ? new(users) : null;

                if (bindings.GetValue().ContainsKey(binding.boundNodeId))
                {
                    var existingBinding = bindings.GetValue(binding.boundNodeId);

                    if (existingBinding.Id() == binding.Id()) // same binding is already applied
                    {
                        UMI3DLogger.LogWarning($"Binding (id:{binding.Id()}, node:{binding.boundNodeId}) is already applied for all users.", DebugScope.EDK | DebugScope.Core);
                        return null;
                    }

                    operations.Add(existingBinding.GetDeleteEntity(targetUsers));

                    var multiBindingResult = UpgradeBinding(existingBinding, binding);

                    if (users is not null)
                    {
                        foreach (UMI3DUser user in targetUsers)
                            bindings.SetValue(user, binding.boundNodeId, multiBindingResult);
                    }
                    else
                    {
                        bindings.SetValue(binding.boundNodeId, multiBindingResult);
                    }
                    operations.Add(multiBindingResult.GetLoadEntity(targetUsers)); // own binding for a user
                }
                else
                {
                    bindings.Add(binding.boundNodeId, binding);
                    operations.Add(binding.GetLoadEntity(targetUsers));
                }
            }
            else // some users have different values
            {
                var targetUsers = users is not null ? users : umi3dServerService.Users();

                var usersWithValues = users.Where((u) => bindings.GetValue(u).ContainsKey(binding.boundNodeId)).ToList();

                usersWithValues
                    .GroupBy(u => bindings.GetValue(binding.boundNodeId, u))
                    .ForEach(userGroup =>
                    {
                        HashSet<UMI3DUser> targetUsers = userGroup.ToHashSet();
                        var existingBinding = userGroup.Key;

                        if (existingBinding == binding) // same binding is already applied
                        {
                            UMI3DLogger.LogWarning($"Binding (id:{binding.Id()}, node:{binding.boundNodeId}) is already applied for set of users.", DebugScope.EDK | DebugScope.Core);
                            return;
                        }

                        operations.Add(existingBinding.GetDeleteEntity(targetUsers));

                        var multiBindingResult = UpgradeBinding(existingBinding, binding);

                        foreach (UMI3DUser user in userGroup)
                            bindings.SetValue(user, binding.boundNodeId, multiBindingResult);
                        operations.Add(multiBindingResult.GetLoadEntity(targetUsers)); // own binding for a user
                    });

                users.Except(usersWithValues).ForEach(user =>
                {
                    bindings.Add(user, binding.boundNodeId, binding);
                    operations.Add(binding.GetLoadEntity(new() { user }));
                });
            }

            return operations;
        }

        protected virtual MultiBinding UpgradeBinding(AbstractBinding existingBinding, AbstractBinding newBinding)
        {
            var multiBindingResult = new MultiBinding(existingBinding.boundNodeId);

            if (existingBinding is AbstractSingleBinding oldSingleBinding)
            {
                multiBindingResult.bindings.Add(oldSingleBinding); // old binding

                if (newBinding is AbstractSingleBinding newSingleBinding)
                    multiBindingResult.bindings.Add(newSingleBinding); // new binding
                else if (newBinding is MultiBinding newMultiBinding)
                    multiBindingResult.bindings.AddRange(newMultiBinding.bindings);
            }
            else if (existingBinding is MultiBinding oldMultiBinding)
            {
                foreach (var singleBindingInOldMulti in oldMultiBinding.bindings)
                {
                    multiBindingResult.bindings.Add(singleBindingInOldMulti);
                }

                if (newBinding is AbstractSingleBinding newSingleBinding)
                    multiBindingResult.bindings.Add(newSingleBinding);
                else if (newBinding is MultiBinding newMultiBinding)
                    multiBindingResult.bindings.AddRange(newMultiBinding.bindings);
            }
            return multiBindingResult;
        }

        #endregion AddBinding

        #region RemoveBinding

        /// <inheritdoc/>
        public virtual List<Operation> RemoveBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null)
        {
            return RemoveOrDowngradeBinding(binding, users);
        }

        /// <inheritdoc/>
        public virtual List<Operation> RemoveBinding(AbstractBinding binding, UMI3DUser user)
        {
            return RemoveOrDowngradeBinding(binding, new UMI3DUser[] { user });
        }

        /// <inheritdoc/>
        public virtual List<Operation> RemoveOrDowngradeBinding(AbstractBinding bindingToRemove, IEnumerable<UMI3DUser> users = null)
        {
            if (bindingToRemove is null)
            {
                UMI3DLogger.LogWarning($"Impossible to remove binding. Binding is null.", DEBUG_SCOPE);
                return null;
            }

            List<Operation> operations = new();

            if (!bindings.isAsync) // all users have same value
            {
                HashSet<UMI3DUser> targetUsers = users is not null ? new(users) : null;

                if (bindings.GetValue().ContainsKey(bindingToRemove.boundNodeId))
                {
                    var bindingOnNode = bindings.GetValue()[bindingToRemove.boundNodeId];

                    if (bindingOnNode.Id() == bindingToRemove.Id()) // those are the same binding
                    {
                        if (users is null) // target is all users
                        {
                            bindings.Remove(bindingToRemove.boundNodeId);
                        }
                        else // target is a fraction of all the users
                        {
                            foreach (UMI3DUser user in users)
                                bindings.Remove(user, bindingToRemove.boundNodeId); // no binding left on node
                        }
                        operations.Add(bindingToRemove.GetDeleteEntity(targetUsers));
                    }
                    else if (bindingOnNode is MultiBinding existingMultiBinding // the binding is inside a multibinding
                            && bindingToRemove is AbstractSingleBinding singleBindingToRemove
                            && existingMultiBinding.bindings.Contains(singleBindingToRemove))
                    {
                        operations.Add(existingMultiBinding.GetDeleteEntity(targetUsers));

                        var newBinding = DowngradeBinding(existingMultiBinding, singleBindingToRemove);

                        operations.Add(newBinding.GetLoadEntity(targetUsers)); // a binding remains on node
                        if (targetUsers is null) // target is all users
                        {
                            bindings.SetValue(newBinding.boundNodeId, newBinding);
                        }
                        else // target is a fraction of all the users
                        {
                            foreach (UMI3DUser user in users)
                                bindings.SetValue(user, newBinding.boundNodeId, newBinding);
                        }
                    }
                    else
                    {
                        UMI3DLogger.LogWarning($"Cannot remove binding {bindingToRemove.Id()}. Not on node and not in multibinding.", DEBUG_SCOPE);
                    }
                }
                else
                {
                    UMI3DLogger.LogWarning($"Cannot remove binding {bindingToRemove.Id()}. Binding not found.", DEBUG_SCOPE);
                }
            }
            else // users may have different values
            {
                var targetUsers = users is not null ? users : umi3dServerService.Users();

                var usersWithValues = users.Where((u) => bindings.GetValue(u).ContainsKey(bindingToRemove.boundNodeId)).ToList();

                if (usersWithValues.Count > 0)
                {
                    var usersWithSameBinding = usersWithValues.Where((u) => bindings.GetValue(u)[bindingToRemove.boundNodeId].Id() == bindingToRemove.Id());

                    if (usersWithSameBinding.Count() > 0)
                    {
                        foreach (UMI3DUser user in usersWithSameBinding)
                            bindings.Remove(user, bindingToRemove.boundNodeId); // no binding left on node
                        operations.Add(bindingToRemove.GetDeleteEntity(usersWithSameBinding.ToHashSet()));
                    }

                    if (bindingToRemove is AbstractSingleBinding singleBindingToRemove)
                    {
                        usersWithValues
                                   .Except(usersWithSameBinding)
                                   .GroupBy((u) => bindings.GetValue(u)[bindingToRemove.boundNodeId] as MultiBinding)
                                   .Where(x => x.Key is not null)
                                   .ForEach((userGroup) =>
                                   {
                                       var existingMultibinding = userGroup.Key;

                                       operations.Add(bindingToRemove.GetDeleteEntity(userGroup.ToHashSet()));
                                       operations.Add(existingMultibinding.GetDeleteEntity(userGroup.ToHashSet()));

                                       var newBinding = DowngradeBinding(existingMultibinding, singleBindingToRemove);

                                       operations.Add(newBinding.GetLoadEntity(userGroup.ToHashSet())); // a binding remains on node

                                       foreach (UMI3DUser user in userGroup)
                                           bindings.SetValue(user, newBinding.boundNodeId, newBinding); // no binding left on node
                                   });
                    }
                }
                else
                {
                    UMI3DLogger.LogWarning($"Cannot remove binding {bindingToRemove.Id()}. Not on node and not in multibinding for any user.", DEBUG_SCOPE);
                }
            }

            return operations;
        }

        /// <summary>
        /// Create new binding from existing binding when removing one of the contained bindings.
        /// </summary>
        /// <param name="existingMultibinding"></param>
        /// <param name="singleBindingToRemove"></param>
        /// <returns></returns>
        private AbstractBinding DowngradeBinding(MultiBinding existingMultibinding, AbstractSingleBinding singleBindingToRemove)
        {
            AbstractBinding newBinding;

            existingMultibinding.bindings.Remove(singleBindingToRemove);

            if (existingMultibinding.bindings.Count > 1)
            {
                newBinding = new MultiBinding(existingMultibinding.boundNodeId)
                {
                    priority = existingMultibinding.priority,
                    partialFit = existingMultibinding.partialFit,
                    bindings = existingMultibinding.bindings
                };
            }
            else
            {
                newBinding = existingMultibinding.bindings[0];
            }

            return newBinding;
        }

        /// <inheritdoc/>
        public virtual List<Operation> RemoveAllBindings(ulong nodeId, IEnumerable<UMI3DUser> users = null)
        {
            var operations = new List<Operation>();
            if (!bindings.isAsync) // all users have same value
            {
                HashSet<UMI3DUser> targetUsers = users is not null ? new(users) : null;

                if (!bindings.GetValue().ContainsKey(nodeId))
                {
                    UMI3DLogger.LogWarning($"Cannot remove bindings on node {nodeId}. Not on node and not in multibinding for any user.", DEBUG_SCOPE);
                    return null;
                }

                var bindingToDelete = bindings.GetValue()[nodeId];

                operations.Add(bindingToDelete.GetDeleteEntity(targetUsers));
            }
            else // some users have specific values
            {
                users
                    .Where((u) => bindings.GetValue(u).ContainsKey(nodeId))
                    .GroupBy(u => bindings.GetValue(u)[nodeId])
                    .ForEach(userGroup =>
                    {
                        var bindingToDelete = userGroup.Key;
                        operations.Add(bindingToDelete.GetDeleteEntity(userGroup.ToHashSet()));
                    });
            }

            if (users is not null)
                foreach (var user in users)
                    bindings.Remove(user, nodeId);
            else
                bindings.Remove(nodeId);

            return operations;
        }

        /// <inheritdoc/>
        public virtual List<Operation> RemoveAllBindings(ulong nodeId, UMI3DUser user)
        {
            return RemoveAllBindings(nodeId, new UMI3DUser[] { user });
        }

        /// <inheritdoc/>
        public virtual List<Operation> RemoveAllBindingsAndReattach(AbstractBinding binding, UMI3DAbstractNode newparent, IEnumerable<UMI3DUser> users = null)
        {
            List<Operation> operations = RemoveAllBindings(binding.boundNodeId, users);

            var node = UMI3DEnvironment.Instance._GetEntityInstance<UMI3DAbstractNode>(binding.boundNodeId);
            node.transform.SetParent(newparent.transform, true);

            if (!bindings.isAsync)
            {
                if (users is not null)
                    foreach (var user in users)
                        operations.Add(node.objectParentId.SetValue(user, newparent));
                else
                    operations.Add(node.objectParentId.SetValue(newparent));
            }
            else
            {
                var targetUsers = users is not null ? users : umi3dServerService.Users();
                foreach (UMI3DUser user in targetUsers)
                    operations.Add(node.objectParentId.SetValue(user, newparent));
            }
            return operations;
        }

        #endregion RemoveBinding
    }
}