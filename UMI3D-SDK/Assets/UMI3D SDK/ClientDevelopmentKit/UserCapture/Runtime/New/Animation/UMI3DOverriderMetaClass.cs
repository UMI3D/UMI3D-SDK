using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class UMI3DOverriderMetaClass : AbstractLoader, IEntity
    {
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

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DOverriderMetaClassDto;
        }

        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            switch (value.dto)
            {
                case UMI3DOverriderMetaClassDto uMI3DOverriderMetaClassDto:

                    break;
            }

            if (overidderMetaClassInstance is not null)
            {
                UMI3DEnvironmentLoader.Instance.RegisterEntity(overidderMetaClassInstance.overriderID, value.dto, overidderMetaClassInstance).NotifyLoaded();
                overidderMetaClassInstance.Init();
            }
            return Task.CompletedTask;
        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ReceivePoseOverriders:

                    break;
            }

            return Task.FromResult(true);

        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ReceivePoseOverriders:

                    break;
            }

            return Task.FromResult(true);
        }

        public void EnableCheck()
        {
            this.isActive = true;
        }

        public void DisableCheck()
        {
            this.isActive = false;
        }

        /// <summary>
        /// return -1 if there is no pose playable,
        /// overwise returns the index of the playable pose
        /// </summary>
        public IEnumerator CheckCondtionOfAllOverriders()
        {
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
            // TODO -- LOGIC 

            return false;
        }
    }
}
