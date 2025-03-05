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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Define an object property that could have a different value depending on the <see cref="UMI3DUser"/>.
    /// </summary>
    public abstract class UMI3DAsyncProperty
    {
        /// <summary>
        /// Add a user in a group
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        /// <returns>SetEntityProperty for all impacted user</returns>
        /// <remarks>Should be call only by <see cref="UMI3DGroupAsyncProperty"/></remarks>
        internal abstract SetEntityProperty AddToGroup(UMI3DUser user, UMI3DGroupAsyncProperty group);

        /// <summary>
        /// Remove a user from a group
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param> 
        /// <returns>SetEntityProperty for all impacted user</returns>
        /// <remarks>Should be call only by <see cref="UMI3DGroupAsyncProperty"/></remarks>
        internal abstract SetEntityProperty RemoveFromGroup(UMI3DUser user, UMI3DGroupAsyncProperty group);

        /// <summary>
        /// Add a group
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        /// <returns><see langword="true"/> if the property already contains the group</returns>
        /// <remarks>Should be call only by <see cref="UMI3DGroupAsyncProperty"/></remarks>
        internal abstract bool AddGroup(UMI3DGroupAsyncProperty group);

        /// <summary>
        /// Remove a group
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        /// <returns>SetEntityProperty for all impacted user</returns>
        /// <remarks>Should be call only by <see cref="UMI3DGroupAsyncProperty"/></remarks>
        internal abstract SetEntityProperty RemoveGroup(UMI3DGroupAsyncProperty group);

        /// <summary>
        /// Get a <see cref="SetEntityProperty"/> operation for this property for all users matching the async information.
        /// </summary>
        public abstract SetEntityProperty GetSetEntityOperationForAllUsers();

        /// <summary>
        /// Get a <see cref="SetEntityProperty"/> operation for this property for all users matching the async information.
        /// </summary>
        public abstract SetEntityProperty GetSetEntityOperationForAllUsers(UMI3DGroupAsyncProperty group);

        /// <summary>
        /// Get a list of <see cref="SetEntityProperty"/> operation for this property for all users matching the async information.
        /// </summary>
        public abstract List<SetEntityProperty> GetSetEntityOperationForAllUsersAndGroups();

        /// <summary>
        /// Get a <see cref="SetEntityProperty"/> operation for this property for a given user.
        /// </summary>
        public abstract SetEntityProperty GetSetEntityOperationForUser(UMI3DUser user);

        /// <summary>
        /// Get a <see cref="SetEntityProperty"/> operation for this property for users matching the given condition and the async information.
        /// </summary>
        public abstract SetEntityProperty GetSetEntityOperationForUsers(Func<UMI3DUser, bool> condition);

        /// <summary>
        /// Get a <see cref="SetEntityProperty"/> operation for this property for users matching the given condition and the async information.
        /// </summary>
        public abstract SetEntityProperty GetSetEntityOperationForUsers(Func<UMI3DUser, bool> condition, UMI3DGroupAsyncProperty group);

        /// <summary>
        /// Get a list of <see cref="SetEntityProperty"/> operation for this property for users matching the given condition and the async information.
        /// </summary>
        public abstract List<SetEntityProperty> GetSetEntityOperationForUsersAndGroups(Func<UMI3DUser, bool> condition);

        /// <summary>
        /// Indicates if the property is asynchronous.
        /// i.e if some user have specific values
        /// </summary>
        public abstract bool isAsync { get; }

        /// <summary>
        /// Indicates if the property is desynchronous.
        /// i.e if some user doesn't listen to change.
        /// </summary>
        public abstract bool isDeSync { get; }

        /// <summary>
        /// Collection of users that have this property set as asynchronous, i.e. with specific values.
        /// </summary>
        public abstract IEnumerable<UMI3DUser> AsynchronousUser { get; }
        /// <summary>
        /// Collection of users that have this property set as desynchronous, i.e. they don't listen to changes.
        /// </summary>
        public abstract IEnumerable<UMI3DUser> DesynchronousUser { get; }

        /// <summary>
        /// Set the property as synchronized/asynchronous.
        /// This does not affect users in groups
        /// </summary>
        public abstract SetEntityProperty Sync();

        /// <summary>
        /// Set the property as synchronized/asynchronous in a group.
        /// </summary>
        public abstract SetEntityProperty Sync(UMI3DGroupAsyncProperty group);

        /// <summary>
        /// Set the property as synchronized/asynchronous.
        /// </summary>
        public abstract List<SetEntityProperty> SyncAll();

        /// <summary>
        /// Set the property as synchronized/asynchronous for a user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="isSync">Is the property async ?</param>
        public abstract SetEntityProperty Sync(UMI3DUser user, bool isSync);

        /// <summary>
        /// The property will not notify update if desync.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="isSync">Is the property async ?</param>
        public abstract SetEntityProperty DeSync(UMI3DUser user, bool isnotifying);
    }


    /// <summary>
    /// Define an object property that could be edited and have a different value depending on the <see cref="UMI3DUser"/>.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// AsyncProperties require to be manually updated when their value change.
    public class UMI3DAsyncProperty<T> : UMI3DAsyncProperty
    {
        /// <summary>
        /// The current default or synchronized value.
        /// </summary>
        private T value;

        /// <summary>
        /// Maps of group, use to give default value for user in group
        /// </summary>
        protected Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> userGroupMaps = new();
        protected Dictionary<UMI3DGroupAsyncProperty, T> groupValueMaps = new();

#if UNITY_EDITOR
        internal IReadOnlyDictionary<UMI3DUser, UMI3DGroupAsyncProperty> TEST_UserGroupMaps => userGroupMaps;
        internal IReadOnlyDictionary<UMI3DGroupAsyncProperty, T> TEST_GroupValueMaps => groupValueMaps;
#endif
        /// <summary>
        /// The id of this property.
        /// </summary>
        public uint propertyId { private set; get; }

        /// <summary>
        /// The id of the entity.
        /// </summary>
        public ulong entityId { private set; get; }

        /// <summary>
        /// The current async (user specific) value per user.
        /// </summary>
        protected Dictionary<UMI3DUser, T> asyncValues;
        /// <summary>
        /// Collection of current desync users for this property.
        /// </summary>
        protected HashSet<UMI3DUser> UserDesync;

        /// <summary>
        /// A event that is triggered when value changes.
        /// </summary>
        public Action<T> OnValueChanged;

        /// <summary>
        /// A event that is triggered when value changes.
        /// </summary>
        public Action<T, UMI3DGroupAsyncProperty> OnGroupValueChanged;

        /// <summary>
        /// A event that is triggered when value changes.
        /// </summary>
        public Action<UMI3DUser, T> OnUserValueChanged;

        /// <inheritdoc/>
        public override bool isAsync => asyncValues != null && asyncValues.Count > 0;

        /// <inheritdoc/>
        public override bool isDeSync => UserDesync != null && UserDesync.Count > 0;

        /// <summary>
        /// The function checking the Equality between two <see cref="T"/> objects.
        /// </summary>
        private readonly Func<T, T, bool> Equal;

        /// <summary>
        /// The function serializing a <see cref="T"/> object.
        /// </summary>
        private readonly Func<T, UMI3DUser, object> Serializer;

        /// <inheritdoc/>
        public override IEnumerable<UMI3DUser> AsynchronousUser => asyncValues.Keys;

        /// <inheritdoc/>
        public override IEnumerable<UMI3DUser> DesynchronousUser => UserDesync.ToList();

        private readonly IUMI3DServer umi3dServerService;

        /// <summary>
        /// UMI3DAsyncProperty constructor.
        /// </summary>
        /// <param name="source">The object to which this property belongs.</param>
        /// <param name="value">The current default or synchronized value.</param>
        /// <param name="equal">Set the function use to check the equality between to value. If null the default object.Equals function will be use</param>
        public UMI3DAsyncProperty(ulong entityId, uint propertyId, T value, Func<T, UMI3DUser, object> serializer = null, Func<T, T, bool> equal = null) :
            this(UMI3DServer.Instance, entityId, propertyId, value, serializer, equal)
        { }


        internal UMI3DAsyncProperty(IUMI3DServer umi3dServerService, ulong entityId, uint propertyId, T value, Func<T, UMI3DUser, object> serializer = null, Func<T, T, bool> equal = null)
        {
            if (equal == null)
            {
                equal = (T a, T b) => { return a.Equals(b); };
            }
            Equal = equal;
            if (serializer == null)
            {
                serializer = (T a, UMI3DUser u) => { return a; };
            }
            Serializer = serializer;
            this.entityId = entityId;
            this.propertyId = propertyId;
            this.value = value;
            asyncValues = new Dictionary<UMI3DUser, T>();
            UserDesync = new HashSet<UMI3DUser>();

            this.umi3dServerService = umi3dServerService;

            this.umi3dServerService.OnUserLeave.AddListener((u) => { DeSync(u, true); });
        }

        #region Group
        /// <inheritdoc/>
        internal override SetEntityProperty AddToGroup(UMI3DUser user, UMI3DGroupAsyncProperty group)
        {
            if (!groupValueMaps.ContainsKey(group))
                throw new Exception($"This property does not contain {group}, the group should be added with AddGroup(group)");

            userGroupMaps[user] = group;

            return GetSetEntityOperationForUser(user);
        }

        /// <inheritdoc/>
        internal override SetEntityProperty RemoveFromGroup(UMI3DUser user, UMI3DGroupAsyncProperty group)
        {
            if (!groupValueMaps.ContainsKey(group))
                return null;

            if (userGroupMaps.TryGetValue(user, out var groupT) && groupT == group)
                userGroupMaps.Remove(user);

            return GetSetEntityOperationForUser(user);
        }

        /// <inheritdoc/>
        internal override bool AddGroup(UMI3DGroupAsyncProperty group)
        {
            if (groupValueMaps.ContainsKey(group))
                return false;

            groupValueMaps.Add(group, GetValue());
            group.ForEach(u => userGroupMaps[u] = group);

            return true;
        }

        /// <inheritdoc/>
        internal override SetEntityProperty RemoveGroup(UMI3DGroupAsyncProperty group)
        {
            if (groupValueMaps.Remove(group))
            {
                group.ForEach(u => userGroupMaps.Remove(u));
                return GetSetEntityOperationForUsers(group.Contains);
            }
            return null;
        }
        #endregion Group

        /// <summary>
        /// Get property value for a given user
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns></returns>
        public virtual T GetValue(UMI3DUser user)
        {
            if (user == null)
                return value;

            return asyncValues.ContainsKey(user)
                ? asyncValues[user]
                : userGroupMaps.TryGetValue(user, out var group)
                    ? groupValueMaps.TryGetValue(group, out var groupValue)
                        ? groupValue
                        : value
                    : value;
        }

        /// <summary>
        /// Get property default/synchronized value
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns></returns>
        public virtual T GetValue()
        {
            return value;
        }

        public virtual T GetValue(UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return GetValue();

            return groupValueMaps.TryGetValue(group, out T value) ? value : this.value;
        }

        #region Set
        /// <summary>
        /// Set the property's default/synchronized value.
        /// </summary>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        public virtual SetEntityProperty SetValue(T value, bool forceOperation = false)
        {
            if (((this.value == null && value == null) || (this.value != null && Equal(this.value, value))) && !forceOperation)
                return null;

            this.value = value;

            if (OnValueChanged != null)
                OnValueChanged.Invoke(value);

            if (UMI3DEnvironment.Exists)
            {
                return GetSetEntityOperationForAllUsers();
            }

            return null;
        }

        public virtual SetEntityProperty SetValue(T value, UMI3DGroupAsyncProperty group, bool forceOperation = false)
        {
            if(group == null)
                return SetValue(value, forceOperation);

            if (!this.groupValueMaps.TryGetValue(group, out T defaultValue))
            {
                group.Add(this);
                defaultValue = this.groupValueMaps[group];
            }

            if (((defaultValue == null && value == null) || (defaultValue != null && Equal(defaultValue, value))) && !forceOperation)
                return null;

            this.groupValueMaps[group] = value;

            if (OnGroupValueChanged != null)
                OnGroupValueChanged.Invoke(value, group);

            return GetSetEntityOperationForAllUsers();
        }

        /// <summary>
        /// Set the property's value for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        public virtual SetEntityProperty SetValue(UMI3DUser user, T value, bool forceOperation = false)
        {
            if (user == null)
                return SetValue(value, forceOperation);

            if (asyncValues.ContainsKey(user))
            {
                if (((asyncValues[user] == null && value == null) || Equal(asyncValues[user], value)) && !forceOperation)
                {
                    return null;
                }
                else
                {
                    asyncValues[user] = value;
                    if (OnUserValueChanged != null)
                        OnUserValueChanged.Invoke(user, value);
                    if (!UserDesync.Contains(user) || forceOperation)
                        return GetSetEntityOperationForUser(user);
                    else
                        return null;
                }
            }
            else
            {
                Sync(user, false);
                asyncValues[user] = value;
                if (OnUserValueChanged != null)
                    OnUserValueChanged.Invoke(user, value);
                if (!UserDesync.Contains(user) || forceOperation)
                    return GetSetEntityOperationForUser(user);
                else
                    return null;
            }
        }

        /// <inheritdoc/>
        public override SetEntityProperty GetSetEntityOperationForAllUsers()
        {
            return GetSetEntityOperationForUsers(u => true);
        }

        /// <inheritdoc/>
        public override SetEntityProperty GetSetEntityOperationForAllUsers(UMI3DGroupAsyncProperty group)
        {
            if(group == null)
                return GetSetEntityOperationForUsers(u => true);

            return GetSetEntityOperationForUsers(u => true, group);
        }

        /// <inheritdoc/>
        public override SetEntityProperty GetSetEntityOperationForUser(UMI3DUser user)
        {
            return new SetEntityProperty()
            {
                users = new HashSet<UMI3DUser>() { user },
                entityId = entityId,
                property = propertyId,
                value = Serializer(GetValue(user), user)
            };
        }

        public override List<SetEntityProperty> GetSetEntityOperationForAllUsersAndGroups()
        {
            return GetSetEntityOperationForUsersAndGroups(u => true);
        }

        public override List<SetEntityProperty> GetSetEntityOperationForUsersAndGroups(Func<UMI3DUser, bool> condition)
        {
            List<SetEntityProperty> list = new()
            {
                GetSetEntityOperationForUsers(condition)
            };

            if (groupValueMaps.Count > 0)
                list.AddRange(groupValueMaps.Select(g => GetSetEntityOperationForUsers(condition, g.Key)));

            return list;
        }

        /// <inheritdoc/>
        public override SetEntityProperty GetSetEntityOperationForUsers(Func<UMI3DUser, bool> condition)
        {
            bool IsUserSyncAndCondition(UMI3DUser user)
            {
                return !asyncValues.ContainsKey(user) && !UserDesync.Contains(user) && !userGroupMaps.ContainsKey(user) && condition(user);
            }

            bool IsCondition(UMI3DUser user)
            {
                return !userGroupMaps.ContainsKey(user) && condition(user);
            }

            var _c = (isAsync || isDeSync) ? IsUserSyncAndCondition : (Func<UMI3DUser, bool>)IsCondition;

            return new SetEntityProperty()
            {
                users = GetUsersWhere(_c),
                entityId = entityId,
                property = propertyId,
                value = Serializer(GetValue(), null)
            };
        }

        /// <inheritdoc/>
        public override SetEntityProperty GetSetEntityOperationForUsers(Func<UMI3DUser, bool> condition, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return GetSetEntityOperationForUsers(condition);

            bool IsUserSyncAndCondition(UMI3DUser user)
            {
                return !asyncValues.ContainsKey(user) && !UserDesync.Contains(user) && userGroupMaps.ContainsKey(user) && userGroupMaps[user] == group && condition(user);
            }

            bool IsCondition(UMI3DUser user)
            {
                return userGroupMaps.ContainsKey(user) && userGroupMaps[user] == group && condition(user);
            }

            var _c = (isAsync || isDeSync) ? (Func<UMI3DUser, bool>)IsUserSyncAndCondition : IsCondition;

            return new SetEntityProperty()
            {
                users = GetUsersWhere(_c),
                entityId = entityId,
                property = propertyId,
                value = Serializer(GetValue(group), null)
            };
        }

        protected HashSet<UMI3DUser> GetUsersWhere(Func<UMI3DUser, bool> condition) => new HashSet<UMI3DUser>(umi3dServerService.Users().Where(condition));

        #endregion Set

        /// <inheritdoc/>
        public override SetEntityProperty Sync()
        {
            if (!isAsync && !isDeSync)
                return null;

            asyncValues.Keys.Where(u => !userGroupMaps.ContainsKey(u)).ToList().ForEach(u => asyncValues.Remove(u));
            UserDesync.Where(u => !userGroupMaps.ContainsKey(u)).ToList().ForEach(u => UserDesync.Remove(u));

            return GetSetEntityOperationForAllUsers();
        }

        /// <inheritdoc/>
        public override SetEntityProperty Sync(UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return null;

            if (!isAsync && !isDeSync)
                return null;

            group.ForEach(u => asyncValues.Remove(u));
            group.ForEach(u => UserDesync.Remove(u));

            return GetSetEntityOperationForAllUsers(group);
        }

        /// <inheritdoc/>
        public override List<SetEntityProperty> SyncAll()
        {
            if (!isAsync && !isDeSync)
                return null;

            asyncValues.Clear();
            UserDesync.Clear();

            return GetSetEntityOperationForAllUsersAndGroups();
        }

        /// <inheritdoc/>
        public override SetEntityProperty Sync(UMI3DUser user, bool isSync)
        {
            SetEntityProperty operation = null;

            if (!isSync)
            {
                asyncValues[user] = CopyOfValue(GetValue(user));
            }
            else if (asyncValues.ContainsKey(user))
            {
                T userAsyncValue = asyncValues[user];
                asyncValues.Remove(user);
                if (!Equal(userAsyncValue, GetValue(user)) && !UserDesync.Contains(user))
                {
                    operation = GetSetEntityOperationForUser(user);
                }
            }
            return operation;
        }

        /// <inheritdoc/>
        public override SetEntityProperty DeSync(UMI3DUser user, bool isNotifying)
        {
            SetEntityProperty operation = null;
            if (!isNotifying)
            {
                UserDesync.Add(user);
            }
            else if (UserDesync.Contains(user))
            {
                UserDesync.Remove(user);
                operation = GetSetEntityOperationForUser(user);
            }
            return operation;
        }

        /// <summary>
        /// Get a copy of the value set as parameter.
        /// </summary>
        /// <param name="value">Value to copy.</param>
        /// <returns>Copied value.</returns>
        /// Override this method to implement a custom copier, for reference types for examples.
        protected virtual T CopyOfValue(T value) { return value; }
    }
}