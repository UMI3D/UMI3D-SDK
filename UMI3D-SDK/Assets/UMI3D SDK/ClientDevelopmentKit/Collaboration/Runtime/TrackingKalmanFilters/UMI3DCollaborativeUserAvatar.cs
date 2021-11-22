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
        private readonly Dictionary<uint, KalmanRotation> boneRotationFilters = new Dictionary<uint, KalmanRotation>();
        private GameObject skeleton;
        private bool isProcessing = false;

        private void Update()
        {
            RegressionPosition(nodePositionFilter);
            RegressionRotation(nodeRotationFilter);

            this.transform.localPosition = nodePositionFilter.regressed_position;
            this.transform.localRotation = nodeRotationFilter.regressed_rotation;

            Animator userAnimator = skeleton.GetComponentInChildren<Animator>();

            foreach (uint boneType in boneRotationFilters.Keys)
            {
                RegressionRotation(boneRotationFilters[boneType]);

                Transform boneTransform = userAnimator.GetBoneTransform(boneType.ConvertToBoneType().GetValueOrDefault());
                boneTransform.localRotation = boneRotationFilters[boneType].regressed_rotation;

                List<BoneBindingDto> bindings = userBindings.FindAll(binding => binding.boneType == boneType);
                foreach (BoneBindingDto boneBindingDto in bindings)
                {
                    if (boneBindingDto.active && savedTransforms.ContainsKey(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }))
                    {
                        SavedTransform st = savedTransforms[new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }];
                        if (boneBindingDto.syncPosition)
                            st.obj.position = boneTransform.position + boneTransform.TransformDirection((Vector3)boneBindingDto.offsetPosition);
                        st.obj.rotation = boneTransform.rotation * (Quaternion)boneBindingDto.offsetRotation;
                    }
                }
            }
        }

        /// <summary>
        /// Skeleton instanciation for a specific user
        /// </summary>
        /// <param name="id">the user id</param>
        public static void SkeletonCreation(ulong id)
        {
            if (id != UMI3DClientServer.Instance.GetId())
            {
                var ua = UMI3DClientUserTracking.Instance.embodimentDict[id] as UMI3DCollaborativeUserAvatar;
                ua.skeleton = Instantiate((UMI3DClientUserTracking.Instance as UMI3DCollaborationClientUserTracking).UnitSkeleton, ua.transform);
                ua.skeleton.transform.localScale = ua.userSize;
            }
        }

        /// <summary>
        /// Filtering a boneDto position
        /// </summary>
        /// <param name="dto"></param>
        private void BoneKalmanUpdate(BoneDto dto)
        {
            KalmanRotation boneRotationKalman = boneRotationFilters[dto.boneType];

            var quaternionMeasurment = new Quaternion(dto.rotation.X, dto.rotation.Y, dto.rotation.Z, dto.rotation.W);

            Vector3 targetForward = quaternionMeasurment * Vector3.forward;
            Vector3 targetUp = quaternionMeasurment * Vector3.up;

            double[] targetForwardMeasurement = new double[] { targetForward.x, targetForward.y, targetForward.z };
            double[] targetUpMeasurement = new double[] { targetUp.x, targetUp.y, targetUp.z };


            boneRotationKalman.KalmanFilters.Item1.Update(targetForwardMeasurement); // forward
            boneRotationKalman.KalmanFilters.Item2.Update(targetUpMeasurement); // up

            boneRotationKalman.prediction = new System.Tuple<double[], double[]>(boneRotationKalman.KalmanFilters.Item1.getState(), boneRotationKalman.KalmanFilters.Item2.getState());

            if (boneRotationKalman.estimations.Item1.Length > 0)
                boneRotationKalman.previous_prediction = boneRotationKalman.estimations;
            else
                boneRotationKalman.previous_prediction = new System.Tuple<double[], double[]>(targetForwardMeasurement, targetUpMeasurement);
        }

        /// <summary>
        /// Update the a UserAvatar directly sent by another client.
        /// </summary>
        /// <param name="trackingFrameDto">a dto containing the tracking data</param>
        /// <param name="timeFrame">sending time in ms</param>
        public IEnumerator UpdateAvatarPosition(UserTrackingFrameDto trackingFrameDto, ulong timeFrame)
        {
            if (!isProcessing)
            {
                isProcessing = true;

                MeasuresPerSecond = 1000 / (timeFrame - lastFrameTime);
                lastFrameTime = timeFrame;
                lastMessageTime = Time.time;

                NodeKalmanUpdate(trackingFrameDto.position, trackingFrameDto.rotation);

                foreach (BoneDto boneDto in trackingFrameDto.bones)
                {
                    if (!boneRotationFilters.ContainsKey(boneDto.boneType))
                        boneRotationFilters.Add(boneDto.boneType, new KalmanRotation(50f, 0.001f));

                    BoneKalmanUpdate(boneDto);

                    List<BoneBindingDto> bindings = userBindings.FindAll(binding => binding.boneType == boneDto.boneType);
                    foreach (BoneBindingDto boneBindingDto in bindings)
                    {

                        if (boneBindingDto.active)
                        {
                            UMI3DNodeInstance node = null;
                            UMI3DNodeInstance boneBindingnode = null;
                            Transform obj = null;

                            var wait = new WaitForFixedUpdate();


                            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(boneBindingDto.objectId, (e) => node = e as UMI3DNodeInstance);

                            while (node == null)
                                yield return wait;


                            if (boneBindingDto.rigName != "")
                            {
                                UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(boneBindingDto.objectId, (e) => boneBindingnode = (e as UMI3DNodeInstance));
                                while (boneBindingnode == null)
                                    yield return wait;
                                while (
                                    (obj = boneBindingnode.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == boneBindingDto.rigName)) == null
                                    && (obj = InspectBoundRigs(boneBindingDto)) == null)
                                {
                                    yield return wait;
                                }

                                if (!boundRigs.Contains(obj))
                                    boundRigs.Add(obj);
                            }
                            else
                            {
                                obj = node.transform;
                            }

                            if (!savedTransforms.ContainsKey(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }))
                            {
                                var savedTransform = new SavedTransform
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

                isProcessing = false;
            }
        }
    }
}