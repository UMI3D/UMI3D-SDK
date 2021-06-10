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
    public class UMI3DAsyncListProperty<T> : UMI3DAsyncProperty<List<T>>
    {

        /// <summary>
        /// A event that is triggered when inner value changes.
        /// </summary>
        public Action<int, T> OnInnerValueChanged;
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
        public Action<int, UMI3DUser, T> OnUserInnerValueAdded;

        /// <summary>
        /// A event that is triggered when inner value is Removed.
        /// </summary>
        public Action<int, T> OnInnerValueRemoved;
        /// <summary>
        /// A event that is triggered when inner value is Removed.
        /// </summary>
        public Action<int, UMI3DUser, T> OnUserInnerValueRemoved;

        /// <summary>
        /// the function use to check the Equality between two T objects;
        /// </summary>
        Func<T, T, bool> Equal;

        /// <summary>
        /// the function use to serialize a T object;
        /// </summary>
        Func<T, UMI3DUser, object> Serializer;

        /// <summary>
        /// the function use to serialize a T object;
        /// </summary>
        Func<List<T>, List<T>> Copier;

        static Func<List<T>, UMI3DUser, object> SerializerToListSeriliser(Func<T, UMI3DUser, object> serializer)
        {
            if (serializer == null) return null;
            object ListSerializer(List<T> list, UMI3DUser u)
            {
                return list.Select(t => { return serializer(t, u); }).ToList();
            }
            return ListSerializer;
        }

        private class Comparer : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _equals;

            public Comparer(Func<T, T, bool> equals)
            {
                _equals = equals;
            }

            public bool Equals(T x, T y) => _equals(x, y);

            public int GetHashCode(T obj) => obj.GetHashCode();
        };

        static Func<List<T>, List<T>, bool> EqualToListEqual(Func<T, T, bool> equal)
        {
            if (equal == null) return null;
            bool ListEqual(List<T> list, List<T> other)
            {
                return !list.Except(other, new Comparer(equal)).Any() && !other.Except(list, new Comparer(equal)).Any();
            }
            return ListEqual;
        }

        public UMI3DAsyncListProperty(string entityId, string propertyId, List<T> value, Func<T, UMI3DUser, object> serializer = null, Func<T, T, bool> equal = null, Func<List<T>, List<T>> copier = null) : base(entityId, propertyId, value, SerializerToListSeriliser(serializer), EqualToListEqual(equal))
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

        public T this[int index]
        {
            get => GetValue()[index];
        }

        public T this[int index, UMI3DUser user]
        {
            get => GetValue(user)[index];
        }

        /// <summary>
        /// Get property value for a given user
        /// </summary>
        /// <param name="index">the index</param>
        /// <param name="user">the user</param>
        /// <returns></returns>
        public T GetValue(int index, UMI3DUser user = null)
        {
            return GetValue(user)[index];
        }

        /// <summary>
        /// Set the property's default/synchronized value.
        /// </summary>
        /// <param name="index">the index</param>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        public SetEntityProperty SetValue(int index, T value, bool forceOperation = false)
        {
            var oldValue = GetValue()[index];

            if ((oldValue == null && value == null || oldValue != null && Equal(oldValue, value)) && !forceOperation)
                return null;
            GetValue()[index] = value;

            if (OnInnerValueChanged != null)
                OnInnerValueChanged.Invoke(index, value);

            var operation = new SetEntityListProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(value, null)
            };
            if (UMI3DEnvironment.Exists)
            {
                if ((isAsync || isDeSync))
                {
                    operation += UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(
                        user => !asyncValues.ContainsKey(user) && !UserDesync.Contains(user));
                }
                else
                {
                    operation += UMI3DEnvironment.GetEntities<UMI3DUser>();
                }
            }
            return operation;
        }

        /// <summary>
        /// Set the property's value for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="index">the index</param>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        public SetEntityProperty SetValue(UMI3DUser user, int index, T value, bool forceOperation = false)
        {
            var oldValue = GetValue(user)[index];

            var operation = new SetEntityListProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(value, user)
            };
            operation.users.Add(user);

            if (asyncValues.ContainsKey(user))
            {
                if ((oldValue == null && value == null || Equal(oldValue, value)) && !forceOperation)
                    return null;
                else
                {
                    GetValue(user)[index] = value;
                    if (OnUserInnerValueChanged != null)
                        OnUserInnerValueChanged.Invoke(index, user, value);
                    if (!UserDesync.Contains(user) || forceOperation)
                        return operation;
                    else
                        return null;
                }
            }
            else
            {
                Sync(user, false);
                asyncValues[user] = Copier(GetValue());
                GetValue(user)[index] = value;
                if (OnUserInnerValueChanged != null)
                    OnUserInnerValueChanged.Invoke(index, user, value);
                if (!UserDesync.Contains(user) || forceOperation)
                    return operation;
                else
                    return null;
            }
        }

        public SetEntityProperty Add(T value)
        {
            var index = GetValue().Count;
            GetValue().Add(value);

            OnInnerValueAdded?.Invoke(index, value);

            var operation = new SetEntityListAddProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(value, null)
            };
            if (UMI3DEnvironment.Exists)
            {
                if ((isAsync || isDeSync))
                {
                    operation += UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(
                        user => !asyncValues.ContainsKey(user) && !UserDesync.Contains(user));
                }
                else
                {
                    operation += UMI3DEnvironment.GetEntities<UMI3DUser>();
                }
            }
            return operation;
        }

        public SetEntityProperty Add(UMI3DUser user, T value)
        {
            var index = GetValue(user).Count;


            var operation = new SetEntityListAddProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(value, user)
            };
            operation.users.Add(user);

            if (asyncValues.ContainsKey(user))
            {
                GetValue(user).Add(value);
                if (OnUserInnerValueAdded != null)
                    OnUserInnerValueAdded.Invoke(index, user, value);
                if (!UserDesync.Contains(user))
                    return operation;
                else
                    return null;
            }
            else
            {
                Sync(user, false);
                asyncValues[user] = Copier(GetValue());
                GetValue(user).Add(value);
                if (OnUserInnerValueAdded != null)
                    OnUserInnerValueAdded.Invoke(index, user, value);
                if (!UserDesync.Contains(user))
                    return operation;
                else
                    return null;
            }
        }

        public SetEntityProperty Remove(T value)
        {
            if (!GetValue().Contains(value)) return null;
            int index = GetValue().IndexOf(value);
            if (index < 0) return null;
            return RemoveAt(index);
        }

        public SetEntityProperty Remove(UMI3DUser user, T value)
        {
            if (!GetValue(user).Contains(value)) return null;
            int index = GetValue(user).IndexOf(value);
            if (index < 0) return null;
            return RemoveAt(user, index);
        }

        public SetEntityProperty RemoveAt(int index)
        {
            if (index < 0 || index >= GetValue().Count) return null;
            T value = GetValue()[index];
            GetValue().RemoveAt(index);
            OnInnerValueRemoved?.Invoke(index, value);

            var operation = new SetEntityListRemoveProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(value, null)
            };
            if (UMI3DEnvironment.Exists)
            {
                if ((isAsync || isDeSync))
                {
                    operation += UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(
                        user => !asyncValues.ContainsKey(user) && !UserDesync.Contains(user));
                }
                else
                {
                    operation += UMI3DEnvironment.GetEntities<UMI3DUser>();
                }
            }
            return operation;
        }

        public SetEntityProperty RemoveAt(UMI3DUser user, int index)
        {
            if (index < 0 || index >= GetValue(user).Count) return null;
            T value = GetValue(user)[index];
            var operation = new SetEntityListRemoveProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                index = index,
                value = Serializer(value, user)
            };
            operation.users.Add(user);

            if (asyncValues.ContainsKey(user))
            {
                GetValue(user).RemoveAt(index);
                if (OnUserInnerValueRemoved != null)
                    OnUserInnerValueRemoved.Invoke(index, user, value);
                if (!UserDesync.Contains(user))
                    return operation;
                else
                    return null;
            }
            else
            {
                Sync(user, false);
                GetValue(user).RemoveAt(index);
                if (OnUserInnerValueRemoved != null)
                    OnUserInnerValueRemoved.Invoke(index, user, value);
                if (!UserDesync.Contains(user))
                    return operation;
                else
                    return null;
            }
        }


        protected override List<T> CopyOfValue(List<T> value) { return Copier(value); }
    }
}
