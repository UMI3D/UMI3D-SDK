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
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.userCapture
{
    public class UMI3DClientUserTracking : Singleton<UMI3DClientUserTracking>
    {
        public Transform anchor;
        public Transform viewpoint;
        public string viewpointBonetype;

        public float skeletonParsingIterationCooldown = 0f;
        float cooldownTmp = 0f;
        public float time = 0f;
        float timeTmp = 0;
        public int max = 0;
        int counter = 0;

        public Dictionary<string, UserAvatar> embodimentDict = new Dictionary<string, UserAvatar>();

        public UnityEvent skeletonParsedEvent;
        public UnityEvent cameraHasChanged;

        protected UserTrackingFrameDto LastFrameDto = new UserTrackingFrameDto();
        protected UserCameraPropertiesDto CameraPropertiesDto;
        protected bool hasCameraChanged;


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
            cameraHasChanged.AddListener(() => StartCoroutine("DispatchCamera"));
            cameraHasChanged.Invoke();
        }

        protected virtual void Update()
        {
            if (UMI3DClientServer.Exists)
                DispatchTracking();

            if (iterationCooldown())
                BonesIterator();
        }

        protected virtual void DispatchTracking()
        {
            if ((checkTime() || checkMax()) && LastFrameDto.userId != null)
            {
                UMI3DClientServer.SendTracking(LastFrameDto, false);
            }
        }

        protected virtual IEnumerator DispatchCamera()
        {
            while (UMI3DClientServer.Instance.GetId() == null)
            {
                yield return null;
            }

            Debug.LogWarning("DispatchCamera");
            UMI3DClientServer.SendTracking(CameraPropertiesDto, true);
        }

        /// <summary>
        /// Iterate through the bones of the browser's skeleton to create BoneDto
        /// </summary>
        protected void BonesIterator()
        {
            List<BoneDto> bonesList = new List<BoneDto>();
            foreach (UMI3DClientUserTrackingBone bone in UMI3DClientUserTrackingBone.instances.Values)
            {
                BoneDto dto = bone.ToDto(anchor);
                if (dto != null)
                    bonesList.Add(dto);
            }

            LastFrameDto = new UserTrackingFrameDto()
            {
                bones = bonesList,
                position = anchor.localPosition, //position relative to UMI3DEnvironmentLoader node
                rotation = anchor.localRotation, //rotation relative to UMI3DEnvironmentLoader node
                scale = anchor.localScale,
                userId = UMI3DClientServer.Instance.GetId(),
                refreshFrequency = skeletonParsingIterationCooldown // depends on Checktime() too.
            };

            skeletonParsedEvent.Invoke();
        }

        bool iterationCooldown()
        {
            cooldownTmp -= Time.deltaTime;
            if (skeletonParsingIterationCooldown == 0 || cooldownTmp <= 0)
            {
                cooldownTmp = skeletonParsingIterationCooldown;
                return true;
            }
            return false;
        }

        protected bool checkTime()
        {
            timeTmp -= Time.deltaTime;
            if (time == 0 || timeTmp <= 0)
            {
                timeTmp = time;
                return true;
            }
            return false;
        }

        protected bool checkMax()
        {
            if (max != 0 && counter > max)
            {
                counter = 0;
                return true;
            }
            counter += 1;
            return false;
        }

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

        public virtual bool UnregisterEmbd(string id)
        {
            return embodimentDict.Remove(id);
        }

        public virtual bool TryGetValue(string id, out UserAvatar embd)
        {
            return embodimentDict.TryGetValue(id, out embd);
        }

    }
}