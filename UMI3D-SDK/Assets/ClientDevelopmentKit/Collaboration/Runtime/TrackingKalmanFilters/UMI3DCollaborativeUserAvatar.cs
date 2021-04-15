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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class UMI3DCollaborativeUserAvatar : UserAvatar
    {
        private class KalmanPosition
        {
            public UMI3DUnscentedKalmanFilter KalmanFilter;
            public double[] estimations;
            public double[] previous_prediction;
            public double[] prediction;
            public Vector3 regressed_position;

            public KalmanPosition(double q, double r)
            {
                KalmanFilter = new UMI3DUnscentedKalmanFilter(q, r);
                estimations = new double[] { };
                previous_prediction = new double[] { };
                prediction = new double[] { };
            }
        }

        private class KalmanRotation
        {
            public UMI3DUnscentedKalmanFilter KalmanFilter = new UMI3DUnscentedKalmanFilter();
            public double[] estimations;
            public double[] previous_prediction;
            public double[] prediction;
            public Vector3 regressed_rotation;

            public KalmanRotation(double q, double r)
            {
                KalmanFilter = new UMI3DUnscentedKalmanFilter(q, r);
                estimations = new double[] { };
                previous_prediction = new double[] { };
                prediction = new double[] { };
            }

            public Quaternion RegressedQuaternion()
            {
                return Quaternion.Euler(regressed_rotation.x, regressed_rotation.y, regressed_rotation.z);
            }

        }

        KalmanPosition nodePositionFilter = new KalmanPosition(50, 0.5);
        KalmanRotation nodeRotationFilter = new KalmanRotation(10, 0.5);
        Vector3 scale = Vector3.one;

        Dictionary<string, KalmanPosition> bonePositionFilters = new Dictionary<string, KalmanPosition>();
        Dictionary<string, KalmanRotation> boneRotationFilters = new Dictionary<string, KalmanRotation>();
        Dictionary<string, Vector3> boneScales = new Dictionary<string, Vector3>();

        public float MeasuresPerSecond = 0;

        float lastFrameTime = 0;
        float lastMessageTime = 0;

        private void Update()
        {
            RegressionPosition(nodePositionFilter);
            RegressionRotation(nodeRotationFilter);

            this.transform.localPosition = nodePositionFilter.regressed_position;
            this.transform.localRotation = nodeRotationFilter.RegressedQuaternion();

            foreach (string boneType in bonePositionFilters.Keys)
            {
                RegressionPosition(bonePositionFilters[boneType]);
                RegressionRotation(boneRotationFilters[boneType]);

                List<BoneBindingDto> bindings = userBindings.FindAll(binding => binding.boneType == boneType);
                foreach (BoneBindingDto boneBindingDto in bindings)
                {
                    if (boneBindingDto.active && savedTransforms.ContainsKey(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }))
                    {
                        SavedTransform st = savedTransforms[new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }];
                        Vector3 boneposition = Matrix4x4.TRS(nodePositionFilter.regressed_position, nodeRotationFilter.RegressedQuaternion(), scale).MultiplyPoint3x4(bonePositionFilters[boneType].regressed_position);
                        st.obj.position = Matrix4x4.TRS(boneposition, nodeRotationFilter.RegressedQuaternion() * boneRotationFilters[boneType].RegressedQuaternion(), Vector3.Scale(this.scale, boneScales[boneType])).MultiplyPoint3x4((Vector3)boneBindingDto.position);
                        st.obj.rotation = nodeRotationFilter.RegressedQuaternion() * boneRotationFilters[boneType].RegressedQuaternion() * (Quaternion)boneBindingDto.rotation;
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

                double delta = now - check;

                if (delta * MeasuresPerSecond <= 1)
                {
                    var value_x = (tools.prediction[0] - tools.previous_prediction[0]) * delta * MeasuresPerSecond + tools.previous_prediction[0];
                    var value_y = (tools.prediction[1] - tools.previous_prediction[1]) * delta * MeasuresPerSecond + tools.previous_prediction[1];
                    var value_z = (tools.prediction[2] - tools.previous_prediction[2]) * delta * MeasuresPerSecond + tools.previous_prediction[2];

                    tools.estimations = new double[] { value_x, value_y, value_z };

                    tools.regressed_position = new Vector3((float)value_x, (float)value_y, (float)value_z);
                }
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

                if (delta * MeasuresPerSecond <= 1)
                {
                    var value_x = (tools.prediction[0] - tools.previous_prediction[0]) * MeasuresPerSecond * delta + tools.previous_prediction[0];
                    var value_y = (tools.prediction[1] - tools.previous_prediction[1]) * MeasuresPerSecond * delta + tools.previous_prediction[1];
                    var value_z = (tools.prediction[2] - tools.previous_prediction[2]) * MeasuresPerSecond * delta + tools.previous_prediction[2];

                    tools.estimations = new double[] { value_x, value_y, value_z };

                    tools.regressed_rotation = new Vector3((float)value_x, (float)value_y, (float)value_z);
                }
            }
        }

        /// <summary>
        /// Filtering a boneDto position
        /// </summary>
        /// <param name="dto"></param>
        void BoneKalmanUpdate(BoneDto dto)
        {
            KalmanPosition bonePositionKalman = bonePositionFilters[dto.boneType];
            KalmanRotation boneRotationKalman = boneRotationFilters[dto.boneType];

            double[] bonePositionMeasurement = new double[] { dto.position.X, dto.position.Y, dto.position.Z };
            bonePositionKalman.KalmanFilter.Update(bonePositionMeasurement);

            double[] newPositionState = bonePositionKalman.KalmanFilter.getState();
            bonePositionKalman.prediction = new double[] { newPositionState[0], newPositionState[1], newPositionState[2] };

            if (bonePositionKalman.estimations.Length > 0)
                bonePositionKalman.previous_prediction = bonePositionKalman.estimations;
            else
                bonePositionKalman.previous_prediction = bonePositionMeasurement;

            Vector3 eulerRotation = (new Quaternion(dto.rotation.X, dto.rotation.Y, dto.rotation.Z, dto.rotation.W)).eulerAngles;

            float x_euler; float y_euler; float z_euler;

            if (boneRotationKalman.estimations.Length > 0)
            {
                x_euler = Mathf.Abs(eulerRotation.x - (float)boneRotationKalman.estimations[0]) <= 180 ? eulerRotation.x : (eulerRotation.x > (float)boneRotationKalman.estimations[0] ? eulerRotation.x - 360 : eulerRotation.x + 360);
                y_euler = Mathf.Abs(eulerRotation.y - (float)boneRotationKalman.estimations[1]) <= 180 ? eulerRotation.y : (eulerRotation.y > (float)boneRotationKalman.estimations[1] ? eulerRotation.y - 360 : eulerRotation.y + 360);
                z_euler = Mathf.Abs(eulerRotation.z - (float)boneRotationKalman.estimations[2]) <= 180 ? eulerRotation.z : (eulerRotation.z > (float)boneRotationKalman.estimations[2] ? eulerRotation.z - 360 : eulerRotation.z + 360);
            }
            else
            {
                x_euler = eulerRotation.x;
                y_euler = eulerRotation.y;
                z_euler = eulerRotation.z;
            }

            double[] boneRotationMeasurement = new double[] { x_euler, y_euler, z_euler };
            boneRotationKalman.KalmanFilter.Update(boneRotationMeasurement);

            double[] newRotationState = boneRotationKalman.KalmanFilter.getState();
            boneRotationKalman.prediction = new double[] { newRotationState[0], newRotationState[1], newRotationState[2] };

            if (boneRotationKalman.estimations.Length > 0)
                boneRotationKalman.previous_prediction = boneRotationKalman.estimations;
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

            double[] newPositionState = nodePositionFilter.KalmanFilter.getState();
            nodePositionFilter.prediction = new double[] { newPositionState[0], newPositionState[1], newPositionState[2] };

            if (nodePositionFilter.estimations.Length > 0)
                nodePositionFilter.previous_prediction = nodePositionFilter.estimations;
            else
                nodePositionFilter.previous_prediction = positionMeasurement;

            Vector3 eulerRotation = rotation.eulerAngles;

            float x_euler; float y_euler; float z_euler;

            if (nodeRotationFilter.estimations.Length > 0)
            {
                x_euler = Mathf.Abs(eulerRotation.x - (float)nodeRotationFilter.estimations[0]) <= 180 ? eulerRotation.x : (eulerRotation.x > (float)nodeRotationFilter.estimations[0] ? eulerRotation.x - 360 : eulerRotation.x + 360);
                y_euler = Mathf.Abs(eulerRotation.y - (float)nodeRotationFilter.estimations[1]) <= 180 ? eulerRotation.y : (eulerRotation.y > (float)nodeRotationFilter.estimations[1] ? eulerRotation.y - 360 : eulerRotation.y + 360);
                z_euler = Mathf.Abs(eulerRotation.z - (float)nodeRotationFilter.estimations[2]) <= 180 ? eulerRotation.z : (eulerRotation.z > (float)nodeRotationFilter.estimations[2] ? eulerRotation.z - 360 : eulerRotation.z + 360);
            }
            else
            {
                x_euler = eulerRotation.x;
                y_euler = eulerRotation.y;
                z_euler = eulerRotation.z;
            }

            double[] rotationMeasurement = new double[] { x_euler, y_euler, z_euler };
            nodeRotationFilter.KalmanFilter.Update(rotationMeasurement);

            double[] newRotationState = nodeRotationFilter.KalmanFilter.getState();

            nodeRotationFilter.prediction = new double[] { newRotationState[0], newRotationState[1], newRotationState[2] };

            if (nodeRotationFilter.estimations.Length > 0)
                nodeRotationFilter.previous_prediction = nodeRotationFilter.estimations;
            else
                nodeRotationFilter.previous_prediction = rotationMeasurement;

            this.scale = scale;
        }


        /// <summary>
        /// Update the a UserAvatar directly sent by another client.
        /// </summary>
        /// <param name="trackingFrameDto">a dto containing the tracking data</param>
        /// <param name="timeFrame">sending time in ms</param>
        public IEnumerator UpdateBonePosition(UserTrackingFrameDto trackingFrameDto, ulong timeFrame)
        {
            MeasuresPerSecond = 1000 / (timeFrame - lastFrameTime);
            lastFrameTime = timeFrame;
            lastMessageTime = Time.time;

            NodeKalmanUpdate(trackingFrameDto.position, trackingFrameDto.rotation, trackingFrameDto.scale);

            foreach (BoneDto boneDto in trackingFrameDto.bones)
            {
                if (!bonePositionFilters.ContainsKey(boneDto.boneType))
                    bonePositionFilters.Add(boneDto.boneType, new KalmanPosition(50f, 0.001f));

                if (!boneRotationFilters.ContainsKey(boneDto.boneType))
                    boneRotationFilters.Add(boneDto.boneType, new KalmanRotation(50f, 0.001f));

                if (!boneScales.ContainsKey(boneDto.boneType))
                    boneScales.Add(boneDto.boneType, boneDto.scale);

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