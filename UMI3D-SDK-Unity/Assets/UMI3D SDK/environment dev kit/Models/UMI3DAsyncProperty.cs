/*
Copyright 2019 Gfi Informatique

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
using UnityEngine;
using UnityEngine.Events;

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
        T value;

        /// <summary>
        /// The current async i.e. user specific values.
        /// </summary>
        Dictionary<UMI3DUser, T> asyncValues;
        Dictionary<UMI3DUser, UnityAction<UMI3DUser>> UserAsyncListeners;
        Dictionary<UMI3DUser, UnityAction<UMI3DUser>> UserDesyncListeners;
        /// <summary>
        /// Indicates if the property is asynchronous.
        /// </summary>
        public bool isAsync { get => asyncValues != null && asyncValues.Count > 0; }

        /// <summary>
        /// Indicates if the property is asynchronous.
        /// </summary>
        public bool isDeSync { get => UserDesyncListeners != null && UserDesyncListeners.Count > 0; }

        /// <summary>
        /// A event that is triggered when value changes.
        /// </summary>
        public Action<T> OnValueChanged;
        /// <summary>
        /// A event that is triggered when value changes.
        /// </summary>
        public Action<UMI3DUser, T> OnUserValueChanged;

        /// <summary>
        /// The object to which this property belongs.
        /// </summary>
        IHasAsyncProperties source;

        /// <summary>
        /// the function use to check the Equality between two T object;
        /// </summary>
        Func<T, T, bool> Equal;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<UMI3DUser> AsynchronousUser
        {
            get {
                return asyncValues.Keys;
            }
        }

        /// <summary>
        /// UMI3DAsyncProperty constructor.
        /// </summary>
        /// <param name="source">The object to which this property belongs.</param>
        /// <param name="value">The current default or synchronized value.</param>
        /// <param name="equal">Set the function use to check the equality between to value. If null the default object.Equals function will be use</param>
        public UMI3DAsyncProperty(IHasAsyncProperties source, T value, Func<T,T, bool> equal = null)
        {

            if(equal == null)
            {
                equal = (T a, T b) => { return a.Equals(b); };
            }
            Equal = equal;

            this.source = source;
            this.value = value;
            asyncValues = new Dictionary<UMI3DUser, T>();
            UserAsyncListeners = new Dictionary<UMI3DUser, UnityAction<UMI3DUser>>();
            UserDesyncListeners = new Dictionary<UMI3DUser, UnityAction<UMI3DUser>>();
        }

        /// <summary>
        /// Get property value for a given user
        /// </summary>
        /// <param name="user">the user</param>
        /// <returns></returns>
        public T GetValue(UMI3DUser user)
        {
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
        public void SetValue(T value)
        {
            if (this.value == null && value == null || this.value != null && Equal(this.value, value))
                return;
            this.value = value;
            if (OnValueChanged != null)
                OnValueChanged.Invoke(value);
            if ((isAsync || isDeSync) && UMI3D.Exist)
            {
                foreach (var user in UMI3D.UserManager.GetUsers())
                {
                    if (!asyncValues.ContainsKey(user) && !UserDesyncListeners.ContainsKey(user)) source.NotifyUpdate(user);
                }
            }
            else source.NotifyUpdate();
        }

        /// <summary>
        /// Set the property's value for a given user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="value">the new property's value</param>
        public void SetValue(UMI3DUser user,T value)
        {
            if (asyncValues.ContainsKey(user))
            {
                if (asyncValues[user] == null && value == null || Equal(asyncValues[user], value))
                    return ;
                else
                {
                    asyncValues[user] = value;
                    if (OnUserValueChanged != null)
                        OnUserValueChanged.Invoke(user, value);
                    if (!UserDesyncListeners.ContainsKey(user))
                        source.NotifyUpdate(user);
                }
            }
            else
            {
                Sync(user, false);
                asyncValues[user] = value;
                if (OnUserValueChanged != null)
                    OnUserValueChanged.Invoke(user, value);
                if(!UserDesyncListeners.ContainsKey(user))
                    source.NotifyUpdate(user);
            }
        }

        /// <summary>
        /// Set the property as synchronized/asynchronous.
        /// </summary>
        public void Sync()
        {
            if (isAsync || isDeSync)
                source.NotifyUpdate();
            foreach(var pair in UserAsyncListeners)
            {
                pair.Key.onUserDisconnection.RemoveListener(pair.Value);
            }
            foreach (var pair in UserDesyncListeners)
            {
                pair.Key.onUserDisconnection.RemoveListener(pair.Value);
            }
            asyncValues.Clear();
            UserAsyncListeners.Clear();
            UserDesyncListeners.Clear();
        }

        /// <summary>
        /// Set the property as synchronized/asynchronous for a user.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="isSync">Is the property async ?</param>
        public void Sync(UMI3DUser user, bool isSync)
        {
            if (!isSync)
            {
                asyncValues[user] = value;
                UnityAction<UMI3DUser> action = (u) => { Sync(user, true); };
                user.onUserDisconnection.AddListener(action);
                UserAsyncListeners[user] = action;
            }
            else if(asyncValues.ContainsKey(user))
            {
                if(!Equal(asyncValues[user], value) && !UserDesyncListeners.ContainsKey(user))
                {
                    source.NotifyUpdate(user);
                }
                asyncValues.Remove(user);
                user.onUserDisconnection.RemoveListener(UserAsyncListeners[user]);
                UserAsyncListeners.Remove(user);
            }
        }

        /// <summary>
        /// THe property will not notify update if desync.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="isSync">Is the property async ?</param>
        public void DeSync(UMI3DUser user, bool isnotifying)
        {
            if (!isnotifying)
            {
                if (!UserDesyncListeners.ContainsKey(user))
                {
                    UnityAction<UMI3DUser> action = (u) => { DeSync(user, true); };
                    user.onUserDisconnection.AddListener(action);
                    UserDesyncListeners[user] = action;
                }
            }
            else if (UserDesyncListeners.ContainsKey(user))
            {
                user.onUserDisconnection.RemoveListener(UserDesyncListeners[user]);
                UserDesyncListeners.Remove(user);
            }
        }

    }

    public class UMI3DAsyncPropertyEquality
    {
        public float epsilon = 0.000001f;

        public bool Vector3Equality(Vector3 a, Vector3 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y) && InRange(a.z - b.z);
        }

        public bool Vector2Equality(Vector2 a, Vector2 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y);
        }

        public bool Vector4Equality(Vector4 a, Vector4 b)
        {
            return InRange(a.x - b.x) && InRange(a.y - b.y) && InRange(a.z - b.z) && InRange(a.w - b.w);
        }

        public bool QuaternionEquality(Quaternion a, Quaternion b)
        {
            return InRange(Quaternion.Angle(a, b));
        }

        public bool FloatEquality(float a, float b)
        {
            return InRange(a-b);
        }

        bool InRange(float d)
        {
            return (d < epsilon && d > -epsilon);
        }
    }

}
