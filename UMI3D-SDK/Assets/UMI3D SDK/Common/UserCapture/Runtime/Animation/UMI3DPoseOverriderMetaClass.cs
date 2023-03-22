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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace umi3d.common.userCapture
{
    [Serializable]
    public class UMI3DPoseOverriderMetaClass : IEntity
    {
        [SerializeField] List<UMI3DPoseOveridder_so> poseOverriders = new List<UMI3DPoseOveridder_so>();

        List<PoseOverriderDto> poseOverridersDtos = new List<PoseOverriderDto>();

        /// <summary>
        /// When the condtions of a pose are satisfied,
        /// returns the right pose overrider
        /// </summary>
        public event Action<PoseOverriderDto> onConditionValidated;

        bool isActive = false;

        public void Init()
        {
            poseOverridersDtos.Clear();
            poseOverriders.ForEach(po =>
            {
                po.pose.onPoseReferencedAndIndexSetted += (indexInPoseManager) =>
                {
                    poseOverridersDtos.Add(po.ToDto(indexInPoseManager));
                };
            });
        }

        public void AddPoseOveriderDtos(List<PoseOverriderDto> poseOverriderDtos)
        {
            this.poseOverridersDtos.AddRange(poseOverriderDtos);
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
                poseOverridersDtos.ForEach(po =>
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

