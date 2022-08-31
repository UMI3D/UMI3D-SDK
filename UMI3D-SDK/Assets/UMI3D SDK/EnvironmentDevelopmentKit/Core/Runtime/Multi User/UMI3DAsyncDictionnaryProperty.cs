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
    /// <see cref="UMI3DAsyncProperty"/> for key-value collections.
    /// </summary>
    /// <typeparam name="T">Key type</typeparam>
    /// <typeparam name="L">Value type</typeparam>
    public class UMI3DAsyncDictionnaryProperty<T, L> : UMI3DAsyncProperty<Dictionary<T, L>>
    {
        /// <summary>
        /// A event that is triggered when inner value changes.
        /// </summary>
        public Action<T, L> OnInnerValueChanged;
        /// <summary>
        /// A event that is triggered when inner value changes.
        /// </summary>
        public Action<T, UMI3DUser, L> OnUserInnerValueChanged;
        /// <summary>
        /// A event that is triggered when inner value is Added.
        /// </summary>
        public Action<T, L> OnInnerValueAdded;
        /// <summary>
        /// A event that is triggered when inner value is Added.
        /// </summary>
        public Action<T, UMI3DUser, L> OnUserInnerValueAdded;

        /// <summary>
        /// A event that is triggered when inner value is Removed.
        /// </summary>
        public Action<T> OnInnerValueRemoved;
        /// <summary>
        /// A event that is triggered when inner value is Removed.
        /// </summary>
        public Action<T, UMI3DUser> OnUserInnerValueRemoved;

        /// <summary>
        /// Check the Equality between two <see cref="T"/> objects.
        /// </summary>
        private readonly Func<L, L, bool> Equal;

        /// <summary>
        /// The function serializing a <see cref="T"/> object.
        /// </summary>
        private readonly Func<T, UMI3DUser, object> SerializerT;

        /// <summary>
        /// The function serializing a <see cref="T"/> object.
        /// </summary>
        private readonly Func<L, UMI3DUser, object> SerializerL;

        /// <summary>
        /// The function serializing a <see cref="T"/> object.
        /// </summary>
        private readonly Func<Dictionary<T, L>, Dictionary<T, L>> Copier;

        /// <summary>
        /// Convert a serializer for a single value to a dictionnary serializer.
        /// </summary>
        /// <param name="serializer">Serializer for a single value.</param>
        /// <returns>Serializer for lists.</returns>
        private static Func<Dictionary<T, L>, UMI3DUser, object> SerializerToListSeriliser(Func<T, UMI3DUser, object> serializerT, Func<L, UMI3DUser, object> serializerL)
        {
            if (serializerT == null && serializerL == null) return null;
            if (serializerT == null)
            {
                serializerT = (T a, UMI3DUser u) => { return a; };
            }
            if (serializerL == null)
            {
                serializerL = (L a, UMI3DUser u) => { return a; };
            }
            object DictionarySerializer(Dictionary<T, L> dictionary, UMI3DUser u)
            {
                return dictionary.Select(t => { return new KeyValuePair<object, object>(serializerT(t.Key, u), serializerL(t.Value, u)); }).ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            return DictionarySerializer;
        }

        /// <summary>
        /// Helper class to compare two <see cref="T"/> objects.
        /// </summary>
        private class Comparer : IEqualityComparer<L>
        {
            private readonly Func<L, L, bool> _equals;

            public Comparer(Func<L, L, bool> equals)
            {
                _equals = equals;
            }

            public bool Equals(L x, L y)
            {
                return _equals(x, y);
            }

            public int GetHashCode(L obj)
            {
                return obj.GetHashCode();
            }
        };

        /// <summary>
        /// Convert a equal check function for a single value to a list check function.
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns>True if all the objects of the collections are the same</returns>
        private static Func<Dictionary<T, L>, Dictionary<T, L>, bool> EqualToListEqual(Func<L, L, bool> equal)
        {
            if (equal == null) return null;
            bool DictionnaryEqual(Dictionary<T, L> dict, Dictionary<T, L> other)
            {
                return !dict.Keys.Except(other.Keys).Any() && !other.Keys.Except(dict.Keys).Any() && !dict.Where(p => { return !equal(p.Value, other[p.Key]); }).Any();
            }
            return DictionnaryEqual;
        }

        public UMI3DAsyncDictionnaryProperty(ulong entityId, uint propertyId, Dictionary<T, L> value, Func<T, UMI3DUser, object> serializerT = null, Func<L, UMI3DUser, object> serializerL = null, Func<L, L, bool> equal = null, Func<Dictionary<T, L>, Dictionary<T, L>> copier = null) : base(entityId, propertyId, value, SerializerToListSeriliser(serializerT, serializerL), EqualToListEqual(equal))
        {
            if (equal == null)
            {
                equal = (L a, L b) => { return a.Equals(b); };
            }
            Equal = equal;
            if (serializerT == null)
            {
                serializerT = (T a, UMI3DUser u) => { return a; };
            }
            SerializerT = serializerT;
            if (serializerL == null)
            {
                serializerL = (L a, UMI3DUser u) => { return a; };
            }
            SerializerL = serializerL;
            if (copier == null)
            {
                copier = (Dictionary<T, L> a) => { return a.ToDictionary(p => p.Key, p => p.Value); };
            }
            Copier = copier;
        }

        //get[] operator definition
        public L this[T key] => GetValue()[key];

        public L this[T key, UMI3DUser user] => GetValue(user)[key];

        /// <summary>
        /// Get property value by key for a given user
        /// </summary>
        /// <param name="key">The key in the dictionnary</param>
        /// <param name="user">The user to get the key for</param>
        /// <returns></returns>
        /// A null user will call <see cref="UMI3DAsyncProperty.GetValue"/>
        public L GetValue(T key, UMI3DUser user = null)
        {
            return GetValue(user)[key];
        }

        /// <summary>
        /// Set the property's default/synchronized value.
        /// </summary>
        /// <param name="key">the index</param>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty SetValue(T key, L value, bool forceOperation = false)
        {
            L oldValue = GetValue()[key];

            if (((oldValue == null && value == null) || (oldValue != null && Equal(oldValue, value))) && !forceOperation)
                return null;
            GetValue()[key] = value;

            if (OnInnerValueChanged != null)
                OnInnerValueChanged.Invoke(key, value);

            var operation = new SetEntityDictionaryProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                key = SerializerT(key, null),
                value = SerializerL(value, null)
            };
            if (UMI3DEnvironment.Exists)
            {
                if (isAsync || isDeSync)
                {
                    operation += UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(
                        user => !asyncValues.ContainsKey(user) && !UserDesync.Contains(user));
                }
                else
                {
                    operation += UMI3DServer.Instance.Users();
                }
            }
            return operation;
        }

        /// <summary>
        /// Set the property's value for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="key">the index</param>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty SetValue(UMI3DUser user, T key, L value, bool forceOperation = false)
        {
            L oldValue = GetValue(user)[key];

            var operation = new SetEntityDictionaryProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                key = SerializerT(key, user),
                value = SerializerL(value, user)
            };
            operation.users.Add(user);

            if (asyncValues.ContainsKey(user))
            {
                if (((oldValue == null && value == null) || Equal(oldValue, value)) && !forceOperation)
                {
                    return null;
                }
                else
                {
                    GetValue(user)[key] = value;
                    if (OnUserInnerValueChanged != null)
                        OnUserInnerValueChanged.Invoke(key, user, value);
                    if (!UserDesync.Contains(user) || forceOperation)
                        return operation;
                    else
                        return null;
                }
            }
            else
            {
                Sync(user, false);
                GetValue(user)[key] = value;
                if (OnUserInnerValueChanged != null)
                    OnUserInnerValueChanged.Invoke(key, user, value);
                if (!UserDesync.Contains(user) || forceOperation)
                    return operation;
                else
                    return null;
            }
        }

        /// <summary>
        /// Add a keay-value pair to the dictionnary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty Add(T key, L value)
        {
            GetValue().Add(key, value);
            OnInnerValueAdded?.Invoke(key, value);

            var operation = new SetEntityDictionaryAddProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                key = SerializerT(key, null),
                value = SerializerL(value, null)
            };
            if (UMI3DEnvironment.Exists)
            {
                if (isAsync || isDeSync)
                {
                    operation += UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(
                        user => !asyncValues.ContainsKey(user) && !UserDesync.Contains(user));
                }
                else
                {
                    operation += UMI3DServer.Instance.Users();
                }
            }
            return operation;
        }

        /// <summary>
        /// Add a keay-value pair to the dictionnary for a given user.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty Add(UMI3DUser user, T key, L value)
        {
            var operation = new SetEntityDictionaryAddProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                key = SerializerT(key, user),
                value = SerializerL(value, user)
            };
            operation.users.Add(user);

            if (asyncValues.ContainsKey(user))
            {
                GetValue(user).Add(key, value);
                if (OnUserInnerValueAdded != null)
                    OnUserInnerValueAdded.Invoke(key, user, value);
                if (!UserDesync.Contains(user))
                    return operation;
                else
                    return null;
            }
            else
            {
                Sync(user, false);
                GetValue(user).Add(key, value);
                if (OnUserInnerValueAdded != null)
                    OnUserInnerValueAdded.Invoke(key, user, value);
                if (!UserDesync.Contains(user))
                    return operation;
                else
                    return null;
            }
        }

        /// <summary>
        /// Remove a key-value pair from the collection.
        /// </summary>
        /// <param name="key">Key of the pair to remove</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty Remove(T key)
        {
            if (!GetValue().ContainsKey(key)) return null;
            L value = GetValue()[key];
            if (!GetValue().Remove(key)) return null;
            if (OnInnerValueRemoved != null)
                OnInnerValueRemoved.Invoke(key);

            var operation = new SetEntityDictionaryRemoveProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                key = SerializerT(key, null),
                value = SerializerL(value, null)
            };
            if (UMI3DEnvironment.Exists)
            {
                if (isAsync || isDeSync)
                {
                    operation += UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(
                        user => !asyncValues.ContainsKey(user) && !UserDesync.Contains(user));
                }
                else
                {
                    operation += UMI3DServer.Instance.Users();
                }
            }
            return operation;
        }

        /// <summary>
        /// Remove a key-value pair from the collection for a given user.
        /// </summary>
        /// <param name="key">Key of the pair to remove</param>
        /// <returns>The operation to send to synchronize the changes.</returns>
        public SetEntityProperty Remove(UMI3DUser user, T key)
        {
            if (!GetValue(user).ContainsKey(key)) return null;
            L value = GetValue(user)[key];

            var operation = new SetEntityDictionaryRemoveProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                key = SerializerT(key, user),
                value = SerializerL(value, user)
            };
            operation.users.Add(user);

            if (asyncValues.ContainsKey(user))
            {
                if (!GetValue(user).Remove(key)) return null;
                if (OnUserInnerValueRemoved != null)
                    OnUserInnerValueRemoved.Invoke(key, user);
                if (!UserDesync.Contains(user))
                    return operation;
                else
                    return null;
            }
            else
            {
                Sync(user, false);
                if (!GetValue(user).Remove(key)) return null;
                if (OnUserInnerValueRemoved != null)
                    OnUserInnerValueRemoved.Invoke(key, user);
                if (!UserDesync.Contains(user))
                    return operation;
                else
                    return null;
            }
        }

        /// <inheritdoc/>
        protected override Dictionary<T, L> CopyOfValue(Dictionary<T, L> value) { return Copier(value); }
    }
}
