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

        public override UMI3DVersion.VersionCompatibility version => throw new System.NotImplementedException();

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            throw new System.NotImplementedException();
        }

        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            throw new System.NotImplementedException();
        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            throw new System.NotImplementedException();
        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            throw new System.NotImplementedException();
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
