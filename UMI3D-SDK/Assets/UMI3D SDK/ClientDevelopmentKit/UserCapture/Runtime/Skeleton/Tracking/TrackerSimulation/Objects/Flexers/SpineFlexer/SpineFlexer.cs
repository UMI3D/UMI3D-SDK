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

using inetum.unityUtils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking.constraint
{
    /// <summary>
    /// Flexer that handles the rotation of the spine based on the head movement.
    /// </summary>
    public class SpineFlexer : IFlexer
    {
        private readonly ISkeleton skeleton;
        private readonly ICoroutineService coroutineService;

        public SpineFlexer(ISkeleton skeleton) 
            : this(skeleton, CoroutineManager.Instance)
        {
        }

        public SpineFlexer(ISkeleton skeleton, ICoroutineService coroutineService)
        {
            this.skeleton = skeleton;
            this.coroutineService = coroutineService;
        }

        /// <summary>
        /// Data storage for trackers.
        /// </summary>
        private class ControlledBone
        {
            /// <summary>
            /// Affected bone.
            /// </summary>
            public uint boneType;

            /// <summary>
            /// Null if no controlled is applied by the flexer.
            /// </summary>
            public SimulatedTracker controller;
        }

        private readonly Dictionary<uint, ControlledBone> controlledBones = new();

        /// <summary>
        /// Game object that imick the hips for the fake controllers to be rotated around.
        /// </summary>
        private GameObject spineFlexerBaseGO;

        /// <summary>
        /// Raised when a spine flexer is enabled.
        /// </summary>
        /// One per browser so static for quick access.
        public static event System.Action Enabled;

        /// <summary>
        /// Raised when a spine flexer is disabled.
        /// </summary>
        public static event System.Action Disabled;

        public bool IsEnabled => isEnabled;

        /// <inheritdoc/>
        public void Enable()
        {
            if (isEnabled)
                return;

            // don't enable spine flexer if a controller already exist on spine.
            if (skeleton.TrackedSubskeleton.Controllers.ContainsKey(BoneType.Spine)
             || skeleton.TrackedSubskeleton.Controllers.ContainsKey(BoneType.Chest)
             || skeleton.TrackedSubskeleton.Controllers.ContainsKey(BoneType.UpperChest))
            {
                UMI3DLogger.LogWarning("Spine Flexer : A controller already exist on spine. Cannot automatically flex spine.", DebugScope.CDK | DebugScope.UserCapture);
                return;
            }
                
            // create a parent game object and stores the fake controllers within it
            spineFlexerBaseGO = new("Spine Flexer");

            CreateFakeController(BoneType.Spine, spineFlexerBaseGO.transform, out GameObject spineGo);
            CreateFakeController(BoneType.Chest, spineGo.transform, out GameObject chestGo);
            CreateFakeController(BoneType.UpperChest, spineGo.transform, out _);

            void CreateFakeController(uint boneType, Transform parentTransform, out GameObject createdGo)
            {
                SimulatedTracker simulatedTracker = new GameObject($"Spine Flexer Controller - {BoneTypeHelper.GetBoneName(boneType)}").AddComponent<SimulatedTracker>();
                simulatedTracker.transform.SetParent(parentTransform, false);

                simulatedTracker.Init(boneType);
                simulatedTracker.isActive = true;
                simulatedTracker.isOverrider = true; // required to overwrite IK data

                simulatedTracker.GameObject.transform.SetPositionAndRotation(skeleton.Bones[boneType].Position, skeleton.Bones[boneType].Rotation);
                controlledBones.Add(boneType, new() { boneType = boneType, controller = simulatedTracker });

                skeleton.TrackedSubskeleton.ReplaceController(simulatedTracker.Controller);
                createdGo = simulatedTracker.gameObject;
            }

            // because this flexer affects the controller, it should have its effects applied before the IK
            isEnabled = true;
            flexCoroutine = coroutineService.AttachCoroutine(FlexCoroutine());

            Enabled?.Invoke();
        }

        /// <inheritdoc/>
        public void Disable()
        {
            if (!isEnabled)
                return;

            // reset and destroy used fake controllers
            foreach (var bone in controlledBones.Values.ToList())
            {
                DeleteFakeController(bone.boneType);
            }

            void DeleteFakeController(uint boneType)
            {
                if (controlledBones.TryGetValue(boneType, out var controlledBone))
                {
                    skeleton.TrackedSubskeleton.RemoveController(boneType);

                    if (controlledBone.controller != null)
                        UnityEngine.Object.Destroy(controlledBone.controller);

                    controlledBones.Remove(boneType);
                    return;
                }
            }

            UnityEngine.Object.Destroy(spineFlexerBaseGO);

            coroutineService.DetachCoroutine(flexCoroutine);
            isEnabled = false;

            Disabled?.Invoke();
        }

        private bool isEnabled;
        private Coroutine flexCoroutine;

        /// <summary>
        /// Routine appplying the spine flexing.
        /// </summary>
        /// <returns></returns>
        private IEnumerator FlexCoroutine()
        {
            while (isEnabled)
            {
                Flex();
                yield return null;
            }
        }

        /// <inheritdoc/>
        public void Flex()
        {
            if (!isEnabled)
                return;

            // control the spine based on the head
            if (!skeleton.TrackedSubskeleton.Controllers.TryGetValue(BoneType.Head, out IController headController)
                && !skeleton.TrackedSubskeleton.Controllers.TryGetValue(BoneType.Viewpoint, out headController))
            {
                UMI3DLogger.LogWarning("Spine Flexer : No Controller for head nor viewpoint. Cannot flex spine.", DebugScope.CDK | DebugScope.UserCapture);
                return;
            }

            // divide the Y-rotation of the head on spine, chest, and upperchest
            Quaternion originalHeadRotation = headController.rotation;
            Quaternion hipsRotation = skeleton.Bones[BoneType.Hips].Rotation;
            Quaternion headFromHipsRotation = Quaternion.FromToRotation(Vector3.ProjectOnPlane((hipsRotation * Vector3.forward).normalized, Vector3.up),
                                                                Vector3.ProjectOnPlane((originalHeadRotation * Vector3.forward).normalized, Vector3.up));

            float yRotation = inetum.unityUtils.math.RotationUtils.ProjectAngleIn180Range(headFromHipsRotation.eulerAngles.y); // to correctly divide the rotation

            spineFlexerBaseGO.transform.position = skeleton.Bones[BoneType.Hips].Position;
            // position controllers correctly, need to follow movement
            foreach (var bone in controlledBones.Values)
            {
                if (bone.controller != null)
                    bone.controller.GameObject.transform.position = skeleton.Bones[bone.boneType].Position;
            }

            // limitations of rotation will be applied by the muscles system
            if (!skeleton.TrackedSubskeleton.Controllers.TryGetValue(BoneType.Hips, out IController hipsControllerConstrained) || hipsControllerConstrained == null)
                return;

            spineFlexerBaseGO.transform.rotation = hipsControllerConstrained.rotation;
            controlledBones[BoneType.Spine].controller.GameObject.transform.localRotation = Quaternion.AngleAxis(yRotation / 3, Vector3.up);
            controlledBones[BoneType.Chest].controller.GameObject.transform.localRotation = Quaternion.AngleAxis(yRotation / 3, Vector3.up);
            controlledBones[BoneType.UpperChest].controller.GameObject.transform.localRotation = Quaternion.AngleAxis(yRotation / 3, Vector3.up);
        }
    }
}