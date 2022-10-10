/*
Copyright 2019 - 2022 Inetum

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

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Perform form values regression based on linear regresion.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Obsolete]
    public abstract class UMI3DKalmanLerp<T>
    {
        #region Fields

        /// <summary>
        /// Previous value received.
        /// </summary>
        protected T lastValue;

        /// <summary>
        /// Time when <see cref="lastValue"/> was set.
        /// </summary>
        protected float lastValueTime;

        /// <summary>
        /// New value received.
        /// </summary>
        protected T newValue;

        /// <summary>
        /// Time when <see cref="newValue"/> was received.
        /// </summary>
        protected float newValueTime;

        /// <summary>
        /// Duration between <see cref="newValue"/> and <see cref="lastValue"/>.
        /// </summary>
        protected float duration;

        /// <summary>
        /// Time spending interpolating between <see cref="lastValue"/> and <see cref="newValue"/>.
        /// </summary>
        protected float time;

        /// <summary>
        /// Value computed by <see cref="GetValue(float)"/>.
        /// </summary>
        protected T res;

        #endregion

        public UMI3DKalmanLerp(T initValue)
        {
            lastValue = initValue;
            newValue = initValue;
            duration = Mathf.Infinity;
            lastValueTime = Time.time;
        }

        /// <summary>
        /// Updates <see cref="newValue"/>.
        /// </summary>
        /// <param name="newValue"></param>
        public void UpdateValue(T newValue)
        {
            lastValueTime = newValueTime;
            newValueTime = Time.time;
            duration = (newValueTime - lastValueTime);

            lastValue = this.newValue;
            this.newValue = newValue;
            time = 0;
        }

        /// <summary>
        /// Returns an interpolation between <see cref="lastValue"/> and <see cref="newValue"/>.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public abstract T GetValue(float deltaTime);
    }

    /// <summary>
    /// Lerp Helper for <see cref="Quaternion"/>.
    /// </summary>
    [System.Obsolete]
    public class UMI3DKalmanQuaternionLerp : UMI3DKalmanLerp<Quaternion>
    {
        public UMI3DKalmanQuaternionLerp(Quaternion initValue) : base(initValue)
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public override Quaternion GetValue(float deltaTime)
        {
            res = Quaternion.Lerp(lastValue, newValue, Mathf.Min(time / duration, 1));
            time += deltaTime;
            return res;
        }
    }

    /// <summary>
    /// Lerp Helper <see cref="Vector3"/>.
    /// </summary>
    [System.Obsolete]
    public class UMI3DKalmanVector3Lerp : UMI3DKalmanLerp<Vector3>
    {
        public UMI3DKalmanVector3Lerp(Vector3 initValue) : base(initValue)
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public override Vector3 GetValue(float deltaTime)
        {
            res = Vector3.Lerp(lastValue, newValue, Mathf.Min(time / duration, 1));
            time += deltaTime;
            return res;
        }
    }
}