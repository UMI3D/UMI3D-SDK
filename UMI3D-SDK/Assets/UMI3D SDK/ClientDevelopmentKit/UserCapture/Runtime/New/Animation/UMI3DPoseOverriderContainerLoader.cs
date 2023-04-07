using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class UMI3DPoseOverriderContainerLoader : AbstractLoader, IEntity
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Interaction;
        /// <summary>
        /// When the condtions of a pose are satisfied,
        /// returns the right pose overrider
        /// </summary>
        public event Action<PoseOverriderDto> onConditionValidated;
        bool isActive = false;
        public List<PoseOverriderDto> poseOverriderDtos = new List<PoseOverriderDto>();

        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        ulong overriderID;

        /// <summary>
        /// Init the IDs, inits the overriders, registers this entity to the environnement loader
        /// </summary>
        /// <param name="uMI3DOverriderMetaClassDto"></param>
        private void InitDefinition(UMI3DPoseOverriderContainerDto uMI3DOverriderMetaClassDto)
        {
            overriderID = uMI3DOverriderMetaClassDto.id;
            poseOverriderDtos.AddRange(uMI3DOverriderMetaClassDto.poseOverriderDtos);
            UMI3DEnvironmentLoader.Instance.RegisterEntity(this.overriderID, uMI3DOverriderMetaClassDto, this).NotifyLoaded();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DPoseOverriderContainerDto;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            switch (value.dto)
            {
                case UMI3DPoseOverriderContainerDto uMI3DOverriderMetaClassDto:
                    InitDefinition(uMI3DOverriderMetaClassDto);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ReceivePoseOverriders:
                    InitDefinition(value.entity.dto as UMI3DPoseOverriderContainerDto);
                    break;
            }

            return Task.FromResult(true);

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ReceivePoseOverriders:
                    ulong id = UMI3DSerializer.Read<ulong>(value.container);
                    PoseOverriderDto[] dtos = UMI3DSerializer.ReadArray<PoseOverriderDto>(value.container);
                    InitDefinition(new UMI3DPoseOverriderContainerDto() { id = id, poseOverriderDtos = dtos });
                    break;
            }

            return Task.FromResult(true);
        }

        #region CheckConditions
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
        public IEnumerator CheckCondtionOfAllOverriders()
        {
            isActive = true;
            while (isActive)
            {
                poseOverriderDtos.ForEach(po =>
                {
                    if (CheckConditions(po.poseConditions))
                    {
                        onConditionValidated.Invoke(po);
                    }
                });

                yield return new WaitForSeconds(0.2f);
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
                case UserScaleConditionDto userScaleConditinoDto:

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
            UMI3DNodeInstance targetNodeInstance = UMI3DEnvironmentLoader.Instance
                                                        .GetEntityInstance(magnitudeConditionDto.targetObjectId) 
                                                        as UMI3DNodeInstance;
            if (targetNodeInstance == null)
            {
                UMI3DLogger.LogError("you havent referenced the right entity ID in your magnitude DTO", scope);
                return false;
            }
            Vector3 targetPosition = targetNodeInstance.transform.position;
            Vector3 bonePosition = PersonalSkeleton.Instance.TrackedSkeleton.bones[magnitudeConditionDto.boneOrigine].transform.position;
            float distance = Vector3.Distance(targetPosition, bonePosition);

            if (distance < magnitudeConditionDto.magnitude) 
            {
                return true;
            }

            return false;
        }

        private bool HandleBoneRotation(BoneRotationConditionDto boneRotationConditionDto)
        {
            Quaternion boneRotation = PersonalSkeleton.Instance.TrackedSkeleton.bones[boneRotationConditionDto.boneId].transform.rotation;

            if (Quaternion.Angle(boneRotation, boneRotationConditionDto.rotation.ToQuaternion()) < boneRotationConditionDto.acceptanceRange) 
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
            Vector3 wantedScale = scaleConditionDto.scale;

            if (targetScale.sqrMagnitude <= wantedScale.sqrMagnitude)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
