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
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Define an object property that could have a different value depending on the UMI3DUser.
    /// </summary>
    /// <typeparam name="T">the type of the property value.</typeparam>
    public class UMI3DAsyncProperty<T>
    {
        /// <summary>
        /// The current default or synchronized value.
        /// </summary>
        private T value;

        /// <summary>
        /// The id of this property.
        /// </summary>
        public uint propertyId { private set; get; }

        /// <summary>
        /// The id of the entity.
        /// </summary>
        public ulong entityId { private set; get; }

        /// <summary>
        /// The current async i.e. user specific values.
        /// </summary>
        protected Dictionary<UMI3DUser, T> asyncValues;
        protected HashSet<UMI3DUser> UserDesync;

        /// <summary>
        /// A event that is triggered when value changes.
        /// </summary>
        public Action<T> OnValueChanged;
        /// <summary>
        /// A event that is triggered when value changes.
        /// </summary>
        public Action<UMI3DUser, T> OnUserValueChanged;

        /// <summary>
        /// Indicates if the property is asynchronous.
        /// i.e if some user have specific values
        /// </summary>
        public bool isAsync => asyncValues != null && asyncValues.Count > 0;

        /// <summary>
        /// Indicates if the property is desynchronous.
        /// i.e if some user doesn't listen to change.
        /// </summary>
        public bool isDeSync => UserDesync != null && UserDesync.Count > 0;


        /// <summary>
        /// the function use to check the Equality between two T objects;
        /// </summary>
        private Func<T, T, bool> Equal;

        /// <summary>
        /// the function use to serialize a T object;
        /// </summary>
        private Func<T, UMI3DUser, object> Serializer;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<UMI3DUser> AsynchronousUser => asyncValues.Keys;

        public IEnumerable<UMI3DUser> DesynchronousUser => UserDesync.ToList();

        /// <summary>
        /// UMI3DAsyncProperty constructor.
        /// </summary>
        /// <param name="source">The object to which this property belongs.</param>
        /// <param name="value">The current default or synchronized value.</param>
        /// <param name="equal">Set the function use to check the equality between to value. If null the default object.Equals function will be use</param>
        public UMI3DAsyncProperty(ulong entityId, uint propertyId, T value, Func<T, UMI3DUser, object> serializer = null, Func<T, T, bool> equal = null)
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
            UMI3DServer.Instance.OnUserLeave.AddListener((u) => { DeSync(u, true); });
        }





        /// <summary>
        /// Get property value for a given user
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns></returns>
        public T GetValue(UMI3DUser user)
        {
            if (user == null)
                return value;
            return asyncValues.ContainsKey(user) ? asyncValues[user] : value;
        }

        /// <summary>
        /// Get property default/synchronized value
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns></returns>
        public T GetValue()
        {
            return value;
        }

        /// <summary>
        /// Set the property's default/synchronized value.
        /// </summary>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        public SetEntityProperty SetValue(T value, bool forceOperation = false)
        {
            if ((this.value == null && value == null || this.value != null && Equal(this.value, value)) && !forceOperation)
                return null;
            this.value = value;

            if (OnValueChanged != null)
                OnValueChanged.Invoke(value);
            SetEntityProperty operation;
            operation = new SetEntityProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
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
                    operation += UMI3DServer.Instance.Users();
                }
            }
            return operation;
        }

        /// <summary>
        /// Set the property's value for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="value">the new property's value</param>
        /// <param name="forceOperation">state if an operation should be return even if the new value is equal to the previous value</param>
        public SetEntityProperty SetValue(UMI3DUser user, T value, bool forceOperation = false)
        {
            var operation = new SetEntityProperty()
            {
                users = new HashSet<UMI3DUser>(),
                entityId = entityId,
                property = propertyId,
                value = Serializer(value, user)
            };
            operation.users.Add(user);

            if (asyncValues.ContainsKey(user))
            {
                if ((asyncValues[user] == null && value == null || Equal(asyncValues[user], value)) && !forceOperation)
                {
                    return null;
                }
                else
                {
                    asyncValues[user] = value;
                    if (OnUserValueChanged != null)
                        OnUserValueChanged.Invoke(user, value);
                    if (!UserDesync.Contains(user) || forceOperation)
                        return operation;
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
                    return operation;
                else
                    return null;
            }
        }

        /// <summary>
        /// Set the property as synchronized/asynchronous.
        /// </summary>
        public SetEntityProperty Sync()
        {
            SetEntityProperty operation = null;
            if (isAsync || isDeSync)
            {
                operation = new SetEntityProperty()
                {
                    users = UMI3DServer.Instance.UserSet(),
                    entityId = entityId,
                    property = propertyId,
                    value = Serializer(value, null)
                };
            }
            asyncValues.Clear();
            UserDesync.Clear();
            return operation;
        }

        /// <summary>
        /// Set the property as synchronized/asynchronous for a user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="isSync">Is the property async ?</param>
        public SetEntityProperty Sync(UMI3DUser user, bool isSync)
        {
            SetEntityProperty operation = null;

            if (!isSync)
            {
                asyncValues[user] = CopyOfValue(value);
            }
            else if (asyncValues.ContainsKey(user))
            {
                if (!Equal(asyncValues[user], value) && !UserDesync.Contains(user))
                {
                    operation = new SetEntityProperty()
                    {
                        users = new HashSet<UMI3DUser>(),
                        entityId = entityId,
                        property = propertyId,
                        value = Serializer(value, user)
                    };
                    operation.users.Add(user);
                }
                asyncValues.Remove(user);
            }
            return operation;
        }

        /// <summary>
        /// THe property will not notify update if desync.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="isSync">Is the property async ?</param>
        public SetEntityProperty DeSync(UMI3DUser user, bool isnotifying)
        {
            SetEntityProperty operation = null;
            if (!isnotifying)
            {
                UserDesync.Add(user);
            }
            else if (UserDesync.Contains(user))
            {
                UserDesync.Remove(user);
                operation = new SetEntityProperty()
                {
                    users = new HashSet<UMI3DUser>(),
                    entityId = entityId,
                    property = propertyId,
                    value = Serializer(GetValue(user), user)
                };
                operation.users.Add(user);
            }
            return operation;
        }

        protected virtual T CopyOfValue(T value) { return value; }

    }

    /// <summary>
    /// Collection of Equality fonction for float and struct containing float using an epsilon range.
    /// </summary>
    public class UMI3DAsyncPropertyEquality
    {
        /// <summary>
        /// Epsilon used for Equality test.
        /// A == B is true if A in ]B - epsilon; B + epsilon[.
        /// </summary>
        public float epsilon = 0.000001f;

        /// <summary>
        /// Vector Equality test component by component using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if all components are close enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool Vector3Equality(Vector3 a, Vector3 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y) && InRange(a.z - b.z);
        }

        /// <summary>
        /// Vector Equality test component by component using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if all components are close enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool Vector2Equality(Vector2 a, Vector2 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y);
        }

        /// <summary>
        /// Vector Equality test component by component using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if all components are close enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool Vector4Equality(Vector4 a, Vector4 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y) && InRange(a.z - b.z) && InRange(a.w - b.w);
        }

        /// <summary>
        /// Color Equality test component by component using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if all components are close enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool ColorEquality(Color a, Color b)
        {
            return InRange(a.a - b.a) && InRange(a.r - b.r) && InRange(a.b - b.b) && InRange(a.g - b.g);
        }

        /// <summary>
        /// Quaternion Equality test by angle using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if the angle between the two quaternion is small enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool QuaternionEquality(Quaternion a, Quaternion b)
        {
            return InRange(Quaternion.Angle(a, b));
        }

        /// <summary>
        /// Float Equality test using epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if the difference between the two float is small enough.</returns>
        /// <seealso cref="epsilon"/>
        public bool FloatEquality(float a, float b)
        {
            return InRange(a - b);
        }

        /// <summary>
        /// Check if a float is in epsilon range
        /// </summary>
        /// <param name="d"></param>
        /// <returns>return true if <paramref name="d"/> is in ]-epsilon,epsilon[</returns>
        private bool InRange(float d)
        {
            return (d < epsilon && d > -epsilon);
        }
    }


}
