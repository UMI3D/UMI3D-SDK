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

using UnityEngine;
using UnityEngine.Events;

namespace umi3d.common
{

    /// <summary>
    /// Wrap a value with updates subscriptions.
    /// </summary>
    [System.Serializable]
    public class ObservableProperty<ValueType>
    {
        [SerializeField] protected ValueType internalValue;

        private class EventType : UnityEvent<ValueType> { }
        private EventType onUpdate = new EventType();

        public ObservableProperty(ValueType value)
        {
            internalValue = value;
        }
        public ObservableProperty() { }

        public void SetValue(ValueType value, bool notifyUpdate = true)
        {
            internalValue = value;
            if (notifyUpdate)
                onUpdate.Invoke(value);
        }

        public ValueType GetValue() => internalValue;

        public void ForceNotification() => onUpdate.Invoke(internalValue);

        /// <summary>
        /// Subscribe to value change updates.
        /// </summary>
        public void Attach(UnityAction<ValueType> callback) => onUpdate.AddListener(callback);

        /// <summary>
        /// Unsubscribe to value change updates.
        /// </summary>
        public void Detach(UnityAction<ValueType> callback) => onUpdate.RemoveListener(callback);
    }

    [System.Serializable]
    public class BoolObservable : ObservableProperty<bool>
    {
        public BoolObservable(bool newValue)
        {
            internalValue = newValue;
        }
    }
    [System.Serializable]
    public class BoundsObservable : ObservableProperty<Bounds>
    {
        public BoundsObservable(Bounds newValue)
        {
            internalValue = newValue;
        }
    }

}