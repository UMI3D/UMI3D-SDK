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
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class UserAvatar : MonoBehaviour
    {
        protected struct SavedTransform
        {
            public Transform obj;
            public Transform savedParent;
            public Vector3 savedPosition;
            public Quaternion savedRotation;
        }

        protected struct BoundObject
        {
            public string objectId;
            public string rigname;
        }

        protected struct Bound
        {
            public Transform bone;
            public string bonetype;
            public Transform obj;
            public Vector3 offsetPosition;
            public Quaternion offsetRotation;
        }

        public List<Transform> boundRigs = new List<Transform>();
        public string userId { get; protected set; }
        public bool activeUserBindings { get; protected set; }
        public List<BoneBindingDto> userBindings { get; protected set; }

        protected Dictionary<BoundObject, SavedTransform> savedTransforms = new Dictionary<BoundObject, SavedTransform>();

        private List<Bound> bounds = new List<Bound>();

        private void Update()
        {
            foreach (var item in bounds)
            {
                if (item.obj != null)
                {
                    item.obj.position = item.bone.TransformPoint(item.offsetPosition);
                    item.obj.rotation = item.bone.rotation * item.offsetRotation;
                }
            }

            RegressionPosition(nodePositionFilter);
            RegressionRotation(nodeRotationFilter);

            this.transform.localPosition = nodePositionFilter.regressed_position;
            this.transform.localRotation = nodeRotationFilter.RegressedQuaternion();
        }

        /// <summary>
        /// Set a new UserAvatar from an UMI3DAvatarNodeDto.
        /// </summary>
        /// <param name="dto"></param>
        public void Set(UMI3DAvatarNodeDto dto)
        {
            userId = dto.userId;
            activeUserBindings = dto.activeBindings;
            userBindings = dto.bindings;

            if (activeUserBindings && userBindings != null)
            {
                foreach (BoneBindingDto boneDto in userBindings)
                {
                    if (boneDto.active)
                        UpdateBindingPosition(boneDto);
                }
            }
        }

        /// <summary>
        /// Set the binding activation from the given bool.
        /// </summary>
        /// <param name="b">the activation value</param>
        public void SetActiveBindings(bool b)
        {
            activeUserBindings = b;

            if (userBindings != null)
            {
                if (activeUserBindings)
                {
                    foreach (BoneBindingDto dto in userBindings)
                        if (dto.active)
                            UpdateBindingPosition(dto);
                }
                else
                {
                    foreach (BoneBindingDto dto in userBindings)
                        if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                            ResetObject(dto);
                }
            }

        }

        /// <summary>
        /// Set a new list of BoneBindingDto.
        /// </summary>
        /// <param name="newBindings">a list of dto containing binding data</param>
        public void SetBindings(List<BoneBindingDto> newBindings)
        {
            foreach (BoneBindingDto dto in userBindings)
                if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                    ResetObject(dto);

            userBindings = newBindings;

            if (activeUserBindings && userBindings != null)
                foreach (BoneBindingDto dto in userBindings)
                    if (dto.active)
                        UpdateBindingPosition(dto);
        }

        /// <summary>
        /// Add a new BoneBindingDto at the given index.
        /// </summary>
        /// <param name="index">the index in the list of bindings</param>
        /// <param name="dto"></param>
        public void AddBinding(int index, BoneBindingDto dto)
        {
            if (index <= userBindings.Count - 1)
            {
                BoneBindingDto dtoAtIndex = userBindings[index];

                if (!dto.bindingId.Contains(dtoAtIndex.bindingId) && !dtoAtIndex.bindingId.Contains(dto.bindingId))
                    AddBinding_(index, dto);
            }
            else
                AddBinding_(index, dto);
        }

        protected void AddBinding_(int index, BoneBindingDto dto)
        {
            userBindings.Insert(index, dto);
            if (activeUserBindings && dto.active)
                UpdateBindingPosition(dto);
        }

        /// <summary>
        /// Remove the BoneBindingDto at the given index.
        /// </summary>
        /// <param name="index">the index in the list of bindings</param>
        /// <param name="dto"></param>
        public void RemoveBinding(int index, BoneBindingDto dto)
        {
            userBindings.RemoveAt(index);
            ResetObject(dto);
        }

        /// <summary>
        /// Update the BoneBindingDto at the given index.
        /// </summary>
        /// <param name="index">the index in the list of bindings</param>
        /// <param name="dto"></param>
        public void UpdateBinding(int index, BoneBindingDto dto)
        {
            userBindings[index] = dto;
            if (activeUserBindings)
            {
                if (dto.active)
                    UpdateBindingPosition(dto);
                else if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                    ResetObject(dto);
            }
        }

        protected void UpdateBindingPosition(BoneBindingDto dto)
        {
            if (userId == UMI3DClientServer.Instance.GetId())
            {
                if (UMI3DClientUserTrackingBone.instances.TryGetValue(dto.boneType, out UMI3DClientUserTrackingBone bone))
                {

                    StartCoroutine(WaitForRig(dto, bone));

                }
                else
                    UnityEngine.Debug.LogWarning(dto.boneType + "not found in bones instances");
            }
        }

        protected Transform InspectBoundRigs(BoneBindingDto dto)
        {
            Transform obj = null;
            foreach (Transform rig in boundRigs)
            {
                if ((obj = rig.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName)) != null)
                    return obj;
            }
            return obj;
        }

        protected IEnumerator WaitForRig(BoneBindingDto dto, UMI3DClientUserTrackingBone bone)
        {
            UMI3DNodeInstance node;
            var wait = new WaitForFixedUpdate();

            while ((node = UMI3DEnvironmentLoader.GetNode(dto.objectId)) == null)
            {
                yield return wait;
            }

            if (node != null)
            {
                Transform obj = null;
                if (dto.rigName != "")
                {
                    while ((obj = UMI3DEnvironmentLoader.GetNode(dto.objectId).transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName)) == null && (obj = InspectBoundRigs(dto)) == null)
                    {
                        yield return wait;
                    }

                    if (!boundRigs.Contains(obj))
                        boundRigs.Add(obj);
                }
                else
                    obj = node.transform;

                if (!savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                {
                    SavedTransform savedTransform = new SavedTransform
                    {
                        obj = obj,
                        savedParent = obj.parent,
                        savedPosition = obj.localPosition,
                        savedRotation = obj.localRotation
                    };

                    savedTransforms.Add(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, savedTransform);

                    bounds.Add(new Bound()
                    {
                        bone = bone.transform,
                        bonetype = dto.boneType,
                        obj = obj,
                        offsetPosition = dto.position,
                        offsetRotation = dto.rotation
                    });

                    if (dto.rigName == "")
                        node.updatePose = false;
                }
                else
                {
                    if (savedTransforms.TryGetValue(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, out SavedTransform savedTransform))
                    {
                        int index = bounds.FindIndex(b => b.obj == savedTransform.obj && b.bonetype == dto.boneType);

                        if (index >= 0)
                        {
                            Bound bound = bounds[index];
                            bound.offsetPosition = dto.position;
                            bound.offsetRotation = dto.rotation;
                            bounds[index] = bound;
                        }
                        else
                        {
                            bounds.Add(new Bound()
                            {
                                bone = bone.transform,
                                bonetype = dto.boneType,
                                obj = obj,
                                offsetPosition = dto.position,
                                offsetRotation = dto.rotation
                            });
                        }
                    }
                }
            }
        }

        protected void ResetObject(BoneBindingDto dto)
        {
            UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.objectId);

            if (node != null)
            {
                if (savedTransforms.TryGetValue(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, out SavedTransform savedTransform))
                {
                    if (savedTransform.obj != null)
                    {
                        int index = bounds.FindIndex(b => b.obj == savedTransform.obj);

                        Bound bd = bounds[index];
                        bounds.Remove(bd);

                        if (dto.rigName == "")
                        {
                            if (node.dto is GlTFNodeDto)
                            {
                                savedTransform.obj.localPosition = (node.dto as GlTFNodeDto).position;
                                savedTransform.obj.localRotation = (node.dto as GlTFNodeDto).rotation;
                            }
                            else if (node.dto is GlTFSceneDto)
                            {
                                savedTransform.obj.localPosition = (node.dto as GlTFSceneDto).extensions.umi3d.position;
                                savedTransform.obj.localRotation = (node.dto as GlTFSceneDto).extensions.umi3d.rotation;
                            }
                        }
                        else
                        {
                            savedTransform.obj.localPosition = savedTransform.savedPosition;
                            savedTransform.obj.localRotation = savedTransform.savedRotation;
                        }
                    }

                    if (dto.rigName == "" && node != null)
                        node.updatePose = true;
                    else
                        boundRigs.Remove(savedTransform.obj);
                }
                else
                    Destroy(node.gameObject);
            }

            if (!dto.active)
            {
                savedTransforms.Remove(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName });
            }
        }

        protected class KalmanPosition
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

        protected class KalmanRotation
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

        protected KalmanPosition nodePositionFilter = new KalmanPosition(50, 0.5);
        protected KalmanRotation nodeRotationFilter = new KalmanRotation(10, 0.5);
        protected Vector3 scale = Vector3.one;

        protected float MeasuresPerSecond = 0;
        protected float lastFrameTime = 0;
        protected float lastMessageTime = 0;

        /// <summary>
        /// Applies linear regression to the filtered position of an object
        /// </summary>
        /// <param name="tools"></param>
        protected void RegressionPosition(KalmanPosition tools)
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
        protected void RegressionRotation(KalmanRotation tools)
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
        /// Filtering a user AvatarNode position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        protected void NodeKalmanUpdate(Vector3 position, Quaternion rotation, Vector3 scale)
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

        public virtual IEnumerator UpdateAvatarPosition(UserTrackingFrameDto trackingFrameDto, ulong timeFrame)
        {
            MeasuresPerSecond = 1000 / (timeFrame - lastFrameTime);
            lastFrameTime = timeFrame;
            lastMessageTime = Time.time;

            NodeKalmanUpdate(trackingFrameDto.position, trackingFrameDto.rotation, trackingFrameDto.scale);

            yield return null; 
        }
    }
}
