using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.edk;
using umi3d.edk.interaction;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class PoseOverriderLinkerHub : UMI3DPoseManager
    {
        public List<OverriderContainerField> allPoseOverriders = new List<OverriderContainerField>();

        protected override void Awake()
        {
            base.Awake();
            InitEachPoseAnimationWithPoseOverriderContainer();
        }

        private void InitEachPoseAnimationWithPoseOverriderContainer()
        {
            for (int i = 0; i < allPoseOverriders.Count; i++)
            {
                allPoseOverriders[i].PoseOverriderContainer.Id();
                if (allPoseOverriders[i].uMI3DEvent.GetComponent<UMI3DPoseOverriderAnimation>() == null)
                {
                    allPoseOverriders[i].uMI3DEvent.gameObject.AddComponent<UMI3DPoseOverriderAnimation>()
                                                    .Init(allPoseOverriders[i].PoseOverriderContainer);
                }

                allPoseOverriders[i].SetNode();
                allPoseOverriderContainer.Add(allPoseOverriders[i].PoseOverriderContainer.ToDto());
            }
        }

        [Serializable]
        public class OverriderContainerField
        {
            [SerializeField] UMI3DPoseOverriderContainer poseOverriderContainer;
            [SerializeField] UMI3DEvent _uMI3DEvent;

            public UMI3DPoseOverriderContainer PoseOverriderContainer { get => poseOverriderContainer; }
            public UMI3DEvent uMI3DEvent { get => _uMI3DEvent;  }

            public void SetNode()
            {
                PoseOverriderContainer.nodeID = uMI3DEvent.GetComponent<UMI3DModel>().Id();
                PoseOverriderContainer.eventID = uMI3DEvent.Id();
            }
        }
    }
}
