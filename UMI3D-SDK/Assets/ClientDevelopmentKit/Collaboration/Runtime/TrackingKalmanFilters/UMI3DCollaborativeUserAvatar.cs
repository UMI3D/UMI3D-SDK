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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class UMI3DCollaborativeUserAvatar : UserAvatar
    {
        private class KalmanPosition
        {
            public UKF KalmanFilter;
            public List<double[]> Measurements;
            public List<double[]> Estimations;
            public double[] previous_prediction;
            public double[] prediction;
            public Vector3 regressed_position;

            public KalmanPosition(double q, double r)
            {
                KalmanFilter = new UKF(q, r);
                Measurements = new List<double[]>();
                Estimations = new List<double[]>();
                previous_prediction = new double[3];
                prediction = new double[3];
            }
        }

        private class KalmanRotation
        {
            public UKF KalmanFilter = new UKF();
            public List<double[]> Measurements = new List<double[]>();
            public List<double[]> Estimations = new List<double[]>();
            public double[] previous_prediction = new double[3];
            public double[] prediction = new double[3];
            public Vector4 regressed_rotation;

            public KalmanRotation(double q, double r)
            {
                KalmanFilter = new UKF(q, r);
                Measurements = new List<double[]>();
                Estimations = new List<double[]>();
                previous_prediction = new double[3];
                prediction = new double[3];
            }

            public Quaternion RegressedQuaternion()
            {
                return Quaternion.Euler(regressed_rotation.x, regressed_rotation.y, regressed_rotation.z);
            }

        }

        KalmanPosition nodePositionFilter = new KalmanPosition(50, 0.5);
        KalmanRotation nodeRotationFilter = new KalmanRotation(10, 0.5);
        Vector3 scale = Vector3.one;

        Dictionary<BoneDto, KalmanPosition> bonePositionFilters = new Dictionary<BoneDto, KalmanPosition>();
        Dictionary<BoneDto, KalmanRotation> boneRotationFilters = new Dictionary<BoneDto, KalmanRotation>();

        public float MeasuresPerSecond = 5;

        float lastMessageTime = 0f;

        private void Update()
        {
            RegressionPosition(nodePositionFilter);
            RegressionRotation(nodeRotationFilter);

            this.transform.localPosition = nodePositionFilter.regressed_position;
            this.transform.localRotation = nodeRotationFilter.RegressedQuaternion();

            foreach (BoneDto boneDto in bonePositionFilters.Keys)
            {
                RegressionPosition(bonePositionFilters[boneDto]);
                RegressionRotation(boneRotationFilters[boneDto]);

                List<BoneBindingDto> bindings = userBindings.FindAll(binding => binding.boneType == boneDto.boneType);
                foreach (BoneBindingDto boneBindingDto in bindings)
                {
                    if (boneBindingDto.active && savedTransforms.ContainsKey(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }))
                    {
                        SavedTransform st = savedTransforms[new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }];
                        st.obj.position = Matrix4x4.TRS(nodePositionFilter.regressed_position, nodeRotationFilter.RegressedQuaternion(), scale).MultiplyPoint3x4(bonePositionFilters[boneDto].regressed_position + (Vector3)boneBindingDto.position * boneDto.scale.X);
                        st.obj.rotation = nodeRotationFilter.RegressedQuaternion() * boneRotationFilters[boneDto].RegressedQuaternion() * (Quaternion)boneBindingDto.rotation;
                    }
                }
            }
        }

        /// <summary>
        /// Applies linear regression to the filtered position of an object
        /// </summary>
        /// <param name="tools"></param>
        void RegressionPosition(KalmanPosition tools)
        {
            if (tools.previous_prediction.Length > 0)
            {
                double check = lastMessageTime;
                double now = Time.time;

                var delta = now - check;

                var value_x = (tools.prediction[0] - tools.previous_prediction[0]) * MeasuresPerSecond * delta + tools.previous_prediction[0];
                var value_y = (tools.prediction[1] - tools.previous_prediction[1]) * MeasuresPerSecond * delta + tools.previous_prediction[1];
                var value_z = (tools.prediction[2] - tools.previous_prediction[2]) * MeasuresPerSecond * delta + tools.previous_prediction[2];

                tools.Estimations.Add(new double[] { value_x, value_y, value_z });
                tools.regressed_position = new Vector3((float)value_x, (float)value_y, (float)value_z);
            }
        }

        /// <summary>
        /// Applies linear regression to the filtered rotation of an object
        /// </summary>
        /// <param name="tools"></param>
        void RegressionRotation(KalmanRotation tools)
        {
            if (tools.previous_prediction.Length > 0)
            {
                double check = lastMessageTime;
                double now = Time.time;

                var delta = now - check;

                var value_x = (tools.prediction[0] - tools.previous_prediction[0]) * MeasuresPerSecond * delta + tools.previous_prediction[0];
                var value_y = (tools.prediction[1] - tools.previous_prediction[1]) * MeasuresPerSecond * delta + tools.previous_prediction[1];
                var value_z = (tools.prediction[2] - tools.previous_prediction[2]) * MeasuresPerSecond * delta + tools.previous_prediction[2];

                tools.Estimations.Add(new double[] { value_x, value_y, value_z });
                tools.regressed_rotation = new Vector4((float)value_x, (float)value_y, (float)value_z);
            }
        }

        /// <summary>
        /// Filtering a boneDto position
        /// </summary>
        /// <param name="dto"></param>
        void BoneKalmanUpdate(BoneDto dto)
        {
            KalmanPosition bonePositionKalman = bonePositionFilters[dto];
            KalmanRotation boneRotationKalman = boneRotationFilters[dto];

            double[] bonePositionMeasurement = new double[] { dto.position.X, dto.position.Y, dto.position.Z };
            bonePositionKalman.KalmanFilter.Update(bonePositionMeasurement);
            bonePositionKalman.Measurements.Add(bonePositionMeasurement);

            double[] newPositionState = bonePositionKalman.KalmanFilter.getState();
            bonePositionKalman.prediction = new double[] { newPositionState[0], newPositionState[1], newPositionState[2] };

            if (bonePositionKalman.Estimations.Count > 0)
                bonePositionKalman.previous_prediction = bonePositionKalman.Estimations[bonePositionKalman.Estimations.Count - 1];
            else
                bonePositionKalman.previous_prediction = bonePositionMeasurement;


            Vector3 eulerRotation = (new Quaternion(dto.rotation.X, dto.rotation.Y, dto.rotation.Z, dto.rotation.W)).eulerAngles;
            double[] boneRotationMeasurement = new double[] { eulerRotation.x, eulerRotation.y, eulerRotation.z };
            boneRotationKalman.KalmanFilter.Update(boneRotationMeasurement);
            boneRotationKalman.Measurements.Add(boneRotationMeasurement);

            double[] newRotationState = boneRotationKalman.KalmanFilter.getState();
            boneRotationKalman.prediction = new double[] { newRotationState[0], newRotationState[1], newRotationState[2] };

            if (boneRotationKalman.Estimations.Count > 0)
                boneRotationKalman.previous_prediction = boneRotationKalman.Estimations[boneRotationKalman.Estimations.Count - 1];
            else
                boneRotationKalman.previous_prediction = boneRotationMeasurement;
        }

        /// <summary>
        /// Filtering a user AvatarNode position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        void NodeKalmanUpdate(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            double[] positionMeasurement = new double[] { position.x, position.y, position.z };
            nodePositionFilter.KalmanFilter.Update(positionMeasurement);
            nodePositionFilter.Measurements.Add(positionMeasurement);

            double[] newPositionState = nodePositionFilter.KalmanFilter.getState();
            nodePositionFilter.prediction = new double[] { newPositionState[0], newPositionState[1], newPositionState[2] };

            if (nodePositionFilter.Estimations.Count > 0)
                nodePositionFilter.previous_prediction = nodePositionFilter.Estimations[nodePositionFilter.Estimations.Count - 1];
            else
                nodePositionFilter.previous_prediction = positionMeasurement;

            Vector3 eulerRotation = rotation.eulerAngles;
            double[] rotationMeasurement = new double[] { eulerRotation.x, eulerRotation.y, eulerRotation.z };
            nodeRotationFilter.KalmanFilter.Update(rotationMeasurement);
            nodeRotationFilter.Measurements.Add(rotationMeasurement);

            double[] newRotationState = nodeRotationFilter.KalmanFilter.getState();

            nodeRotationFilter.prediction = new double[] { newRotationState[0], newRotationState[1], newRotationState[2] };

            if (nodeRotationFilter.Estimations.Count > 0)
                nodeRotationFilter.previous_prediction = nodeRotationFilter.Estimations[nodeRotationFilter.Estimations.Count - 1];
            else
                nodeRotationFilter.previous_prediction = rotationMeasurement;

            this.scale = scale;
        }


        /// <summary>
        /// Update the a UserAvatar directly sent by another client.
        /// </summary>
        /// <param name="trackingFrameDto">a dto containing the tracking data</param>
        public IEnumerator UpdateBonePosition(UserTrackingFrameDto trackingFrameDto)
        {
            float now = Time.time;
            MeasuresPerSecond = 1 / (now - lastMessageTime);
            lastMessageTime = now;

            NodeKalmanUpdate(trackingFrameDto.position, trackingFrameDto.rotation, trackingFrameDto.scale);

            foreach (BoneDto boneDto in trackingFrameDto.bones)
            {
                if (!bonePositionFilters.ContainsKey(boneDto))
                    bonePositionFilters.Add(boneDto, new KalmanPosition(0.001f, 0.001f));

                if (!boneRotationFilters.ContainsKey(boneDto))
                    boneRotationFilters.Add(boneDto, new KalmanRotation(0.001f, 0.001f));

                BoneKalmanUpdate(boneDto);

                List<BoneBindingDto> bindings = userBindings.FindAll(binding => binding.boneType == boneDto.boneType);
                foreach (BoneBindingDto boneBindingDto in bindings)
                {

                    if (boneBindingDto.active)
                    {
                        UMI3DNodeInstance node;
                        var wait = new WaitForFixedUpdate();

                        while ((node = UMI3DEnvironmentLoader.GetNode(boneBindingDto.objectId)) == null)
                        {
                            yield return wait;
                        }

                        Transform obj = null;
                        if (boneBindingDto.rigName != "")
                        {
                            while ((obj = UMI3DEnvironmentLoader.GetNode(boneBindingDto.objectId).transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == boneBindingDto.rigName)) == null && (obj = InspectBoundRigs(boneBindingDto)) == null)
                            {
                                yield return wait;
                            }

                            if (!boundRigs.Contains(obj))
                                boundRigs.Add(obj);
                        }
                        else
                            obj = node.transform;

                        if (!savedTransforms.ContainsKey(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }))
                        {
                            SavedTransform savedTransform = new SavedTransform
                            {
                                obj = obj,
                                savedParent = obj.parent,
                                savedPosition = obj.localPosition,
                                savedRotation = obj.localRotation
                            };

                            savedTransforms.Add(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }, savedTransform);

                            obj.transform.SetParent(UMI3DEnvironmentLoader.Instance.transform);

                            if (boneBindingDto.rigName == "")
                                node.updatePose = false;

                        }

                        if (boneBindingDto.rigName == "")
                        {
                            node.updatePose = false;
                        }

                    }
                }
            }
        }
    }
}