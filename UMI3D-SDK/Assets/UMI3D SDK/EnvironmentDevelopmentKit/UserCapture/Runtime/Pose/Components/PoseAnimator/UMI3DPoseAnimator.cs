/*
using UnityEngine;

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

using System.Collections.Generic;
using System.Linq;

using umi3d.edk;
using umi3d.edk.core;
using umi3d.edk.userCapture.pose;

using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Controls a pose animation.
    /// </summary>
    public class UMI3DPoseAnimator : AbstractLoadableComponentEntity, UMI3DLoadableEntity
    {
        /// <summary>
        /// Pose resource in UMI3D standard. Field used for serialization and in-editor setting.
        /// </summary>
        [SerializeField, Tooltip("Pose resource in UMI3DStandard.")]
        public UMI3DPose_so pose_so;

        private IUMI3DPoseData pose;

        /// <summary>
        /// Pose ressource for animation.
        /// </summary>
        public IUMI3DPoseData Pose
        {
            get
            {
                if (pose == null)
                    pose = pose_so;

                return pose;
            }
            set
            {
                pose = value;
            }
        }

        private PoseClip poseClip;

        /// <summary>
        /// Pose animation controlled by the pose animator.
        /// </summary>
        public PoseClip PoseClip
        {
            get
            {
                if (poseClip == null)
                {
                    poseClip = Pose == null ? null : poseService.RegisterEnvironmentPose(Pose);
                }

                return poseClip;
            }
            set
            {
                poseClip = value;
            }
        }

        [System.Serializable]
        public struct Duration
        {
            public ulong duration;

            public bool hasMin;

            public ulong min;

            public bool hasMax;

            public ulong max;

            public DurationDto ToDto()
            {
                return new()
                {
                    max = hasMax ? max : null,
                    min = hasMin ? min : null,
                    duration = duration
                };
            }
        }

        [Tooltip("Expected duration of the pose animation.")]
        public Duration duration;

        [Tooltip("Can the pose animation be interpolated when it is applied?")]
        [HideInInspector]
        public bool interpolable;

        [Tooltip("Can the pose animation be composed with another pose animation?")]
        public bool composable;

        [SerializeField, Tooltip("Related node. If unset, target is current node.")]
        private UMI3DNode relativeNode;

        public UMI3DNode RelativeNode
        {
            get
            {
                if (relativeNode == null)
                    relativeNode = GetComponent<UMI3DNode>();
                return relativeNode;
            }
            set
            { relativeNode = value; }
        }

        #region Activation

        [Space(10), Header("Activation")]
        [Tooltip("How the pose animator could be activated by the user.")]
        public PoseAnimatorActivationMode activationMode;

        /// <summary>
        /// Poses conditions validated by the environment.
        /// </summary>
        [HideInInspector]
        public List<UMI3DEnvironmentPoseCondition> EnvironmentActivationConditions = new();

        public IReadOnlyList<AbstractBrowserPoseAnimatorActivationCondition> BrowserActivationConditions
        {
            get => activationConditions.Select(x => BrowserPoseAnimatorActivationConditionField.ToCondition(x)).ToList();
            set
            {
                activationConditions = value?.Select(x => BrowserPoseAnimatorActivationConditionField.ToField(x)).ToList();
            }
        }

        /// <summary>
        /// Used for serialization and editor access. Prefer to use <see cref="UMI3DPoseAnimator.ActivationsConditions"/>.
        /// </summary>
        public List<BrowserPoseAnimatorActivationConditionField> activationConditions = new();

        #endregion Activation

        /// <summary>
        /// Pose animator activation conditions, all of them should be validated for the animator to be activated.
        /// </summary>
        public IReadOnlyList<IPoseAnimatorActivationCondition> ActivationsConditions
            => EnvironmentActivationConditions.Cast<IPoseAnimatorActivationCondition>().Union(BrowserActivationConditions).ToList();

        #region Dependencies

        private IPoseManager poseService;

        private void Start()
        {
            poseService = PoseManager.Instance;
        }

        #endregion Dependencies

        public virtual PoseAnimatorDto ToDto()
        {
            return new PoseAnimatorDto()
            {
                id = Id(),
                relatedNodeId = RelativeNode.Id(),
                poseClipId = PoseClip.Id(),
                poseConditions = ActivationsConditions.Select(x => x.ToDto()).ToArray(),
                duration = duration.ToDto(),
                isInterpolable = interpolable,
                isComposable = composable,
                activationMode = (ushort)activationMode
            };
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }
    }
}