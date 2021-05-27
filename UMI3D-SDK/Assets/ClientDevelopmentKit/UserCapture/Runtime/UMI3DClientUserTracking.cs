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
using UnityEngine.Events;

namespace umi3d.cdk.userCapture
{
    public class UMI3DClientUserTracking : Singleton<UMI3DClientUserTracking>
    {
        public Transform anchor;
        public Transform skeletonContainer;
        public Transform viewpoint;
        [ConstStringEnum(typeof(BoneType))]
        public string viewpointBonetype;

        public bool sendTracking = true;

        [SerializeField]
        protected float targetTrackingFPS = 15;

        List<string> streamedBonetypes = new List<string>();

        public Dictionary<string, UserAvatar> embodimentDict = new Dictionary<string, UserAvatar>();

        [Tooltip("This event is raised after each analysis of the skeleton.")]
        public UnityEvent skeletonParsedEvent;

        [Tooltip("This event has to be raised to send a CameraPropertiesDto. By default, it is raised at the beginning of Play Mode.")]
        public UnityEvent cameraHasChanged;

        [Tooltip("This event has to be raised to start sending tracking data. The sending will stop if the Boolean \"sendTracking\" is false. By default, it is raised at the beginning of Play Mode.")]
        public UnityEvent startingSendingTracking;

        protected UserTrackingFrameDto LastFrameDto = new UserTrackingFrameDto();
        protected UserCameraPropertiesDto CameraPropertiesDto;
        protected bool hasCameraChanged;

        ///<inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            skeletonParsedEvent = new UnityEvent();
            CameraPropertiesDto = new UserCameraPropertiesDto()
            {
                scale = 1f,
                projectionMatrix = viewpoint.TryGetComponent(out Camera camera) ? camera.projectionMatrix : new Matrix4x4(),
                boneType = viewpointBonetype,
            };
            hasCameraChanged = true;
        }

        protected virtual void Start()
        {
            streamedBonetypes = UMI3DClientUserTrackingBone.instances.Keys.ToList();
            cameraHasChanged.AddListener(() => StartCoroutine("DispatchCamera"));
            cameraHasChanged.Invoke();
            startingSendingTracking.AddListener(() => { if (sendTracking) StartCoroutine("DispatchTracking"); });
            startingSendingTracking.Invoke();
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

                    if (UMI3DClientServer.Exists && UMI3DClientServer.Instance.GetId() != null)
                        UMI3DClientServer.SendTracking(LastFrameDto);

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
            while (UMI3DClientServer.Instance.GetId() == null)
            {
                yield return null;
            }

            UMI3DClientServer.SendData(CameraPropertiesDto, true);
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
                        BoneDto dto = bone.ToDto(anchor);
                        if (dto != null)
                            bonesList.Add(dto);
                    }
                }

                LastFrameDto = new UserTrackingFrameDto()
                {
                    bones = bonesList,
                    position = anchor.position - UMI3DEnvironmentLoader.Instance.transform.position, //position relative to UMI3DEnvironmentLoader node
                    rotation = Quaternion.Inverse(UMI3DEnvironmentLoader.Instance.transform.rotation) * anchor.rotation, //rotation relative to UMI3DEnvironmentLoader node
                    refreshFrequency = targetTrackingFPS,
                    scale = Vector3.one * 1.2f,
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
        public virtual bool RegisterEmbd(string id, UserAvatar u)
        {
            if (embodimentDict.ContainsKey(id))
                return false;
            else
            {
                embodimentDict.Add(id, u);
                return true;
            }
        }

        /// <summary>
        /// Unregister the UserAvatar instance of a user in the concerned dictionary
        /// </summary>
        /// <param name="id">the id of the user</param>
        /// <returns>A bool indicating if the UserAvatar has been unregistered</returns>
        public virtual bool UnregisterEmbd(string id)
        {
            return embodimentDict.Remove(id);
        }

        /// <summary>
        /// Try to get the UserAvatar instance of a user from the concerned dictionary
        /// </summary>
        /// <param name="id">the id of the user</param>
        /// <param name="embd">the UserAvatar instance if found</param>
        /// <returns>A bool indicating if the UserAvatar has been found</returns>
        public virtual bool TryGetValue(string id, out UserAvatar embd)
        {
            return embodimentDict.TryGetValue(id, out embd);
        }

        public void setFPSTarget(int newFPSTarget)
        {
            targetTrackingFPS = newFPSTarget;
        }

        public void setStreamedBones(List<string> bonesToStream)
        {
            this.streamedBonetypes = bonesToStream;
        }
    }
}