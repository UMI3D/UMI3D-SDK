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
using umi3d.cdk.utils.extrapolation;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Client description of a user's avatar, like <see cref="UserAvatar"/>, but in a collaborative context.
    /// </summary>
    public class UMI3DCollaborativeUserAvatar : UserAvatar
    {
        #region Fields

        /// <summary>
        /// Tools to perfom interpolation for every avatar bones.
        /// </summary>
        private readonly Dictionary<uint, QuaternionLinearDelayedExtrapolator> boneRotationFilters = new Dictionary<uint, QuaternionLinearDelayedExtrapolator>();

        /// <summary>
        /// Reference to the skeleton object of the avatar.
        /// </summary>
        private GameObject skeleton;

        /// <summary>
        /// Extrapolator for the avatar jump height.
        /// </summary>
        protected FloatLinearDelayedExtrapolator skeletonHeightExtrapolator;

        /// <summary>
        /// Coroutine used to update avatar position.
        /// </summary>
        Coroutine updateCoroutine = null;
        private Animator userAnimator;

        #endregion

        #region Methods

        private void Start()
        {
            viewpointObject = GetComponentInChildren<UMI3DViewpointHelper>()?.transform;
            userAnimator = skeleton.GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            // void not to receive unity update messages
        }

        private void LateUpdate()
        {
            if (nodePositionExtrapolator != null)
                this.transform.localPosition = nodePositionExtrapolator.ExtrapolateState();

            if (nodeRotationExtrapolator != null)
                this.transform.localRotation = nodeRotationExtrapolator.ExtrapolateState();

            if (skeleton == null)
                return;

            if (skeletonHeightExtrapolator != null)
            {
                var e = skeletonHeightExtrapolator.ExtrapolateState();
                skeleton.transform.localPosition = new Vector3(0, skeletonHeightExtrapolator.ExtrapolateState(), 0);
            }

            if (ForceDisablingBinding)
                return;
            if (userAnimator == null)
                userAnimator = skeleton.GetComponentInChildren<Animator>();
            if (userAnimator == null)
                return;
            UpdateBoneBindings();
        }

        public void UpdateBoneBindings()
        {
            foreach (uint boneType in boneRotationFilters.Keys)
            {
                Transform boneTransform;

                if (boneType.Equals(BoneType.Viewpoint))
                {
                    boneTransform = viewpointObject;
                    if (boneRotationFilters[boneType] != null)
                        boneTransform.parent.localRotation = boneRotationFilters[boneType].ExtrapolateState();
                }
                else
                {
                    if (boneType.Equals(BoneType.CenterFeet))
                        boneTransform = skeleton.transform;
                    else
                        boneTransform = userAnimator.GetBoneTransform(boneType.ConvertToBoneType().GetValueOrDefault());

                    if (boneRotationFilters[boneType] != null)
                        boneTransform.localRotation = boneRotationFilters[boneType].ExtrapolateState();
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
        /// Update the a UserAvatar skeleton directly sent by another client.
        /// </summary>
        /// <param name="trackingFrameDto">a dto containing the tracking data</param>
        /// <param name="timeFrame">sending time in ms</param>
        public IEnumerator UpdateAvatarPositionCoroutine(UserTrackingFrameDto trackingFrameDto, ulong timeFrame)
        {
            MeasuresPerSecond = 1f / (timeFrame - lastFrameTime);
            lastFrameTime = timeFrame;
            lastMessageTime = Time.time;

            if (nodePositionExtrapolator == null)
            {
                nodePositionExtrapolator = new Vector3LinearDelayedExtrapolator();
                nodePositionExtrapolator.AddMeasure(transform.localPosition, lastMessageTime);
            }
            nodePositionExtrapolator.AddMeasure(trackingFrameDto.position, lastMessageTime);

            if (nodeRotationExtrapolator == null)
            {
                nodeRotationExtrapolator = new QuaternionLinearDelayedExtrapolator();
                nodeRotationExtrapolator.AddMeasure(transform.localRotation, lastMessageTime);
            }

            nodeRotationExtrapolator.AddMeasure(trackingFrameDto.rotation, lastMessageTime);

            if (skeletonHeightExtrapolator == null)
            {
                skeletonHeightExtrapolator = new FloatLinearDelayedExtrapolator();
                skeletonHeightExtrapolator.AddMeasure(skeleton.transform.localPosition.y, lastMessageTime);
            }

            skeletonHeightExtrapolator.AddMeasure(trackingFrameDto.skeletonHighOffset, lastMessageTime);


            foreach (BoneDto boneDto in trackingFrameDto.bones)
            {
                if (!boneRotationFilters.ContainsKey(boneDto.boneType))
                    boneRotationFilters.Add(boneDto.boneType, new QuaternionLinearDelayedExtrapolator());
                boneRotationFilters[boneDto.boneType].AddMeasure(boneDto.rotation, lastMessageTime);

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