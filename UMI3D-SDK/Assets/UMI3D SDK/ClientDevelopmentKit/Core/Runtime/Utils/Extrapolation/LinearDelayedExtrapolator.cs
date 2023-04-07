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

namespace umi3d.cdk.utils.extrapolation
{
    /// <summary>
    /// Abstract base for extrapolators that linearly regress values with one tracking frame of delay.
    /// </summary>*
    /// This frame of delay ensure smooth movements.
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractLinearDelayedExtrapolator<T> : AbstractExtrapolator<T>
    {
        /// <summary>
        /// Last measure given to the extrapolator.
        /// </summary>
        protected T lastMeasure;

        /// <summary>
        /// Measure given to the extrapolator before <see cref="lastMeasure"/>.
        /// </summary>
        protected T secondLastMeasure;

        /// <summary>
        /// Last regressed state.
        /// </summary>
        protected T lastEstimation;

        /// <summary>
        /// Time in the client's clock of the <see cref="secondLastMeasure"/>.
        /// </summary>
        private float secondLastMessageTime;

        /// <inheritdoc/>
        public override void AddMeasure(T measure)
        {
            AddMeasure(measure, Time.time);
        }

        /// <summary>
        /// Alternative to <see cref="AddMeasure(T)"/> with time parameter.
        /// </summary>
        /// <param name="measure"></param>
        /// <param name="time"></param>
        public void AddMeasure(T measure, float time)
        {
            if (time < lastUpdateTime)
                return;

            if (IsInited)
                lastEstimation = Extrapolate();
            else
                lastEstimation = measure;

            secondLastMeasure = lastMeasure;
            lastMeasure = measure;

            if (lastUpdateTime > 0)
                updateFrequency = 1 / (time - lastUpdateTime);

            secondLastMessageTime = lastUpdateTime;
            lastUpdateTime = time;
        }

        /// <inheritdoc/>
        public override bool IsInited => (lastUpdateTime != 0) && (secondLastMessageTime != 0);
    }

    /// <summary>
    /// Extrapolator using a linear extrapolation regression system, specialised for <see cref="float"/> computing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FloatLinearDelayedExtrapolator : AbstractLinearDelayedExtrapolator<float>
    {
        /// <inheritdoc/>
        public override float Extrapolate()
        {
            var t = (Time.time - lastUpdateTime) * updateFrequency;
            if (t > 0 && IsInited)
                return t < 1 ? lastEstimation + (lastMeasure - lastEstimation) * t : lastMeasure; //clamped lerp
            else
                return lastEstimation;
        }
    }

    /// <summary>
    /// Extrapolator using a linear extrapolation regression system, specialised for <see cref="Vector3"/> computing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Vector3LinearDelayedExtrapolator : AbstractLinearDelayedExtrapolator<Vector3>
    {
        /// <inheritdoc/>
        public override Vector3 Extrapolate()
        {
            var t = (Time.time - lastUpdateTime) * updateFrequency;
            if (t > 0 && IsInited)
                return Vector3.Lerp(lastEstimation, lastMeasure, t);
            else
                return lastEstimation;
        }
    }

    /// <summary>
    /// Extrapolator using a linear extrapolation regression system, specialised for <see cref="Quaternion"/> computing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuaternionLinearDelayedExtrapolator : AbstractLinearDelayedExtrapolator<Quaternion>
    {
        /// <inheritdoc/>
        public override Quaternion Extrapolate()
        {
            var t = (Time.time - lastUpdateTime) * updateFrequency;
            if (t > 0 && IsInited)
                return Quaternion.Lerp(lastEstimation, lastMeasure, t);
            else
                return lastEstimation;
        }
    }
}