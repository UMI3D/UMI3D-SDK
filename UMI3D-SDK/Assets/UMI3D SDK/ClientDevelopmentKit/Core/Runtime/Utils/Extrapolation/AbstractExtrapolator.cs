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

namespace umi3d.cdk.utils.extrapolation
{
    /// <summary>
    /// Extrapolator that allows to predict futures values.
    /// </summary>
    public interface IExtrapolator
    {
        /// <summary>
        /// Is the extrapolator able to extrapolate ?
        /// </summary>
        /// It usually need two measures before being able to predict.
        public bool IsInited { get; }

        /// <summary>
        /// Current value linearly regressed from the prediction.
        /// </summary>
        /// <returns></returns>
        public object ExtrapolatedValue { get; }

        /// <summary>
        /// Compute the current regressed state according to the last prediction made by the extrapolator.
        /// </summary>
        /// Call <see cref="GetRegressedValue"/> to get the updated value.
        public void ComputeExtrapolatedValue();

        /// <summary>
        /// Add a measure to the extrapolator. This will change the prediction made.
        /// </summary>
        /// <param name="measure"></param>
        public void AddMeasure(object measure);
    }

    /// <summary>
    /// Extrapolator that allows to predict futures values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractExtrapolator<T> : IExtrapolator
    {
        /// <summary>
        /// Frequency of updates.
        /// </summary>
        protected float updateFrequency;

        /// <summary>
        /// Last received measure time in the browser clock.
        /// </summary>
        protected float lastUpdateTime = 0;

        /// <summary>
        /// Current regressed value from the extrapolator.
        /// </summary>
        /// Needs to ve updated each frame through <see cref="ComputeExtrapolatedValue"/>.
        protected T _extrapolatedValue;

        /// <summary>
        /// Get the current regressed value from the extrapolator.
        /// Always call <see cref="ComputeExtrapolatedValue"/> before calling this method during a same frame.
        /// </summary>
        /// <returns></returns>
        public object ExtrapolatedValue => _extrapolatedValue;

        /// <summary>
        /// Is the extrapolator able to extrapolate ?
        /// </summary>
        /// It usually need two measures before being able to predict.
        public abstract bool IsInited { get; }

        /// <summary>
        /// Predicts an extrapolated state at the current time.
        /// Does not change the state of the extrapolator.
        /// </summary>
        /// Use this method only if you are adding measures
        /// at the same time that you are making predictions.
        /// <returns></returns>
        public abstract T Extrapolate();

        /// <inheritdoc/>
        public void ComputeExtrapolatedValue()
        {
            _extrapolatedValue = Extrapolate();
        }

        /// <summary>
        /// Typed alternative to <see cref="AddMeasure(object)"/>.
        /// </summary>
        /// <param name="measure"></param>
        public abstract void AddMeasure(T measure);

        /// <summary>
        /// Add a measure to the extrapolator. This will change the prediction made.
        /// </summary>
        /// Also automatically types the object according to <see cref="T"/>.
        /// <param name="measure"></param>
        public void AddMeasure(object measure)
        {
            AddMeasure((T)measure);
        }
    }
}