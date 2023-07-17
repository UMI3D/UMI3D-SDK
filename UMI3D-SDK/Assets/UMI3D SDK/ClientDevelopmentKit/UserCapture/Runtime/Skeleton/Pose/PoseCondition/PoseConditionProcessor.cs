/*
Copyright 2019 - 2023 Inetum

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
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Pose overrider handler to calculate whether the pose can be played or not depending on the set of condition of a specific container
    /// </summary>
    public class PoseConditionProcessor
    {
        private readonly IEnvironmentManager environmentManagerService;
        private readonly IWritableSubskeleton trackedSkeletonService;

        public PoseConditionProcessor(UMI3DPoseOverriderContainerDto overriderContainer)
        {
            environmentManagerService = UMI3DEnvironmentLoader.Instance;
            trackedSkeletonService = PersonalSkeletonManager.Instance.personalSkeleton.TrackedSubskeleton;
            SetPoseOverriderContainer(overriderContainer);
        }

        public PoseConditionProcessor(UMI3DPoseOverriderContainerDto overriderContainer, IEnvironmentManager environmentLoaderService, IWritableSubskeleton trackedSkeletonService)
        {
            this.environmentManagerService = environmentLoaderService;
            this.trackedSkeletonService = trackedSkeletonService;
            SetPoseOverriderContainer(overriderContainer);
        }

        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        /// <summary>
        /// If th e condition processor is enable
        /// </summary>
        private bool isActive = false;
        /// <summary>
        /// Ref to the related pose container
        /// </summary>
        private UMI3DPoseOverriderContainerDto poseOverriderContainerDto;

        /// <summary>
        /// Sends a signal when the condition is validated
        /// </summary>
        public event EventHandler<PoseOverriderDto> OnConditionValidated;

        /// <summary>
        /// Sends a signal when the contition is desactivated
        /// </summary>
        public event EventHandler<PoseOverriderDto> OnConditionDesactivated;

        /// <summary>
        /// All the overriders  which ca nonly be considerd if they an interaction ocurs
        /// </summary>
        private List<PoseOverriderDto> nonEnvirnmentalPoseOverriders = new List<PoseOverriderDto>();
        /// <summary>
        /// All the overriders that are considered without interection (more expensive interm of perfs)
        /// </summary>
        private List<PoseOverriderDto> envirnmentalPoseOverriders = new List<PoseOverriderDto>();

        private List<PoseOverriderDto> environmentalActivatedPoseOverriders = new List<PoseOverriderDto>();
        private List<PoseOverriderDto> nonEnvironmentalActivatedPoseOverriders = new List<PoseOverriderDto>();

        /// <summary>
        /// return a copy of the environmental overriders
        /// </summary>
        /// <returns></returns>
        public List<PoseOverriderDto> GetEnvironmentalPoseOverriders()
        {
            List<PoseOverriderDto> poseOverriderDtos = new List<PoseOverriderDto>();
            poseOverriderDtos.AddRange(envirnmentalPoseOverriders);
            return poseOverriderDtos;
        }

        /// <summary>
        /// returns a copy of the non environmental overriders
        /// </summary>
        /// <returns></returns>
        public List<PoseOverriderDto> GetNonEnvironmentalPoseOverriders()
        {
            List<PoseOverriderDto> poseOverriderDtos = new List<PoseOverriderDto>();
            poseOverriderDtos.AddRange(nonEnvirnmentalPoseOverriders);
            return poseOverriderDtos;
        }

        /// <summary>
        /// returns a copy of the activated environmental overriders
        /// </summary>
        /// <returns></returns>
        public List<PoseOverriderDto> GetEnvironmentalActivatedPoseOverriders()
        {
            List<PoseOverriderDto> poseOverriderDtos = new List<PoseOverriderDto>();
            poseOverriderDtos.AddRange(environmentalActivatedPoseOverriders);
            return poseOverriderDtos;
        }

        /// <summary>
        /// returns a copy of the activated non environmental overriders
        /// </summary>
        /// <returns></returns>
        public List<PoseOverriderDto> GetNonEnvironmentalActivatedPoseOverriders()
        {
            List<PoseOverriderDto> poseOverriderDtos = new List<PoseOverriderDto>();
            poseOverriderDtos.AddRange(nonEnvironmentalActivatedPoseOverriders);
            return poseOverriderDtos;
        }

        /// <summary>
        /// Set the
        /// </summary>
        /// <param name="poseOverriderContainerDto"></param>
        /// <returns></returns>
        public bool SetPoseOverriderContainer(UMI3DPoseOverriderContainerDto poseOverriderContainerDto)
        {
            if (poseOverriderContainerDto == null) return false;

            this.poseOverriderContainerDto = poseOverriderContainerDto;
            nonEnvirnmentalPoseOverriders = poseOverriderContainerDto.poseOverriderDtos
                                                .Where(pod=>pod.isRelease || pod.isTrigger || pod.isHoverEnter || pod.isHoverExit)
                                                .ToList();
            envirnmentalPoseOverriders.AddRange(poseOverriderContainerDto.poseOverriderDtos.Except(nonEnvirnmentalPoseOverriders));

            return true;
        }

        /// <summary>
        /// Stops the check of fall the overriders
        /// </summary>
        public void DisableCheck()
        {
            this.isActive = false;
        }

        /// <summary>
        /// Activated if the Hover Enter is triggered
        /// </summary>
        public bool CheckHoverEnterConditions()
        {
            foreach (PoseOverriderDto poseOverrider in nonEnvirnmentalPoseOverriders)
            {
                if (poseOverrider.isHoverEnter)
                {
                    if (CheckConditions(poseOverrider.poseConditions))
                    {
                        if (nonEnvironmentalActivatedPoseOverriders.Contains(poseOverrider)) continue;

                        OnConditionValidated?.Invoke(this, poseOverrider);
                        nonEnvironmentalActivatedPoseOverriders.Add(poseOverrider);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Activated if the Hover Exit is triggered
        /// </summary>
        public bool CheckHoverExitConditions()
        {
            foreach (PoseOverriderDto poseOverrider in nonEnvirnmentalPoseOverriders)
            {
                if (poseOverrider.isHoverExit)
                {
                    if (CheckConditions(poseOverrider.poseConditions))
                    {
                        if (nonEnvironmentalActivatedPoseOverriders.Contains(poseOverrider)) continue;

                        OnConditionValidated?.Invoke(this, poseOverrider);
                        nonEnvironmentalActivatedPoseOverriders.Add(poseOverrider);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Activated if the Trigger is triggered
        /// </summary>
        public bool CheckTriggerConditions()
        {
            foreach (PoseOverriderDto poseOverrider in nonEnvirnmentalPoseOverriders)
            {
                if (poseOverrider.isTrigger)
                {
                    if (CheckConditions(poseOverrider.poseConditions))
                    {
                        if (nonEnvironmentalActivatedPoseOverriders.Contains(poseOverrider)) continue;

                        OnConditionValidated?.Invoke(this, poseOverrider);
                        nonEnvironmentalActivatedPoseOverriders.Add(poseOverrider);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Activated if the Release Enter is triggered
        /// </summary>
        public bool CheckReleaseConditions()
        {
            foreach (PoseOverriderDto poseOverrider in nonEnvirnmentalPoseOverriders)
            {
                if (poseOverrider.isRelease)
                {
                    if (CheckConditions(poseOverrider.poseConditions))
                    {
                        if (nonEnvironmentalActivatedPoseOverriders.Contains(poseOverrider)) continue;

                        OnConditionValidated?.Invoke(this, poseOverrider);
                        nonEnvironmentalActivatedPoseOverriders.Add(poseOverrider);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Start to check all overriders
        /// return -1 if there is no pose playable,
        /// overwise returns the index of the playable pose
        /// </summary>
        public void CheckCondtionOfAllOverriders()
        {
            if (!isActive)
            {
                isActive = true;
                CoroutineManager.Instance.AttachCoroutine(LaunchCheck());
            }
        }

        private IEnumerator LaunchCheck()
        {
            while (isActive)
            {
                yield return new WaitForSeconds(0.1f);
                for (int i = 0; i < envirnmentalPoseOverriders.Count; i++)
                {
                    if (CheckConditions(envirnmentalPoseOverriders[i].poseConditions))
                    {
                        if (environmentalActivatedPoseOverriders.Contains(envirnmentalPoseOverriders[i])) continue;

                        OnConditionValidated?.Invoke(this, envirnmentalPoseOverriders[i]);
                        environmentalActivatedPoseOverriders.Add(envirnmentalPoseOverriders[i]);
                    }
                    else if (environmentalActivatedPoseOverriders.Contains(envirnmentalPoseOverriders[i]))
                    {
                        OnConditionDesactivated?.Invoke(this, envirnmentalPoseOverriders[i]);
                        environmentalActivatedPoseOverriders.Remove(envirnmentalPoseOverriders[i]);
                    }
                }

                for (int i = 0; i < nonEnvironmentalActivatedPoseOverriders.Count; i++)
                {
                    if (!CheckConditions(nonEnvironmentalActivatedPoseOverriders[i].poseConditions))
                    {
                        OnConditionDesactivated?.Invoke(this, nonEnvironmentalActivatedPoseOverriders[i]);
                        nonEnvironmentalActivatedPoseOverriders.Remove(nonEnvironmentalActivatedPoseOverriders[i]);
                    }
                }
            }
        }

        private bool CheckConditions(PoseConditionDto[] poseConditions)
        {
            for (int i = 0; i < poseConditions.Length; i++)
            {
                if (!CheckCondition(poseConditions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckCondition(PoseConditionDto poseConditionDto)
        {
            switch (poseConditionDto)
            {
                case MagnitudeConditionDto magnitudeConditionDto:
                    return HandleMagnitude(magnitudeConditionDto);

                case BoneRotationConditionDto boneRotationConditionDto:
                    return HandleBoneRotation(boneRotationConditionDto);

                case ScaleConditionDto scaleConditionDto:
                    return HandleTargetScale(scaleConditionDto);

                case DirectionConditionDto directionConditionDto:
                    // UNDONE
                    break;

                case RangeConditionDto rangeConditionDto:
                    // UNDONE
                    break;

                case NotConditionDto notConditionDto:
                    // UNDONE
                    break;
            }

            return false;
        }

        private bool HandleMagnitude(MagnitudeConditionDto magnitudeConditionDto)
        {
            UMI3DNodeInstance targetNodeInstance = environmentManagerService.GetNodeInstance(magnitudeConditionDto.TargetNodeId);

            if (targetNodeInstance == null)
            {
                UMI3DLogger.LogError("you havent referenced the right entity ID in your magnitude DTO", scope);
                return false;
            }

            Vector3 targetPosition = targetNodeInstance.transform.position;

            Vector3 bonePosition = Vector3.zero;
            if ((trackedSkeletonService as TrackedSkeleton).bones.TryGetValue(magnitudeConditionDto.BoneOrigine, out TrackedSkeletonBone bone))
                bonePosition = bone.transform.position;

            float distance = Vector3.Distance(targetPosition, bonePosition);

            if (distance < magnitudeConditionDto.Magnitude)
            {
                return true;
            }

            return false;
        }

        private bool HandleBoneRotation(BoneRotationConditionDto boneRotationConditionDto)
        {
            Quaternion boneRotation;
            if ((trackedSkeletonService as TrackedSkeleton).bones.TryGetValue(boneRotationConditionDto.BoneId, out TrackedSkeletonBone bone))
                boneRotation = bone.transform.rotation;
            else
                return false;

            if (Quaternion.Angle(boneRotation, boneRotationConditionDto.Rotation.Quaternion()) < boneRotationConditionDto.AcceptanceRange)
            {
                return true;
            }

            return false;
        }

        private bool HandleTargetScale(ScaleConditionDto scaleConditionDto)
        {
            UMI3DNodeInstance targetNodeInstance = UMI3DEnvironmentLoader.Instance
                                            .GetEntityInstance(scaleConditionDto.TargetId)
                                            as UMI3DNodeInstance;

            Vector3 targetScale = targetNodeInstance.transform.localScale;
            Vector3 wantedScale = scaleConditionDto.Scale.Struct();

            if (targetScale.sqrMagnitude <= wantedScale.sqrMagnitude)
            {
                return true;
            }

            return false;
        }
    }
}