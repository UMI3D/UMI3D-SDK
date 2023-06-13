using System.Collections;
using umi3d.common.userCapture;
using umi3d.common;
using UnityEngine;
using System;
using System.Threading;
using inetum.unityUtils;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Pose overrider handler to calculate whether the pose can be played or not depending on the set of condition of a specific container 
    /// </summary>
    public class PoseOverriderContainerHandlerUnit
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        private ISkeletonManager skeletonManager;

        bool isActive = false;
        UMI3DPoseOverriderContainerDto poseOverriderContainerDto;

        public event Action<PoseOverriderDto> onConditionValidated;

        CancellationTokenSource cancellationTokenSource;

        public void SetPoseOverriderContainer(UMI3DPoseOverriderContainerDto poseOverriderContainerDto)
        {
            this.poseOverriderContainerDto = poseOverriderContainerDto;
        }
        /// <summary>
        /// Stops the check of fall the overriders 
        /// </summary>
        public void DisableCheck()
        {
            this.isActive = false;
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
                skeletonManager = PersonalSkeletonManager.Instance as ISkeletonManager;
                isActive = true;
                PoseManager.StartCoroutine(LaunchCheck());
            }
        }

        private IEnumerator LaunchCheck()
        {
            while (isActive)
            {
                yield return new WaitForSeconds(0.2f);
                foreach (PoseOverriderDto poseOverrider in poseOverriderContainerDto.poseOverriderDtos)
                {
                    if (CheckConditions(poseOverrider.poseConditions))
                    {
                        onConditionValidated.Invoke(poseOverrider);
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
                case DirectionConditionDto directionConditionDto:

                    break;

                case ScaleConditionDto scaleConditionDto:
                    return HandleTargetScale(scaleConditionDto);
                case RangeConditionDto rangeConditionDto:

                    break;

                case NotConditionDto notConditionDto:

                    break;
            }

            return false;
        }

        private bool HandleMagnitude(MagnitudeConditionDto magnitudeConditionDto)
        {
            UMI3DNodeInstance targetNodeInstance = null;

            targetNodeInstance = UMI3DEnvironmentLoader.Instance.GetNodeInstance(magnitudeConditionDto.targetObjectId);

            if (targetNodeInstance == null)
            {
                UMI3DLogger.LogError("you havent referenced the right entity ID in your magnitude DTO", scope);
                return false;
            }

            Vector3 targetPosition = targetNodeInstance.transform.position;


            Vector3 bonePosition = skeletonManager.personalSkeleton.TrackedSkeleton.bones[magnitudeConditionDto.boneOrigine].transform.position;
            float distance = Vector3.Distance(targetPosition, bonePosition);

            if (distance < magnitudeConditionDto.magnitude)
            {
                return true;
            }

            return false;
        }

        private bool HandleBoneRotation(BoneRotationConditionDto boneRotationConditionDto)
        {
            Quaternion boneRotation = skeletonManager.personalSkeleton.TrackedSkeleton.bones[boneRotationConditionDto.boneId].transform.rotation;

            if (Quaternion.Angle(boneRotation, boneRotationConditionDto.rotation.Quaternion()) < boneRotationConditionDto.acceptanceRange)
            {
                return true;
            }

            return false;
        }


        private bool HandleTargetScale(ScaleConditionDto scaleConditionDto)
        {
            UMI3DNodeInstance targetNodeInstance = UMI3DEnvironmentLoader.Instance
                                            .GetEntityInstance(scaleConditionDto.targetId)
                                            as UMI3DNodeInstance;

            Vector3 targetScale = targetNodeInstance.transform.localScale;
            Vector3 wantedScale = scaleConditionDto.scale.Struct();

            if (targetScale.sqrMagnitude <= wantedScale.sqrMagnitude)
            {
                return true;
            }

            return false;
        }
    }
}
