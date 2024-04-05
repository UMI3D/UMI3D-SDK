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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.tracking.constraint;
using umi3d.edk;
using umi3d.edk.core;
using umi3d.edk.userCapture.pose;
using umi3d.edk.userCapture.tracking.constraint;
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
                {
                    if (pose_so == null)
                        return null;
                    pose = new UMI3DPoseResource(pose_so);
                }

                return pose;
            }
            set
            {
                pose = value;
            }
        }

        private UMI3DAsyncProperty<PoseClip> _PoseClip;

        private PoseClip _poseClip
        {
            get
            {
                return Pose == null ? null : poseService.RegisterEnvironmentPose(Pose);
            }
        }

        /// <summary>
        /// Pose animation controlled by the pose animator.
        /// </summary>
        public UMI3DAsyncProperty<PoseClip> PoseClip
        {
            get
            {
                if (_PoseClip.GetValue() == null)
                {
                    _PoseClip.SetValue(_poseClip);
                }

                return _PoseClip;
            }
        }

        [Space(5)]
        [SerializeField, Tooltip("Specifying if the pose is relative to another object."), EditorReadOnly]
        private bool isAnchored;
        public UMI3DAsyncProperty<bool> IsAnchored { get; protected set; }

        [SerializeField, Tooltip("Specifying if the pose is relative to another object."), EditorReadOnly]
        private PoseAnchoringParameters anchoringParameters = new();
        public UMI3DAsyncProperty<IBoneConstraint> AnchoringConstraint { get; protected set; }

        [Serializable]
        public class PoseAnchoringParameters
        {
            [Tooltip("Defining the type of object for the pose anchor.")]
            public PoseAnchoringType anchoringType;

            // -- case NodeBoneConstraint
            [Header("Node constraint")]
            [Tooltip("Node bone constraint to apply when pose is played.")]
            public UMI3DNodeBoneConstraint nodeBoneConstraint;
            // --

            // case BoneBoneConstraint
            private BoneBoneConstraint boneBoneConstraint;

            [Header("Other bone constraint")]
            [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Specifying the BoneType which serves as reference for the pose.")]
            public uint anchoringBone;

            [System.Serializable]
            public struct OffsetData
            {
                public Vector3 position;
                public Quaternion rotation;
            }

            [Space(10), Tooltip("The relative position & rotation of the pose.")]
            public OffsetData anchoringOffset;

            // --

            public IBoneConstraint GetBoneConstraint(GameObject component, uint rootBone, UMI3DUser user)
            {
                switch (anchoringType)
                {
                    case PoseAnchoringType.Node:
                        {
                            if (nodeBoneConstraint != null)
                                return nodeBoneConstraint;

                            if (component.TryGetComponent(out UMI3DNodeBoneConstraint constraint))
                            {
                                nodeBoneConstraint = constraint;
                            }
                            else
                            {
                                nodeBoneConstraint = component.AddComponent<UMI3DNodeBoneConstraint>();
                                nodeBoneConstraint.Init();
                                nodeBoneConstraint.ConstrainedBone.SetValue(rootBone);
                                nodeBoneConstraint.ConstrainingNode.SetValue(component.GetComponent<UMI3DNode>());
                                nodeBoneConstraint.PositionOffset.SetValue(anchoringOffset.position);
                                nodeBoneConstraint.RotationOffset.SetValue(anchoringOffset.rotation);
                            }
                            return nodeBoneConstraint;
                        }
                    case PoseAnchoringType.Bone:
                        {
                            boneBoneConstraint ??= new BoneBoneConstraint();
                            boneBoneConstraint.ConstrainedBone.SetValue(rootBone);
                            boneBoneConstraint.ConstrainingBone.SetValue(anchoringBone);
                            boneBoneConstraint.PositionOffset.SetValue(anchoringOffset.position);
                            boneBoneConstraint.RotationOffset.SetValue(anchoringOffset.rotation);
                            return boneBoneConstraint;

                        }
                    default:
                        return null;
                }
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

        [Space(5)]
        [SerializeField, Tooltip("Expected duration of the pose animation."), EditorReadOnly]
        private Duration duration;
        public UMI3DAsyncProperty<Duration> PoseApplicationDuration { get; protected set; }

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
        [SerializeField, Tooltip("How the pose animator could be activated by the user."), EditorReadOnly]
        private PoseAnimatorActivationMode activationMode;
        public UMI3DAsyncProperty<PoseAnimatorActivationMode> ActivationMode { get; protected set; }

        private IReadOnlyList<AbstractBrowserPoseAnimatorActivationCondition> SerializableActivationConditions
            => activationConditions.Select(x => BrowserPoseAnimatorActivationConditionField.ToCondition(x)).ToList();

        /// <summary>
        /// Used for serialization and editor access. Prefer to use <see cref="UMI3DPoseAnimator.ActivationConditions"/>.
        /// </summary>
        [Tooltip("Pose animator activation conditions, all of them should be validated for the animator to be activated."), EditorReadOnly]
        public List<BrowserPoseAnimatorActivationConditionField> activationConditions = new();

        #endregion Activation

        /// <summary>
        /// Pose animator activation conditions, all of them should be validated for the animator to be activated.
        /// </summary>
        public UMI3DAsyncListProperty<IPoseAnimatorActivationCondition> ActivationConditions { get; protected set; }

        #region Dependencies

        private IPoseManager _poseService;

        private IPoseManager poseService
        {
            get
            {
                if (_poseService == null)
                    _poseService = PoseManager.Instance;
                return _poseService;
            }
            set => _poseService = value;
        }

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            _PoseClip = new UMI3DAsyncProperty<PoseClip>(Id(), UMI3DPropertyKeys.PoseAnimatorPoseClip, _poseClip);
            IsAnchored = new UMI3DAsyncProperty<bool>(Id(), UMI3DPropertyKeys.PoseAnimatorUseAnchoring, isAnchored);
            uint rootBone = PoseClip.GetValue()?.PoseResource.GetValue().Anchor.bone ?? BoneType.None;
            AnchoringConstraint = new UMI3DAsyncProperty<IBoneConstraint>(Id(), UMI3DPropertyKeys.PoseAnimatorAnchoringConstraint, anchoringParameters.GetBoneConstraint(gameObject, rootBone, null));
            PoseApplicationDuration = new UMI3DAsyncProperty<Duration>(Id(), UMI3DPropertyKeys.PoseAnimatorApplicationDuration, duration);
            ActivationMode = new UMI3DAsyncProperty<PoseAnimatorActivationMode>(Id(), UMI3DPropertyKeys.PoseAnimatorActivationMode, activationMode);
            ActivationConditions = new UMI3DAsyncListProperty<IPoseAnimatorActivationCondition>(Id(), UMI3DPropertyKeys.PoseAnimatorActivationConditions, SerializableActivationConditions.Cast<IPoseAnimatorActivationCondition>().ToList());
        }

        #endregion Dependencies

        public virtual PoseAnimatorDto ToDto(UMI3DUser user)
        {
            return new PoseAnimatorDto()
            {
                id = Id(),
                relatedNodeId = RelativeNode.Id(),
                isAnchored = IsAnchored.GetValue(user),
                boneConstraintId = AnchoringConstraint.GetValue(user)?.Id() ?? default,
                poseClipId = PoseClip.GetValue(user).Id(),
                poseConditions = ActivationConditions.GetValue(user).Select(x => x.ToDto()).ToArray(),
                duration = PoseApplicationDuration.GetValue(user).ToDto(),
                activationMode = (ushort)ActivationMode.GetValue(user),
            };
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto(user);
        }
    }
}