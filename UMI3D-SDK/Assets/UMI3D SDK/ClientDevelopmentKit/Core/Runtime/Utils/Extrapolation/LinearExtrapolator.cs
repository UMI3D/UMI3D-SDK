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
    /// Extrapolator using a linear extrapolation prediction system, base template.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractLinearExtrapolator<T> : AbstractExtrapolator<T>
    {
        /// <summary>
        /// Previous extrapolated prediction made by the extrapolator.
        /// </summary>
        protected T previous_prediction;

        /// <summary>
        /// Last extrapolated prediction made by the extrapolator.
        /// </summary>
        protected T prediction;

        /// <summary>
        /// Last regressed state.
        /// </summary>
        protected T lastEstimation;

        /// <summary>
        /// Last measure given to the extrapolator.
        /// </summary>
        protected T lastMeasure;

        /// <summary>
        /// Is the extrapolator able to extrapolate ?
        /// </summary>
        /// It usually need two measures before being able to predict.
        protected bool isInited;

        /// <inheritdoc/>
        public override void AddMeasure(T measure)
        {
            AddMeasure(measure, Time.time);
        }

        /// <summary>
        /// See <see cref="AddMeasure(T)"/>.
        /// </summary>
        /// <param name="measure"></param>
        /// <param name="time"></param>
        public virtual void AddMeasure(T measure, float time)
        {
            if (time <= lastMessageTime)
                return;

            if (!isInited) //requires two measures to be inited
            {
                prediction = measure;
                isInited = true;
            }

            lastEstimation = ExtrapolateState();
            updateFrequency = 1f / (time - lastMessageTime);

            Predict(measure, time);
            lastMeasure = measure;
            lastMessageTime = time;
        }

        /// <summary>
        /// See <see cref="AddMeasure(T)"/>.
        /// </summary>
        /// <param name="measure"></param>
        /// <param name="time"></param>
        public void AddMeasure(T measure, float time, float measurePerSeconds)
        {
            this.updateFrequency = measurePerSeconds;
            AddMeasure(measure, time);
        }

        /// <summary>
        /// Update prediction.
        /// </summary>
        public abstract void Predict(T measure, float time);

        /// <inheritdoc/>
        public override bool IsInited() => isInited;
    }

    /// <summary>
    /// Extrapolator using a linear extrapolation prediction system, specialised for <see cref="float"/> computing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FloatLinearExtrapolator : AbstractLinearExtrapolator<float>
    {
        /// <inheritdoc/>
        public override void Predict(float measure, float time)
        {
            previous_prediction = prediction;
            var t = 1f + (time - lastMessageTime) * updateFrequency;
            t = t > 0 ? t : 0;
            prediction = measure + (measure - lastMeasure) * t;
        }

        /// <inheritdoc/>
        public override float ExtrapolateState()
        {
            var t = (Time.time - lastMessageTime) * updateFrequency;
            if (t > 0 && isInited && !lastEstimation.Equals(prediction))
                return lastEstimation + (prediction - lastEstimation) * t;
            else
                return lastEstimation;
        }
    }

    /// <summary>
    /// Extrapolator using a linear extrapolation prediction system, specialised for <see cref="Vector3"/> computing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Vector3LinearExtrapolator : AbstractLinearExtrapolator<Vector3>
    {
        /// <inheritdoc/>
        public override void Predict(Vector3 measure, float time)
        {
            previous_prediction = prediction;
            var t = 1f + (time - lastMessageTime) * updateFrequency;
            t = t > 0 ? t : 0;
            prediction = Vector3.LerpUnclamped(measure, measure + (measure - lastMeasure), t);
        }

        /// <inheritdoc/>
        public override Vector3 ExtrapolateState()
        {
            var t = (Time.time - lastMessageTime) * updateFrequency;
            if (t > 0 && isInited && !lastEstimation.Equals(prediction))
                return Vector3.Lerp(lastEstimation, prediction, t);
            else
                return lastEstimation;
        }
    }

    /// <summary>
    /// Extrapolator using a linear extrapolation prediction system, specialised for <see cref="Quaternion"/> computing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuaternionLinearExtrapolator : AbstractLinearExtrapolator<Quaternion>
    {
        /// <inheritdoc/>
        public override void Predict(Quaternion measure, float time)
        {
            previous_prediction = prediction;
            var t = 1f + (time - lastMessageTime) * updateFrequency;
            t = t > 0 ? t : 0;
            prediction = Quaternion.LerpUnclamped(measure, measure * (measure * Quaternion.Inverse(lastMeasure)), t);
        }

        /// <inheritdoc/>
        public override Quaternion ExtrapolateState()
        {
            var t = (Time.time - lastMessageTime) * updateFrequency;
            if (t > 0 && isInited && !lastEstimation.Equals(prediction))
                return Quaternion.Lerp(lastEstimation, prediction, t);
            else
                return lastEstimation;
        }
    }
}