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

        /// <summary>
        /// Indicates if the property is asynchronous.
        /// </summary>
        bool isAsync;
        
        /// <summary>
        /// A event that is triggered when value changes.
        /// </summary>
        public Action<T> OnValueChanged;

        /// <summary>
        /// The object to which this property belongs.
        /// </summary>
        IHasAsyncProperties source;

        /// <summary>
        /// UMI3DAsyncProperty constructor.
        /// </summary>
        /// <param name="source">The object to which this property belongs.</param>
        /// <param name="value">The current default or synchronized value.</param>
        /// <param name="async">Indicates if the property is asynchronous.</param>
        public UMI3DAsyncProperty(IHasAsyncProperties source, T value, bool async = false)
        {
            this.source = source;
            this.value = value;
            this.isAsync = async;
            asyncValues = new Dictionary<UMI3DUser, T>();
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
            if (this.value == null && value == null || this.value != null && this.value.Equals(value))
                return;
            this.value = value;
            if (OnValueChanged != null)
                OnValueChanged.Invoke(value);
            source.NotifyUpdate();
        }

        /// <summary>
        /// Set the property's value for a given user.
        /// If synchronized, the default/synchronized value is setted.
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="value">the new property's value</param>
        public void SetValue(UMI3DUser user,T value)
        {
            if (isAsync)
            {
                if (asyncValues.ContainsKey(user))
                {
                    if (asyncValues[user] == null && value == null || asyncValues[user].Equals(value))
                        return ;
                    else
                    {
                        asyncValues[user] = value;
                        source.NotifyUpdate(user);
                    }
                }
                else
                {
                    asyncValues.Add(user, value);
                    source.NotifyUpdate(user);
                }
            }
            else
                SetValue(value);
        }

        /// <summary>
        /// Set the property as synchronized/asynchronous.
        /// </summary>
        /// <param name="isSync">Is the property async ?</param>
        public void Sync(bool isSync)
        {
            if (isAsync && isSync)
                source.NotifyUpdate();
            isAsync = !isSync;
            if (isSync)
            {
                asyncValues.Clear();
            }
        }

    }
}
