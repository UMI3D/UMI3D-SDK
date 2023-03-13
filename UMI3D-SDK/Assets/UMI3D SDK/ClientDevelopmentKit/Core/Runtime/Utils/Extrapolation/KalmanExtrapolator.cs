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

namespace umi3d.cdk.utils.extrapolation.kalman
{
    /// <summary>
    /// Interface for Kalman filter based objects.
    /// </summary>
    public interface IKalman
    {
        /// <summary>
        /// Is the kalman-filter based object able to make predictions?
        /// </summary>
        /// <returns></returns>
        bool IsInited { get; }

        /// <summary>
        /// Add a measure to the kalman-filter based object and correct the prediction onto an estimation.
        /// </summary>
        void Update(object measure);
    }

    /// <summary>
    /// Extrapolator using a Kalman filter prediction system, specialised for <see cref="float"/> computing.
    /// </summary>
    public class FloatKalmanExtrapolator : FloatLinearExtrapolator, IKalman
    {
        /// <summary>
        /// Associated Kalman filter
        /// </summary>
        protected UMI3DUnscentedKalmanFilter filter;

        public FloatKalmanExtrapolator(double q, double r)
        {
            filter = new UMI3DUnscentedKalmanFilter(q, r);
        }

        /// <inheritdoc/>
        public override void AddMeasure(float measure)
        {
            previous_prediction = prediction;
            filter.Update(new double[] { measure });
            Predict(measure, Time.time);
        }

        /// <inheritdoc/>
        public override void Predict(float measure, float time)
        {
            previous_prediction = prediction;

            if (!isInited)
                prediction = (float)filter.getState()[0];
            else
                prediction = measure;

        }

        /// <inheritdoc/>
        public void Update(object measure) => AddMeasure(measure);
    }

    /// <summary>
    /// Extrapolator using a Kalman filter prediction system, specialised for <see cref="Vector3"/> computing.
    /// </summary>
    public class Vector3KalmanExtrapolator : Vector3LinearExtrapolator, IKalman
    {
        /// <summary>
        /// Associated Kalman filter
        /// </summary>
        protected UMI3DUnscentedKalmanFilter filter;

        public Vector3KalmanExtrapolator(double q, double r)
        {
            filter = new UMI3DUnscentedKalmanFilter(q, r);
        }

        /// <inheritdoc/>
        public override void AddMeasure(Vector3 measure)
        {
            base.AddMeasure(measure);
            filter.Update(new double[] { measure.x, measure.y, measure.z });
            Predict(measure, Time.time);
        }

        /// <inheritdoc/>
        public override void AddMeasure(Vector3 measure, float time)
        {
            base.AddMeasure(measure, time);
            filter.Update(new double[] { measure.x, measure.y, measure.z });
            Predict(measure, time);
        }

        /// <inheritdoc/>
        public override void Predict(Vector3 measure, float time)
        {
            previous_prediction = prediction;
            if (!isInited)
            {
                var vec = filter.getState();
                prediction = new Vector3((float)vec[0], (float)vec[1], (float)vec[2]);
            }
            else
                prediction = measure;

        }

        /// <inheritdoc/>
        public void Update(object measure) => AddMeasure(measure);
    }

    /// <summary>
    /// Extrapolator using a Kalman filter prediction system, specialised for <see cref="Quaternion"/> computing.
    /// </summary>
    public class QuaternionKalmanExtrapolator : QuaternionLinearExtrapolator, IKalman
    {
        /// <summary>
        /// Kalman filter for rotation, handling the forward vector part.
        /// </summary>
        protected UMI3DUnscentedKalmanFilter filterForward;

        /// <summary>
        /// Kalman filter for rotation, handling the up vector part.
        /// </summary>
        protected UMI3DUnscentedKalmanFilter filterUp;

        public QuaternionKalmanExtrapolator(double q, double r)
        {
            filterForward = new UMI3DUnscentedKalmanFilter(q, r);
            filterUp = new UMI3DUnscentedKalmanFilter(q, r);
        }

        /// <inheritdoc/>
        public override void AddMeasure(Quaternion measure)
        {
            base.AddMeasure(measure);
            var forward = measure * Vector3.forward;
            var up = measure * Vector3.up;

            filterForward.Update(new double[] { forward[0], forward[1], forward[2] });
            filterUp.Update(new double[] { up[0], up[1], forward[2] });
            Predict(measure, Time.time);
        }

        /// <inheritdoc/>
        public override void Predict(Quaternion measure, float time)
        {
            previous_prediction = prediction;

            if (!isInited)
            {
                var stateForward = filterForward.getState();
                var stateUp = filterUp.getState();
                prediction = Quaternion.LookRotation(new Vector3((float)stateForward[0], (float)stateForward[1], (float)stateForward[2]),
                                                new Vector3((float)stateUp[0], (float)stateUp[1], (float)stateUp[2]));
            }
            else
                prediction = measure;
        }

        /// <inheritdoc/>
        public void Update(object measure) => AddMeasure(measure);
    }
}