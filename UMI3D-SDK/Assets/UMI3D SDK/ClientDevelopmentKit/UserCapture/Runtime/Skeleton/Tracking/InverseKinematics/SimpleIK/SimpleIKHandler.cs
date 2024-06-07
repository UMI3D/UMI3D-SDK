/*
Copyright 2019 - 2024 Inetum

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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking.ik
{
    /// <summary>
    /// Handle basic IK in a simplified way : only for hands and foots.
    /// </summary>
    /// This handler does not uses a Unity Animator
    /// and thus can be used outside of OnAnimatorIK message in the Unity loop.
    public class SimpleIKHandler : MonoBehaviour, IIKHandler
    {
        /// <summary>
        /// Node containing the IK gameObjects
        /// </summary>
        private GameObject IKNode;

        /// <summary>
        /// Skeleton on which IK is applied.
        /// </summary>
        private ISkeleton skeleton;

        /// <summary>
        /// IK controllers for each managed bone type.
        /// </summary>
        public IReadOnlyDictionary<uint, SimpleIKController> IKControllers => ikControllers;

        private readonly Dictionary<uint, SimpleIKController> ikControllers = new();

        /// <summary>
        /// Parameters for IK Controllers
        /// </summary>
        private struct IKControlParameters
        {
            /// <summary>
            /// Bone that receive the IK.
            /// </summary>
            public uint goalBoneType;

            /// <summary>
            /// Joints rotated by IK.
            /// </summary>
            public (uint first, uint intermerdiary) affectedBones;

            /// <summary>
            /// Relative position of the hint in the reference frame of the Hips.
            /// </summary>
            public Vector3 hintOffset;

            /// <summary>
            /// If joints are not aligned on Z-axis.
            /// </summary>
            public Quaternion rotationOffset;
        }

        /// <summary>
        /// IK Parameters for each managed bone.
        /// </summary>
        private readonly Dictionary<uint, IKControlParameters> ikParameters = new()
        {
            { BoneType.LeftHand, new()
                {
                    goalBoneType = BoneType.LeftHand,
                    affectedBones = (BoneType.LeftUpperArm, BoneType.LeftForearm),
                    hintOffset = 0.5f * Vector3.left,
                    rotationOffset = Quaternion.FromToRotation(Vector3.forward, Vector3.right) // bones on left arm are initially aligned with -X axis.
                }
            },
            { BoneType.RightHand, new()
                {
                    goalBoneType = BoneType.RightHand,
                    affectedBones = (BoneType.RightUpperArm, BoneType.RightForearm),
                    hintOffset = 0.5f * Vector3.right,
                    rotationOffset = Quaternion.FromToRotation(Vector3.forward, Vector3.left) // bones on left arm are initially aligned with +X axis.
                }
            },
            { BoneType.LeftAnkle, new()
                {
                    goalBoneType = BoneType.LeftAnkle,
                    affectedBones = (BoneType.LeftHip, BoneType.LeftKnee),
                    hintOffset = 0.08f * Vector3.left + 0.5f * Vector3.forward + 0.5f * Vector3.down,
                    rotationOffset = Quaternion.FromToRotation(Vector3.forward, Vector3.up) // bones on legs are initially aligned with -Y axis.
                }
            },
            { BoneType.RightAnkle, new()
                {
                    goalBoneType = BoneType.RightAnkle,
                    affectedBones = (BoneType.RightHip, BoneType.RightKnee),
                    hintOffset = 0.08f * Vector3.right + 0.5f * Vector3.forward + 0.5f * Vector3.down,
                    rotationOffset = Quaternion.FromToRotation(Vector3.forward, Vector3.up) // bones on legs are initially aligned with -Y axis.
                }
            },
        };

        #region LifeCycle

        #region Initialization

        private bool inited;

        /// <summary>
        /// To call before starting to use the component.
        /// </summary>
        /// <param name="skeleton">Receiving IK skeleton.</param>
        public void Init(ISkeleton skeleton)
        {
            this.skeleton = skeleton;

            IKNode = new GameObject("IK Hints");
            IKNode.transform.SetParent(skeleton.Bones[BoneType.Hips].Transform); // hints are relative to hips
            IKNode.transform.localPosition = Vector3.zero;
            IKNode.transform.localRotation = Quaternion.identity;

            SetupIK(BoneType.LeftHand);
            SetupIK(BoneType.RightHand);

            SetupIK(BoneType.LeftAnkle);
            SetupIK(BoneType.RightAnkle);

            inited = true;
        }

        private SimpleIKController SetupIK(uint goalBoneType)
        {
            string name = BoneTypeHelper.GetBoneName(goalBoneType);

            SimpleIKController ikController = new(skeleton.Bones[ikParameters[goalBoneType].affectedBones.first].Transform,
                                                        skeleton.Bones[ikParameters[goalBoneType].affectedBones.intermerdiary].Transform,
                                                        skeleton.Bones[goalBoneType].Transform,
                                                        new GameObject($"IK hint - {name}").transform,
                                                        new GameObject($"IK goal - {name}").transform,
                                                        ikParameters[goalBoneType].rotationOffset);

            ikController.hint.transform.SetParent(IKNode.transform);
            ikController.hint.transform.localPosition = ikParameters[goalBoneType].hintOffset;
            ikController.hint.transform.localRotation = Quaternion.identity;

            ikController.goal.transform.SetParent(IKNode.transform); // goals are positionned before each application anyway
            ikController.goal.transform.position = skeleton.Bones[goalBoneType].Position;

            ikControllers.Add(goalBoneType, ikController);

            return ikController;
        }

        #endregion Initialization

        private void OnDestroy()
        {
            if (IKNode != null)
                UnityEngine.Object.Destroy(IKNode);
        }

        private void Reset()
        {
            // empty unity message because of interface naming
        }

        #endregion LifeCycle

        /// <inheritdoc/>
        public void HandleAnimatorIK(int layerIndex, IController controller)
        {
            if (ikControllers.TryGetValue(controller.boneType, out SimpleIKController ikController))
            {
                ikController.goal.transform.position = controller.position;
                ikController.Apply();
            }
        }

        /// <summary>
        /// <inheritdoc/> <br/>
        /// Called by OnAnimatorIK in TrackedAnimator, set all tracked bones as computed and positions IK hints.
        /// </summary>
        /// <param name="layerIndex"></param>
        public void HandleAnimatorIK(int layerIndex, IEnumerable<IController> controllers)
        {
            foreach (var controller in controllers)
            {
                HandleAnimatorIK(0, controller);
            }
        }

        /// <inheritdoc/>
        public void Reset(IEnumerable<IController> controllers, IDictionary<uint, TrackedSubskeletonBone> bones)
        {
            foreach ((uint bone, SimpleIKController ikController) in ikControllers)
            {
                ikController.goal.transform.position = skeleton.Bones[bone].Position;
            }
        }

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!inited)
                return;

            foreach ((uint bone, SimpleIKController ikController) in ikControllers)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(ikController.goal.transform.position, 0.05f);

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(ikController.hint.transform.position, 0.05f);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(ikController.firstJoint.transform.position, 0.025f);

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(ikController.intermediaryJoint.transform.position, 0.025f);

                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(ikController.finalJoint.transform.position, 0.025f);

                Gizmos.color = Color.grey;
                Gizmos.DrawLine(ikController.firstJoint.transform.position, ikController.intermediaryJoint.transform.position);
                Gizmos.DrawLine(ikController.intermediaryJoint.transform.position, ikController.finalJoint.transform.position);
            }
        }

        #endregion Gizmos
    }
}