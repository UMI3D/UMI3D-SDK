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

using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.userCapture
{
    public class UMI3DClientUserTracking : Singleton<UMI3DClientUserTracking>
    {
        public Transform skeletonContainer;
        public Transform viewpoint;
        [ConstEnum(typeof(BoneType), typeof(uint))]
        public uint viewpointBonetype;

        [SerializeField]
        protected bool sendTracking = true;

        [SerializeField]
        protected float targetTrackingFPS = 15;

        List<uint> streamedBonetypes = new List<uint>();

        public Dictionary<ulong, UserAvatar> embodimentDict = new Dictionary<ulong, UserAvatar>();

        [HideInInspector]
        [Tooltip("This event is raised after each analysis of the skeleton.")]
        public UnityEvent skeletonParsedEvent;

        [HideInInspector]
        [Tooltip("This event has to be raised to send a CameraPropertiesDto. By default, it is raised at the beginning of Play Mode.")]
        public UnityEvent sendingCameraProperties;

        [HideInInspector]
        [Tooltip("This event has to be raised to start sending tracking data. The sending will stop if the Boolean \"sendTracking\" is false. By default, it is raised at the beginning of Play Mode.")]
        public UnityEvent startingSendingTracking;

        public class HandPoseEvent : UnityEvent<UMI3DHandPoseDto> { };

        public HandPoseEvent handPoseEvent = new HandPoseEvent();

        public class AvatarEvent : UnityEvent<ulong> { };

        public AvatarEvent avatarEvent = new AvatarEvent();

        protected UserTrackingFrameDto LastFrameDto = new UserTrackingFrameDto();
        protected UserCameraPropertiesDto CameraPropertiesDto = null;
        protected bool sendCameraProperties = false;

        ///<inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            skeletonParsedEvent = new UnityEvent();
        }

        protected virtual void Start()
        {
            streamedBonetypes = UMI3DClientUserTrackingBone.instances.Keys.ToList();
            sendingCameraProperties.AddListener(() => StartCoroutine(DispatchCamera()));
            startingSendingTracking.AddListener(() => { if (sendTracking) StartCoroutine(DispatchTracking()); });
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() => StartCoroutine(DispatchCamera()));
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() => { if (sendTracking) StartCoroutine(DispatchTracking()); });
        }

        /// <summary>
        /// Dispatch User Tracking data through Tracking Channel
        /// </summary>
        protected virtual IEnumerator DispatchTracking()
        {
            while (sendTracking)
            {
                if (targetTrackingFPS > 0)
                {
                    BonesIterator();

                    if (UMI3DClientServer.Exists && UMI3DClientServer.Instance.GetId() != 0)
                        UMI3DClientServer.SendTracking(LastFrameDto);

                    if (sendCameraProperties)
                        sendingCameraProperties.Invoke();

                    yield return new WaitForSeconds(1f / targetTrackingFPS);
                }
                else
                    yield return new WaitUntil(() => targetTrackingFPS > 0 || !sendTracking);
            }
        }

        /// <summary>
        /// Dispatch Camera data through Tracking Channel
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator DispatchCamera()
        {
            while (UMI3DClientServer.Instance.GetId() == 0)
            {
                yield return null;
            }

            UserCameraPropertiesDto newCameraProperties = new UserCameraPropertiesDto()
            {
                scale = 1f,
                projectionMatrix = viewpoint.TryGetComponent(out Camera camera) ? camera.projectionMatrix : new Matrix4x4(),
                boneType = viewpointBonetype,
            };

            if (!newCameraProperties.Equals(CameraPropertiesDto))
            {
                UMI3DClientServer.SendData(newCameraProperties, true);
                CameraPropertiesDto = newCameraProperties;
            }

        }

        /// <summary>
        /// Iterate through the bones of the browser's skeleton to create BoneDto
        /// </summary>
        protected void BonesIterator()
        {
            if (UMI3DEnvironmentLoader.Exists)
            {
                List<BoneDto> bonesList = new List<BoneDto>();
                foreach (UMI3DClientUserTrackingBone bone in UMI3DClientUserTrackingBone.instances.Values)
                {
                    if (streamedBonetypes.Contains(bone.boneType))
                    {
                        BoneDto dto = bone.ToDto();
                        if (dto != null)
                            bonesList.Add(dto);
                    }
                }

                LastFrameDto = new UserTrackingFrameDto()
                {
                    bones = bonesList,
                    position = this.transform.position - UMI3DEnvironmentLoader.Instance.transform.position, //position relative to UMI3DEnvironmentLoader node
                    rotation = Quaternion.Inverse(UMI3DEnvironmentLoader.Instance.transform.rotation) * this.transform.rotation, //rotation relative to UMI3DEnvironmentLoader node
                    refreshFrequency = targetTrackingFPS,
                    userId = UMI3DClientServer.Instance.GetId(),
                };

                skeletonParsedEvent.Invoke();
            }
        }

        /// <summary>
        /// Register the UserAvatar instance of a user in the concerned dictionary
        /// </summary>
        /// <param name="id">the id of the user</param>
        /// <param name="u">the UserAvatar instance to register</param>
        /// <returns>A bool indicating if the UserAvatar has been registered</returns>
        public virtual bool RegisterEmbd(ulong id, UserAvatar u)
        {
            if (embodimentDict.ContainsKey(id))
                return false;
            else
            {
                embodimentDict.Add(id, u);
                avatarEvent.Invoke(id);
                return true;
            }
        }

        /// <summary>
        /// Unregister the UserAvatar instance of a user in the concerned dictionary
        /// </summary>
        /// <param name="id">the id of the user</param>
        /// <returns>A bool indicating if the UserAvatar has been unregistered</returns>
        public virtual bool UnregisterEmbd(ulong id)
        {
            return embodimentDict.Remove(id);
        }

        /// <summary>
        /// Try to get the UserAvatar instance of a user from the concerned dictionary
        /// </summary>
        /// <param name="id">the id of the user</param>
        /// <param name="embd">the UserAvatar instance if found</param>
        /// <returns>A bool indicating if the UserAvatar has been found</returns>
        public virtual bool TryGetValue(ulong id, out UserAvatar embd)
        {
            return embodimentDict.TryGetValue(id, out embd);
        }

        public void setFPSTarget(int newFPSTarget)
        {
            targetTrackingFPS = newFPSTarget;
        }

        public void setStreamedBones(List<uint> bonesToStream)
        {
            this.streamedBonetypes = bonesToStream;
        }

        public void setCameraPropertiesSending(bool activeSending)
        {
            this.sendCameraProperties = activeSending;
        }

        public void setTrackingSending(bool activeSending)
        {
            this.sendTracking = activeSending;
            startingSendingTracking.Invoke();
        }
    }
}