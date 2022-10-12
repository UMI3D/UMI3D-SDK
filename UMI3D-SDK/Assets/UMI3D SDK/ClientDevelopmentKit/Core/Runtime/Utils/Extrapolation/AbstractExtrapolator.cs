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
    public abstract class AbstractExtrapolator
    {
        /// <summary>
        /// Frequency of updates.
        /// </summary>
        public float updateFrequency;

        /// <summary>
        /// Last received received time in the browser clock.
        /// </summary>
        public float lastMessageTime = 0;

        /// <summary>
        /// Id of the entity that has an extrapolated property.
        /// </summary>
        public ulong entityId;

        /// <summary>
        /// Id of the extrapolated property.
        /// </summary>
        public ulong property;

        /// <summary>
        /// Is the extrapolator able to extrapolate ?
        /// </summary>
        /// It usually need two measures before being able to predict.
        public abstract bool IsInited();

        /// <summary>
        /// Current value linearly regressed from the prediction.
        /// </summary>
        /// <returns></returns>
        public abstract object GetRegressedValue();

        /// <summary>
        /// Compute the current regressed state according to the last prediction made by the extrapolator.
        /// </summary>
        /// Call <see cref="GetRegressedValue"/> to get the updated value.
        public abstract void UpdateRegressedValue();

        /// <summary>
        /// Add a measure to the extrapolator. This will change the prediction made.
        /// </summary>
        /// <param name="measure"></param>
        public abstract void AddMeasure(object measure);
    }

    /// <summary>
    /// Extrapolator that allows to predict futures values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractExtrapolator<T> : AbstractExtrapolator
    {
        /// <summary>
        /// Current regressed value from the extrapolator.
        /// </summary>
        /// Needs to ve updated each frame through
        protected T regressed_value;

        /// <summary>
        /// Get the current regressed value from the extrapolator.
        /// Always call <see cref="UpdateRegressedValue"/> before calling this method during a same frame.
        /// </summary>
        /// <returns></returns>
        public override object GetRegressedValue() => regressed_value;

        /// <summary>
        /// Compute the current extrapolated state according to the last prediction made by the extrapolator.
        /// </summary>
        /// <returns></returns>
        public abstract T ExtrapolateState();

        /// <inheritdoc/>
        public override void UpdateRegressedValue()
        {
            regressed_value = ExtrapolateState();
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
        public override void AddMeasure(object measure)
        {
            AddMeasure((T)measure);
        }
    }
}