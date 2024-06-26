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

namespace umi3d.edk.binding
{
    /// <summary>
    /// Manage bindings from envrionment side.
    /// </summary>
    public class BindingManager : Singleton<BindingManager>, IBindingService
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.EDK | DebugScope.Core;

        /// <summary>
        /// Bindings, indexed by id of the bound node.
        /// </summary>
        public readonly UMI3DAsyncDictionnaryProperty<ulong, AbstractBinding> bindings = new(UMI3DGlobalID.EnvironmentId, UMI3DPropertyKeys.Bindings, new());

        /// <summary>
        /// Are bindings computations enabled on the browser of the user?
        /// </summary>
        public readonly UMI3DAsyncProperty<bool> areBindingsEnabled = new(UMI3DGlobalID.EnvironmentId, UMI3DPropertyKeys.ActiveBindings, true);

        #region DI

        private readonly IUMI3DServer umi3dServerService;
        private readonly IUMI3DEnvironmentManager umi3dEnvironmentService;

        /// <summary>
        /// Set of users that already got the binding entities on the scene
        /// </summary>
        private HashSet<UMI3DUser> usersWithBindingsReceived = new();

        public BindingManager() : base()
        {
            umi3dServerService = UMI3DServer.Instance;
            umi3dEnvironmentService = UMI3DEnvironment.Instance;
            Init();
        }

        public BindingManager(IUMI3DServer umi3dServerService) : base()
        {
            this.umi3dServerService = umi3dServerService;
            this.umi3dEnvironmentService = UMI3DEnvironment.Instance;
            Init();
        }

        #endregion DI

        #region Initialization

        protected void Init()
        {
            umi3dServerService.OnUserActive.AddListener(DispatchBindings);
            umi3dServerService.OnUserRefreshed.AddListener(ReDispatchBindings);
            umi3dServerService.OnUserMissing.AddListener(CleanBindings);
            umi3dServerService.OnUserLeave.AddListener(CleanBindings);
        }


        protected virtual void ReDispatchBindings(UMI3DUser user)
        {
            if (usersWithBindingsReceived.Contains(user))
            {
                usersWithBindingsReceived.Remove(user);
                DispatchBindings(user);
            }
        }

        /// <summary>
        /// Send all synchronized bindings to a user.
        /// </summary>
        /// <param name="user"></param>
        protected virtual void DispatchBindings(UMI3DUser user)
        {
            if (usersWithBindingsReceived.Contains(user))
                return;

            if (bindings.GetValue().Count > 0)
            {
                Transaction t = new() { reliable = true };
                foreach (var (_, binding) in bindings.GetValue())
                {
                    var g = umi3dEnvironmentService._GetEntityInstance<UMI3DNode>(binding.boundNodeId);

                    if (g != null)
                        t.AddIfNotNull(binding.GetLoadEntity(new() { user }));
                }
                t.Dispatch();
            }

            usersWithBindingsReceived.Add(user);
        }

        protected virtual void CleanBindings(UMI3DUser user)
        {
            if (!usersWithBindingsReceived.Contains(user))
                return;

            usersWithBindingsReceived.Remove(user);
        }

        #endregion Initialization

        /// <inheritdoc/>
        public virtual bool AreBindingsEnabled(UMI3DUser user = null)
        {
            return areBindingsEnabled.GetValue(user);
        }

        /// <inheritdoc/>
        public virtual IDictionary<ulong, AbstractBinding> GetBindings(UMI3DUser user = null)
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
        public virtual IReadOnlyList<Operation> AddBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null)
        {
            return AddOrUpgradeBinding(binding, users);
        }

        /// <inheritdoc/>
        public virtual IReadOnlyList<Operation> AddBindingRange(IEnumerable<AbstractBinding> bindings, IEnumerable<UMI3DUser> users = null)
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
        private IReadOnlyList<Operation> AddOrUpgradeBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null)
        {
            if (binding is null)
            {
                UMI3DLogger.LogWarning($"Impossible to add binding. Binding is null.", DEBUG_SCOPE);
                return null;
            }

            List<Operation> operations = new();

            if (users == null) // all users have same value
            {
                if (bindings.GetValue().ContainsKey(binding.boundNodeId)) //if a binding is already added on that node, upgrade binding
                {
                    var existingBinding = bindings.GetValue(binding.boundNodeId);

                    // check if same binding is already applied as it is not allowed
                    if (existingBinding.Id() == binding.Id())
                    {
                        UMI3DLogger.LogWarning($"Binding (id:{binding.Id()}, node:{binding.boundNodeId}) is already applied for all users.", DebugScope.EDK | DebugScope.Core);
                        return null;
                    }

                    // replace existing binding by a new upgraded one
                    operations.Add(existingBinding.GetDeleteEntity());

                    var multiBindingResult = UpgradeBinding(existingBinding, binding);
                    bindings.SetValue(binding.boundNodeId, multiBindingResult);
                    operations.Add(multiBindingResult.GetLoadEntity());
                }
                else //users should receive the added binding
                {
                    bindings.Add(binding.boundNodeId, binding);
                    operations.Add(binding.GetLoadEntity());
                }
            }
            else // some users have different values
            {
                var usersWithValues = users.Where((u) => bindings.GetValue(u).ContainsKey(binding.boundNodeId)).ToList();

                // for each group of users that has the same binding on the node, update existing binding
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

                // users with no binding receive the added binding
                users.Except(usersWithValues).ForEach(user =>
                {
                    bindings.Add(user, binding.boundNodeId, binding);
                    operations.Add(binding.GetLoadEntity(new() { user }));
                });
            }

            return operations;
        }

        /// <summary>
        /// Merge two <see cref="AbstractBinding"/> into a <see cref="MultiBinding"/>.
        /// </summary>
        /// <param name="existingBinding"></param>
        /// <param name="newBinding"></param>
        /// <returns></returns>
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
        public virtual IReadOnlyList<Operation> RemoveBinding(AbstractBinding binding, IEnumerable<UMI3DUser> users = null, bool syncServerTransform = true)
        {
            return RemoveOrDowngradeBinding(binding, users, syncServerTransform);
        }

        /// <inheritdoc/>
        public virtual IReadOnlyList<Operation> RemoveBinding(AbstractBinding binding, UMI3DUser user, bool syncServerTransform = true)
        {
            return RemoveOrDowngradeBinding(binding, new UMI3DUser[] { user }, syncServerTransform);
        }

        /// <inheritdoc/>
        public virtual IReadOnlyList<Operation> RemoveOrDowngradeBinding(AbstractBinding bindingToRemove, IEnumerable<UMI3DUser> users = null, bool syncServerTransform = true)
        {
            if (bindingToRemove is null)
            {
                UMI3DLogger.LogWarning($"Impossible to remove binding. Binding is null.", DEBUG_SCOPE);
                return null;
            }

            List<Operation> operations = new();

            if (users == null) // all users have same value
            {
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
                        operations.Add(bindingToRemove.GetDeleteEntity());

                        if (syncServerTransform)
                        {
                            var node = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractNode>(bindingToRemove.boundNodeId);
                            operations.Add(node.objectPosition.SetValue(node.transform.localPosition, true));
                            operations.Add(node.objectRotation.SetValue(node.transform.localRotation, true));
                            operations.Add(node.objectScale.SetValue(node.transform.localScale, true));
                        }
                    }
                    else if (bindingOnNode is MultiBinding existingMultiBinding // the binding is inside a multibinding
                            && bindingToRemove is AbstractSingleBinding singleBindingToRemove
                            && existingMultiBinding.bindings.Contains(singleBindingToRemove))
                    {
                        operations.Add(existingMultiBinding.GetDeleteEntity());

                        var newBinding = DowngradeBinding(existingMultiBinding, singleBindingToRemove);

                        operations.Add(newBinding.GetLoadEntity()); // a binding remains on node

                        bindings.SetValue(newBinding.boundNodeId, newBinding);

                        if (syncServerTransform)
                        {
                            var node = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractNode>(bindingToRemove.boundNodeId);
                            operations.Add(node.objectPosition.SetValue(node.transform.localPosition, true));
                            operations.Add(node.objectRotation.SetValue(node.transform.localRotation, true));
                            operations.Add(node.objectScale.SetValue(node.transform.localScale, true));
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
                var usersWithValues = users.Where((u) => bindings.GetValue(u).ContainsKey(bindingToRemove.boundNodeId)).ToList();

                if (usersWithValues.Count > 0)
                {
                    var usersWithSameBinding = usersWithValues.Where((u) => bindings.GetValue(u)[bindingToRemove.boundNodeId].Id() == bindingToRemove.Id()).ToList();

                    if (usersWithSameBinding.Count() > 0)
                    {
                        var ops = new List<Operation>();

                        UMI3DAbstractNode node = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractNode>(bindingToRemove.boundNodeId);

                        foreach (UMI3DUser user in usersWithSameBinding)
                        {
                            bindings.Remove(user, bindingToRemove.boundNodeId); // no binding left on node
                            if (syncServerTransform)
                            {
                                ops.Add(node.objectPosition.SetValue(user, node.transform.localPosition, true));
                                ops.Add(node.objectRotation.SetValue(user, node.transform.localRotation, true));
                                ops.Add(node.objectScale.SetValue(user, node.transform.localScale, true));
                            }
                        }
                        operations.Add(bindingToRemove.GetDeleteEntity(usersWithSameBinding.ToHashSet()));
                        operations.AddRange(ops);
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
                        if (syncServerTransform)
                        {
                            var node = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractNode>(bindingToRemove.boundNodeId);
                            operations.Add(node.objectPosition.SetValue(node.transform.localPosition, true));
                            operations.Add(node.objectRotation.SetValue(node.transform.localRotation, true));
                            operations.Add(node.objectScale.SetValue(node.transform.localScale, true));
                        }
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
        public virtual IReadOnlyList<Operation> RemoveAllBindings(ulong nodeId, IEnumerable<UMI3DUser> users = null, bool syncServerTransform = true)
        {
            var operations = new List<Operation>();
            if (users == null) // all users have same value
            {
                if (!bindings.GetValue().ContainsKey(nodeId))
                {
                    UMI3DLogger.LogWarning($"Cannot remove bindings on node {nodeId}. Not on node and not in multibinding for any user.", DEBUG_SCOPE);
                    return null;
                }

                var bindingToDelete = bindings.GetValue()[nodeId];

                operations.Add(bindingToDelete.GetDeleteEntity());

                if (syncServerTransform)
                {
                    var node = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractNode>(nodeId);
                    operations.Add(node.objectPosition.SetValue(node.objectPosition.GetValue(), true));
                    operations.Add(node.objectRotation.SetValue(node.objectRotation.GetValue(), true));
                    operations.Add(node.objectScale.SetValue(node.objectScale.GetValue(), true));
                }
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

                        if (syncServerTransform)
                        {
                            var ops = new List<Operation>();
                            var userList = userGroup.ToList();
                            var node = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractNode>(bindingToDelete.boundNodeId);

                            foreach (var user in userList)
                            {
                                ops.Add(node.objectPosition.SetValue(user, node.objectPosition.GetValue(user)));
                                ops.Add(node.objectRotation.SetValue(user, node.objectRotation.GetValue(user)));
                                ops.Add(node.objectScale.SetValue(user, node.objectScale.GetValue(user)));
                            }

                            operations.AddRange(ops);
                        }
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
        public virtual IReadOnlyList<Operation> RemoveAllBindings(ulong nodeId, UMI3DUser user, bool syncServerTransform = true)
        {
            return RemoveAllBindings(nodeId, new UMI3DUser[] { user }, syncServerTransform);
        }

        /// <inheritdoc/>
        public virtual IReadOnlyList<Operation> RemoveAllBindingsAndReattach(AbstractBinding binding, UMI3DAbstractNode newparent, IEnumerable<UMI3DUser> users = null)
        {
            List<Operation> operations = new();
            operations.AddRange(RemoveAllBindings(binding.boundNodeId, users));

            var node = umi3dEnvironmentService._GetEntityInstance<UMI3DAbstractNode>(binding.boundNodeId);
            node.transform.SetParent(newparent.transform, true);

            if (users == null)
            {
                operations.Add(node.objectParentId.SetValue(newparent));
            }
            else
            {
                foreach (UMI3DUser user in users)
                    operations.Add(node.objectParentId.SetValue(user, newparent));
            }
            return operations;
        }

        #endregion RemoveBinding
    }
}