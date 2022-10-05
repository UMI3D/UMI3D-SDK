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
        #region Fields

        /// <summary>
        /// Tools to perfom interpolation for every avatar bones.
        /// </summary>
        private readonly Dictionary<uint, UMI3DKalmanQuaternionLerp> boneRotationFilters = new Dictionary<uint, UMI3DKalmanQuaternionLerp>();
        private GameObject skeleton;

        protected UMI3DKalmanVector3Lerp skeletonHeightLerp;

        /// <summary>
        /// Coroutine used to update avatar position.
        /// </summary>
        Coroutine updateCoroutine = null;

        #endregion

        #region Methods

        private void Start()
        {
            viewpointObject = GetComponentInChildren<UMI3DViewpointHelper>()?.transform;
        }

        private void Update()
        {
            if (nodePositionLerp != null)
                this.transform.localPosition = nodePositionLerp.GetValue(Time.deltaTime);

            if (nodeRotationLerp != null)
                this.transform.localRotation = nodeRotationLerp.GetValue(Time.deltaTime);

            if (skeleton == null)
                return;

            if (skeletonHeightLerp != null)
                skeleton.transform.localPosition = skeletonHeightLerp.GetValue(Time.deltaTime);

            Animator userAnimator = skeleton.GetComponentInChildren<Animator>();

            foreach (uint boneType in boneRotationFilters.Keys)
            {
                Transform boneTransform;

                if (boneType.Equals(BoneType.Viewpoint))
                {
                    boneTransform = viewpointObject;
                    if (boneRotationFilters[boneType] != null)
                        boneTransform.parent.localRotation = boneRotationFilters[boneType].GetValue(Time.deltaTime);
                }
                else
                {
                    if (boneType.Equals(BoneType.CenterFeet))
                        boneTransform = skeleton.transform;
                    else
                        boneTransform = userAnimator.GetBoneTransform(boneType.ConvertToBoneType().GetValueOrDefault());

                    if (boneRotationFilters[boneType] != null)
                        boneTransform.localRotation = boneRotationFilters[boneType].GetValue(Time.deltaTime);
                }


                List<BoneBindingDto> bindings = userBindings.FindAll(binding => binding.boneType == boneType);
                foreach (BoneBindingDto boneBindingDto in bindings)
                {
                    if (boneBindingDto.active && savedTransforms.ContainsKey(new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }))
                    {
                        SavedTransform st = savedTransforms[new BoundObject() { objectId = boneBindingDto.objectId, rigname = boneBindingDto.rigName }];
                        if (boneBindingDto.syncPosition)
                            st.obj.position = boneTransform.position + boneTransform.TransformDirection((Vector3)boneBindingDto.offsetPosition);
                        if (boneBindingDto.syncRotation)
                            st.obj.rotation = boneTransform.rotation * bounds.Find(b => st.obj == b.obj).anchorRelativeRot * (Quaternion)boneBindingDto.offsetRotation;
                        if (boneBindingDto.freezeWorldScale)
                        {
                            Vector3 WscaleMemory = st.savedLossyScale;
                            Vector3 WScaleParent = st.obj.parent.lossyScale;

                            st.obj.localScale = new Vector3(WscaleMemory.x / WScaleParent.x, WscaleMemory.y / WScaleParent.y, WscaleMemory.z / WScaleParent.z) + boneBindingDto.offsetScale;
                        }
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
            if (id != UMI3DClientServer.Instance.GetUserId()
                && UMI3DClientUserTracking.Instance.embodimentDict.TryGetValue(id, out UserAvatar value)
                && value is UMI3DCollaborativeUserAvatar ua
                && ua.skeleton == null)
            {
                ua.skeleton = Instantiate((UMI3DClientUserTracking.Instance as UMI3DCollaborationClientUserTracking).UnitSkeleton, ua.transform);
                ua.skeleton.transform.localScale = ua.userSize;
            }
        }

        /// <summary>
        /// Updates avatar (position, rotation and bones rotations).
        /// </summary>
        /// <param name="trackingFrameDto"></param>
        /// <param name="timeFrame"></param>
        public void UpdateAvatarPosition(UserTrackingFrameDto trackingFrameDto, ulong timeFrame)
        {
            if (timeFrame < lastFrameTime)
                return;

            if (UMI3DEnvironmentLoader.GetNode(trackingFrameDto.parentId)?.transform != transform.parent)
                return;

            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);

            updateCoroutine = StartCoroutine(UpdateAvatarPositionCoroutine(trackingFrameDto, timeFrame));
        }


        /// <summary>
        /// Update the a UserAvatar directly sent by another client.
        /// </summary>
        /// <param name="trackingFrameDto">a dto containing the tracking data</param>
        /// <param name="timeFrame">sending time in ms</param>
        public IEnumerator UpdateAvatarPositionCoroutine(UserTrackingFrameDto trackingFrameDto, ulong timeFrame)
        {
            MeasuresPerSecond = 1000 / (timeFrame - lastFrameTime);
            lastFrameTime = timeFrame;
            lastMessageTime = Time.time;

            if (nodePositionLerp == null)
                nodePositionLerp = new UMI3DKalmanVector3Lerp(trackingFrameDto.position);
            else
                nodePositionLerp.UpdateValue(trackingFrameDto.position);

            if (nodeRotationLerp == null)
                nodeRotationLerp = new UMI3DKalmanQuaternionLerp(trackingFrameDto.rotation);
            else
                nodeRotationLerp.UpdateValue(trackingFrameDto.rotation);


            if (skeletonHeightLerp == null)
                skeletonHeightLerp = new UMI3DKalmanVector3Lerp(new Vector3(0, trackingFrameDto.skeletonHighOffset, 0));
            else
                skeletonHeightLerp.UpdateValue(new Vector3(0, trackingFrameDto.skeletonHighOffset, 0));

            foreach (BoneDto boneDto in trackingFrameDto.bones)
            {
                if (boneRotationFilters.ContainsKey(boneDto.boneType))
                {
                    boneRotationFilters[boneDto.boneType].UpdateValue(boneDto.rotation);
                }
                else
                {
                    boneRotationFilters.Add(boneDto.boneType, new UMI3DKalmanQuaternionLerp(boneDto.rotation));
                }

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
                            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(boneBindingDto.objectId, (e) => boneBindingnode = e as UMI3DNodeInstance);
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

                        if (boneBindingDto.rigName == "")
                        {
                            node.updatePose = false;
                        }
                    }
                }
            }
            updateCoroutine = null;
        }

        #endregion
    }
}