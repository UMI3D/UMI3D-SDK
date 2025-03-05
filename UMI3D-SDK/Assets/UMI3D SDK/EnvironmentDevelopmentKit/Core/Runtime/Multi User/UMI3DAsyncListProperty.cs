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

using System;
using System.Collections.Generic;
using System.Linq;

namespace umi3d.edk
{
    /// <summary>
    /// <see cref="UMI3DAsyncProperty"/> for list-like collections.
    /// </summary>
    /// <typeparam name="T">type of the values in the collection.</typeparam>
    public class UMI3DAsyncListProperty<T> : UMI3DAsyncProperty<List<T>>
    {
        /// <summary>
        /// A event that is triggered when inner value changes.
        /// </summary>
        public Action<int, T> OnInnerValueChanged;
        /// <summary>
        /// A event that is triggered when inner value changes.
        /// </summary>
        public Action<int, T, UMI3DGroupAsyncProperty> OnInnerGroupValueChanged;
        /// <summary>
        /// A event that is triggered when inner value changes.
        /// </summary>
        public Action<int, UMI3DUser, T> OnUserInnerValueChanged;
        /// <summary>
        /// A event that is triggered when inner value is Added.
        /// </summary>
        public Action<int, T> OnInnerValueAdded;
        /// <summary>
        /// A event that is triggered when inner value is Added.
        /// </summary>
        public Action<int, T, UMI3DGroupAsyncProperty> OnInnerGroupValueAdded;
        /// <summary>
        /// A event that is triggered when inner value is Added.
        /// </summary>
        public Action<int, UMI3DUser, T> OnUserInnerValueAdded;

        /// <summary>
        /// A event that is triggered when inner value is Removed.
        /// </summary>
        public Action<int, T> OnInnerValueRemoved;
        /// <summary>
        /// A event that is triggered when inner value is Removed.
        /// </summary>
        public Action<int, T, UMI3DGroupAsyncProperty> OnInnerGroupValueRemoved;
        /// <summary>
        /// A event that is triggered when inner value is Removed.
        /// </summary>
        public Action<int, UMI3DUser, T> OnUserInnerValueRemoved;

        /// <summary>
        /// The function checking the Equality between two <see cref="T"/> objects;
        /// </summary>
        private readonly Func<T, T, bool> Equal;

        /// <summary>
        /// The function serializing a <see cref="T"/> object.
        /// </summary>
        private readonly Func<T, UMI3DUser, object> Serializer;

        /// <summary>
        /// The function serializing a <see cref="T"/> object.
        /// </summary>
        private readonly Func<List<T>, List<T>> Copier;

        /// <summary>
        /// Convert a serializer for a single value to a list serializer.
        /// </summary>
        /// <param name="serializer">Serializer for a single value.</param>
        /// <returns>Serializer for lists.</returns>
        private static Func<List<T>, UMI3DUser, object> SerializerToListSeriliser(Func<T, UMI3DUser, object> serializer)
        {
            if (serializer == null) return null;
            object ListSerializer(List<T> list, UMI3DUser u)
            {
                return list.Select(t => { return serializer(t, u); }).ToList();
            }
            return ListSerializer;
        }

        /// <summary>
        /// Helper class to compare two <see cref="T"/> objects.
        /// </summary>
        private class Comparer : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _equals;

            public Comparer(Func<T, T, bool> equals)
            {
                _equals = equals;
            }

            public bool Equals(T x, T y)
            {
                return _equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        };

        /// <summary>
        /// Convert a equal check function for a single value to a list check function.
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns>True if all the objects of the collections are the same</returns>
        private static Func<List<T>, List<T>, bool> EqualToListEqual(Func<T, T, bool> equal)
        {
            if (equal == null) return null;
            bool ListEqual(List<T> list, List<T> other)
            {
                return !list.Except(other, new Comparer(equal)).Any() && !other.Except(list, new Comparer(equal)).Any();
            }
            return ListEqual;
        }


        internal UMI3DAsyncListProperty(IUMI3DServer umi3dServerService, ulong entityId, uint propertyId, List<T> value, Func<T, UMI3DUser, object> serializer = null, Func<T, T, bool> equal = null, Func<List<T>, List<T>> copier = null) : base(umi3dServerService,entityId, propertyId, value, SerializerToListSeriliser(serializer), EqualToListEqual(equal))
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
            if (copier == null)
            {
                copier = (List<T> a) => { return a.ToList(); };
            }
            Copier = copier;
        }

        public UMI3DAsyncListProperty(ulong entityId, uint propertyId, List<T> value, Func<T, UMI3DUser, object> serializer = null, Func<T, T, bool> equal = null, Func<List<T>, List<T>> copier = null)
            : this(UMI3DServer.Instance, entityId, propertyId, value, serializer, equal, copier)
        { }


        //get[] operator definition
        public T this[int index] => GetValue()[index];

        public T this[int index, UMI3DUser user] => GetValue(user)[index];

        /// <summary>
        /// Get property value by index for a given user.
        /// </summary>
        /// <param name="index">the index</param>
        /// <param name="user">the user</param>
        /// <returns></returns>
        /// A null user will call <see cref="UMI3DAsyncProperty.GetValue"/>
        public T GetValue(int index, UMI3DUser user = null)
        {
            return GetValue(user)[index];
        }

        public T GetValue(int index, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return GetValue(index);

            return GetValue(group)[index];
        }

        /// <summary>
        /// Set the property's default/synchronized value.
        /// </summary>
        /// <param name="index">the index</param>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty SetValue(int index, T value, bool forceOperation = false)
        {
            T oldValue = GetValue()[index];

            if (((oldValue == null && value == null) || (oldValue != null && Equal(oldValue, value))) && !forceOperation)
                return null;
            GetValue()[index] = value;

            if (OnInnerValueChanged != null)
                OnInnerValueChanged.Invoke(index, value);


            if (UMI3DEnvironment.Exists)
            {
                return GetSetEntityOperationForAllUsers(index);
            }
            return null;
        }

        public SetEntityProperty SetValue(int index, T value, UMI3DGroupAsyncProperty group, bool forceOperation = false)
        {
            if(group == null)
                return SetValue(index,value, forceOperation);

            if (!this.groupValueMaps.TryGetValue(group, out List<T> defaultValue))
            {
                group.Add(this);
                defaultValue = this.groupValueMaps[group];
            }

            T oldValue = defaultValue[index];

            if (((oldValue == null && value == null) || (oldValue != null && Equal(oldValue, value))) && !forceOperation)
                return null;

            defaultValue[index] = value;

            if (OnInnerGroupValueChanged != null)
                OnInnerGroupValueChanged.Invoke(index, value, group);

            return UMI3DEnvironment.Exists ? GetSetEntityOperationForAllUsers(index) : (SetEntityProperty)null;
        }

        /// <summary>
        /// Set the property's value for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="index">the index</param>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty SetValue(UMI3DUser user, int index, T value, bool forceOperation = false)
        {
            T oldValue = GetValue(user)[index];

            if (asyncValues.ContainsKey(user))
            {
                if (((oldValue == null && value == null) || Equal(oldValue, value)) && !forceOperation)
                {
                    return null;
                }
                else
                {
                    GetValue(user)[index] = value;
                    if (OnUserInnerValueChanged != null)
                        OnUserInnerValueChanged.Invoke(index, user, value);
                    if (!UserDesync.Contains(user) || forceOperation)
                        return GetSetEntityOperationForUser(user);
                    else
                        return null;
                }
            }
            else
            {
                Sync(user, false);
                GetValue(user)[index] = value;
                if (OnUserInnerValueChanged != null)
                    OnUserInnerValueChanged.Invoke(index, user, value);
                if (!UserDesync.Contains(user) || forceOperation)
                    return GetSetEntityOperationForUser(user);
                else
                    return null;
            }
        }

        /// <summary>
        /// Add a value to the collection.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty Add(T value)
        {
            int index = GetValue().Count;
            GetValue().Add(value);

            OnInnerValueAdded?.Invoke(index, value);

            return GetSetEntityListAddOperationForAllUsers(index);
        }

        public SetEntityProperty Add(T value, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return Add(value);

            if (!this.groupValueMaps.TryGetValue(group, out List<T> defaultValue))
            {
                group.Add(this);
                defaultValue = this.groupValueMaps[group];
            }

            int index = defaultValue.Count;
            defaultValue.Add(value);

            OnInnerGroupValueAdded?.Invoke(index, value,group);

            return GetSetEntityListAddOperationForAllUsers(index);
        }


        /// <summary>
        /// Add a value to the collection for a given user.
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty Add(UMI3DUser user, T value)
        {
            int index = GetValue(user).Count;

            if (!asyncValues.ContainsKey(user))
                Sync(user, false);

            GetValue(user).Add(value);
            if (OnUserInnerValueAdded != null)
                OnUserInnerValueAdded.Invoke(index, user, value);
            if (!UserDesync.Contains(user))
                return GetSetEntityListAddOperationForUser(index, user);
            else
                return null;
        }

        /// <summary>
        /// Remove a value from the collection.
        /// </summary>
        /// <param name="value">Value to remove</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty Remove(T value)
        {
            if (!GetValue().Contains(value)) return null;
            int index = GetValue().IndexOf(value);
            if (index < 0) return null;
            return RemoveAt(index);
        }

        public SetEntityProperty Remove(T value, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return Remove(value);

            if (!this.groupValueMaps.TryGetValue(group, out List<T> defaultValue))
            {
                group.Add(this);
                defaultValue = this.groupValueMaps[group];
            }

            if (!defaultValue.Contains(value)) return null;
            int index = defaultValue.IndexOf(value);
            if (index < 0) return null;
            return RemoveAt(index, group);
        }

        /// <summary>
        /// Remove a value from the collection for a given user.
        /// </summary>
        /// <param name="value">Value to remove</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty Remove(UMI3DUser user, T value)
        {
            if (!GetValue(user).Contains(value)) return null;
            int index = GetValue(user).IndexOf(value);
            if (index < 0) return null;
            return RemoveAt(user, index);
        }

        /// <summary>
        /// Remove a value from the collection at a given index.
        /// </summary>
        /// <param name="index">Index of the value to remove</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty RemoveAt(int index)
        {
            if (index < 0 || index >= GetValue().Count) return null;
            T value = GetValue()[index];

            var operation = GetSetEntityListRemoveOperationForAllUsers(index);

            GetValue().RemoveAt(index);
            OnInnerValueRemoved?.Invoke(index, value);

            return operation;
        }

        public SetEntityProperty RemoveAt(int index, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return RemoveAt(index);

            if (!this.groupValueMaps.TryGetValue(group, out List<T> defaultValue))
            {
                group.Add(this);
                defaultValue = this.groupValueMaps[group];
            }

            if (index < 0 || index >= defaultValue.Count) return null;
            T value = defaultValue[index];

            var operation = GetSetEntityListRemoveOperationForAllUsers(index, group);

            defaultValue.RemoveAt(index);
            OnInnerGroupValueRemoved?.Invoke(index, value, group);

            return operation;
        }

        /// <summary>
        /// Remove a value from the collection at a given index for a given user.
        /// </summary>
        /// <param name="index">Index of the value to remove</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty RemoveAt(UMI3DUser user, int index)
        {
            if (index < 0 || index >= GetValue(user).Count) return null;
            T value = GetValue(user)[index];

            if (!asyncValues.ContainsKey(user))
                Sync(user, false);

            var operation = GetSetEntityListRemoveOperationForUser(index, user);

            GetValue(user).RemoveAt(index);
            if (OnUserInnerValueRemoved != null)
                OnUserInnerValueRemoved.Invoke(index, user, value);
            if (!UserDesync.Contains(user))
                return operation;
            else
                return null;

        }

        /// <inheritdoc/>
        protected override List<T> CopyOfValue(List<T> value) { return Copier(value); }


        #region Set
        /// <summary>
        /// Get a SetEntityListProperty for this property for all users matching the async information.
        /// </summary>
        public virtual SetEntityListProperty GetSetEntityOperationForAllUsers(int index)
        {
            return GetSetEntityOperationForUsers(index, u => true);
        }

        public virtual SetEntityListProperty GetSetEntityOperationForAllUsers(int index, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return GetSetEntityOperationForAllUsers(index);

            return GetSetEntityOperationForUsers(index, u => true, group);
        }

        public virtual List<SetEntityListProperty> GetSetEntityOperationForUsersAndGroups(int index, Func<UMI3DUser, bool> condition)
        {
            var result = new List<SetEntityListProperty>()
            {
                GetSetEntityOperationForUsers(index,condition)
            };

            if (groupValueMaps.Count > 0)
                result.AddRange(groupValueMaps.Select(g => GetSetEntityOperationForUsers(index, condition, g.Key)));

            return result;
        }

        /// <summary>
        /// Get a SetEntityListProperty for this property for a given users.
        /// </summary>
        public virtual SetEntityListProperty GetSetEntityOperationForUser(int index, UMI3DUser user)
        {
            return new SetEntityListProperty()
            {
                users = new HashSet<UMI3DUser>() { user },
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(GetValue(user)[index], user)
            };
        }

        /// <summary>
        /// Get a SetEntityListProperty for this property for users matching the given condition and the async information.
        /// </summary>
        public virtual SetEntityListProperty GetSetEntityOperationForUsers(int index, Func<UMI3DUser, bool> condition)
        {
            bool IsUserAsync(UMI3DUser user)
            {
                return !asyncValues.ContainsKey(user) && !UserDesync.Contains(user) && !userGroupMaps.ContainsKey(user) && condition(user);
            }

            bool IsCondition(UMI3DUser user)
            {
                return !userGroupMaps.ContainsKey(user) && condition(user);
            }

            var _c = (isAsync || isDeSync) ? IsUserAsync : (Func<UMI3DUser, bool>)IsCondition;

            return new SetEntityListProperty()
            {
                users = GetUsersWhere(_c),
                entityId = entityId,
                index = index,
                property = propertyId,
                value = Serializer(GetValue()[index], null)
            };
        }

        /// <summary>
        /// Get a SetEntityListProperty for this property for users matching the given condition and the async information.
        /// </summary>
        public virtual SetEntityListProperty GetSetEntityOperationForUsers(int index, Func<UMI3DUser, bool> condition, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return GetSetEntityOperationForUsers(index, condition);

            bool IsUserAsync(UMI3DUser user)
            {
                return !asyncValues.ContainsKey(user) && !UserDesync.Contains(user) && userGroupMaps.ContainsKey(user) && userGroupMaps[user] == group && condition(user);
            }

            bool IsCondition(UMI3DUser user)
            {
                return userGroupMaps.ContainsKey(user) && userGroupMaps[user] == group && condition(user);
            }

            var _c = (isAsync || isDeSync) ? IsUserAsync : (Func<UMI3DUser, bool>)IsCondition;

            return new SetEntityListProperty()
            {
                users = GetUsersWhere(_c),
                entityId = entityId,
                index = index,
                property = propertyId,
                value = Serializer(GetValue()[index], null)
            };
        }
        #endregion Set

        #region Add
        /// <summary>
        /// Get a SetEntityListAddProperty for this property for all users matching the async information.
        /// </summary>
        public virtual SetEntityListAddProperty GetSetEntityListAddOperationForAllUsers(int index)
        {
            return GetSetEntityListAddOperationForUsers(index, u => true);
        }

        public virtual SetEntityListAddProperty GetSetEntityListAddOperationForAllUsers(int index, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                GetSetEntityListAddOperationForAllUsers(index);

            return GetSetEntityListAddOperationForUsers(index, u => true, group);
        }

        public virtual List<SetEntityListAddProperty> GetSetEntityListAddOperationForUsersAndGroups(int index, Func<UMI3DUser, bool> condition)
        {
            var result = new List<SetEntityListAddProperty>()
            {
                GetSetEntityListAddOperationForUsers(index,condition)
            };

            if (groupValueMaps.Count > 0)
                result.AddRange(groupValueMaps.Select(g => GetSetEntityListAddOperationForUsers(index, condition, g.Key)));

            return result;
        }

        /// <summary>
        /// Get a SetEntityListAddProperty for this property for a given users.
        /// </summary>
        public virtual SetEntityListAddProperty GetSetEntityListAddOperationForUser(int index, UMI3DUser user)
        {
            return new SetEntityListAddProperty()
            {
                users = new HashSet<UMI3DUser>() { user },
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(GetValue(user)[index], user)
            };
        }

        /// <summary>
        /// Get a SetEntityListAddProperty for this property for users matching the given condition and the async information.
        /// </summary>
        public virtual SetEntityListAddProperty GetSetEntityListAddOperationForUsers(int index, Func<UMI3DUser, bool> condition)
        {
            bool IsUserAsync(UMI3DUser user)
            {
                return !asyncValues.ContainsKey(user) && !UserDesync.Contains(user) && !userGroupMaps.ContainsKey(user) && condition(user);
            }

            bool IsCondition(UMI3DUser user)
            {
                return !userGroupMaps.ContainsKey(user) && condition(user);
            }

            var _c = (isAsync || isDeSync) ? IsUserAsync : (Func<UMI3DUser, bool>)IsCondition;

            return new SetEntityListAddProperty()
            {
                users = GetUsersWhere(_c),
                entityId = entityId,
                index = index,
                property = propertyId,
                value = Serializer(GetValue()[index], null)
            };
        }

        /// <summary>
        /// Get a SetEntityListProperty for this property for users matching the given condition and the async information.
        /// </summary>
        public virtual SetEntityListAddProperty GetSetEntityListAddOperationForUsers(int index, Func<UMI3DUser, bool> condition, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                return GetSetEntityListAddOperationForUsers(index, condition);

            bool IsUserAsync(UMI3DUser user)
            {
                return !asyncValues.ContainsKey(user) && !UserDesync.Contains(user) && userGroupMaps.ContainsKey(user) && userGroupMaps[user] == group && condition(user);
            }

            bool IsCondition(UMI3DUser user)
            {
                return userGroupMaps.ContainsKey(user) && userGroupMaps[user] == group && condition(user);
            }

            var _c = (isAsync || isDeSync) ? IsUserAsync : (Func<UMI3DUser, bool>)IsCondition;

            return new SetEntityListAddProperty()
            {
                users = GetUsersWhere(_c),
                entityId = entityId,
                index = index,
                property = propertyId,
                value = Serializer(GetValue()[index], null)
            };
        }
        #endregion Add

        #region Remove
        /// <summary>
        /// Get a SetEntityListRemoveProperty for this property for all users matching the async information.
        /// </summary>
        public virtual SetEntityListRemoveProperty GetSetEntityListRemoveOperationForAllUsers(int index)
        {
            return GetSetEntityListRemoveOperationForUsers(index, u => true);
        }

        public virtual SetEntityListRemoveProperty GetSetEntityListRemoveOperationForAllUsers(int index, UMI3DGroupAsyncProperty group)
        {
            if (group == null)
                GetSetEntityListRemoveOperationForAllUsers(index);

            return GetSetEntityListRemoveOperationForUsers(index, u => true, group);
        }

        public virtual List<SetEntityListRemoveProperty> GetSetEntityListRemoveOperationForUsersAndGroups(int index, Func<UMI3DUser, bool> condition)
        {
            var result = new List<SetEntityListRemoveProperty>()
            {
                GetSetEntityListRemoveOperationForUsers(index,condition)
            };

            if (groupValueMaps.Count > 0)
                result.AddRange(groupValueMaps.Select(g => GetSetEntityListRemoveOperationForUsers(index, condition, g.Key)));

            return result;
        }

        /// <summary>
        /// Get a SetEntityListRemoveProperty for this property for a given users.
        /// </summary>
        public virtual SetEntityListRemoveProperty GetSetEntityListRemoveOperationForUser(int index, UMI3DUser user)
        {
            return new SetEntityListRemoveProperty()
            {
                users = new HashSet<UMI3DUser>() { user },
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(GetValue(user)[index], user)
            };
        }

        /// <summary>
        /// Get a SetEntityListRemoveProperty for this property for users matching the given condition and the async information.
        /// </summary>
        public virtual SetEntityListRemoveProperty GetSetEntityListRemoveOperationForUsers(int index, Func<UMI3DUser, bool> condition)
        {
            bool IsUserAsync(UMI3DUser user)
            {
                return !asyncValues.ContainsKey(user) && !UserDesync.Contains(user) && !userGroupMaps.ContainsKey(user) && condition(user);
            }

            bool IsCondition(UMI3DUser user)
            {
                return !userGroupMaps.ContainsKey(user) && condition(user);
            }

            var _c = (isAsync || isDeSync) ? IsUserAsync : (Func<UMI3DUser, bool>)IsCondition;

            return new SetEntityListRemoveProperty()
            {
                users = GetUsersWhere(_c),
                entityId = entityId,
                index = index,
                property = propertyId,
                value = Serializer(GetValue()[index], null)
            };
        }

        /// <summary>
        /// Get a SetEntityListProperty for this property for users matching the given condition and the async information.
        /// </summary>
        public virtual SetEntityListRemoveProperty GetSetEntityListRemoveOperationForUsers(int index, Func<UMI3DUser, bool> condition, UMI3DGroupAsyncProperty group)
        {
            if(group == null)
                return GetSetEntityListRemoveOperationForUsers(index, condition);

            bool IsUserAsync(UMI3DUser user)
            {
                return !asyncValues.ContainsKey(user) && !UserDesync.Contains(user) && userGroupMaps.ContainsKey(user) && userGroupMaps[user] == group && condition(user);
            }

            bool IsCondition(UMI3DUser user)
            {
                return userGroupMaps.ContainsKey(user) && userGroupMaps[user] == group && condition(user);
            }

            var _c = (isAsync || isDeSync) ? IsUserAsync : (Func<UMI3DUser, bool>)IsCondition;

            return new SetEntityListRemoveProperty()
            {
                users = GetUsersWhere(_c),
                entityId = entityId,
                index = index,
                property = propertyId,
                value = Serializer(GetValue()[index], null)
            };
        }
        #endregion Remove
    }
}
