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
using System.Threading.Tasks;

using umi3d.common;
using umi3d.common.userCapture.description;

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Manages tracker simulation for a skeleton.
    /// </summary>
    internal class TrackerSimulator : ITrackerSimulator
    {
        private struct SimulatedTrackingRecord
        {
            public ISimulatedTracker simulatedTracker;
            public HashSet<PoseAnchorDto> registeredPoseAnchors;
        }

        private Dictionary<uint, SimulatedTrackingRecord> simulatedTrackingRecords = new();

        #region DI

        private readonly ILoadingManager loadingService;
        private readonly ISkeleton skeleton;

        public TrackerSimulator(ILoadingManager loadingService, ISkeleton skeleton)
        {
            this.loadingService = loadingService;
            this.skeleton = skeleton;
        }

        public TrackerSimulator() : this(UMI3DEnvironmentLoader.Instance, PersonalSkeletonManager.Instance.PersonalSkeleton)
        {
        }

        #endregion DI

        public async Task StartTrackerSimulation(PoseAnchorDto poseAnchor)
        {
            if (poseAnchor == null)
                return;

            if (simulatedTrackingRecords.TryGetValue(poseAnchor.bone, out SimulatedTrackingRecord trackingRecord))
            {
                trackingRecord.registeredPoseAnchors.Add(poseAnchor);
                return;
            }

            ISimulatedTracker tracker = null;
            switch (poseAnchor)
            {
                case NodePoseAnchorDto nodePose:
                    UMI3DNodeInstance nodeReference = (UMI3DNodeInstance)await loadingService.WaitUntilEntityLoaded(UMI3DGlobalID.EnvironmentId, nodePose.node, null);

                    NodeAnchoredSimulatedTracker nodeTracker = new GameObject("NodeTracker" + nodePose.node).AddComponent<NodeAnchoredSimulatedTracker>();
                    nodeTracker.Init(nodeReference, nodePose.bone, nodePose.position.Struct(), nodePose.rotation.Quaternion());
                    tracker = nodeTracker;
                    break;

                case BonePoseAnchorDto bonePose:
                    if (!skeleton.Bones.TryGetValue(bonePose.otherBone, out ISkeleton.Transformation boneReference))
                    {
                        await Task.Run(() => UMI3DLogger.LogWarning("Bone not found for applying BonePoseAnchor.", DebugScope.CDK | DebugScope.UserCapture));
                        break;
                    }

                    BoneAnchoredSimulatedTracker boneTracker = new GameObject("BoneTracker" + bonePose.otherBone).AddComponent<BoneAnchoredSimulatedTracker>();
                    boneTracker.Init(boneReference, bonePose.bone, bonePose.position.Struct(), bonePose.rotation.Quaternion());
                    tracker = boneTracker;
                    break;

                case FloorPoseAnchorDto floorPose:
                    await Task.Run(() => UMI3DLogger.LogWarning("FloorAnchor is not yet implemented.", DebugScope.CDK | DebugScope.UserCapture));
                    return;

                default:
                    // case when anchor does not require any tracker simulation
                    return;
            }

            trackingRecord = new()
            {
                simulatedTracker = tracker,
                registeredPoseAnchors = new HashSet<PoseAnchorDto>() { poseAnchor },
            };
            simulatedTrackingRecords.Add(poseAnchor.bone, trackingRecord);

            skeleton.TrackedSubskeleton.ReplaceController(tracker.Controller, true);

            tracker.Destroyed += (tracker) =>
            {
                OnTrackerDestroyed(tracker.BoneType);
            };
        }

        public void StopTrackerSimulation(PoseAnchorDto poseAnchor)
        {
            if (!simulatedTrackingRecords.TryGetValue(poseAnchor.bone, out SimulatedTrackingRecord simulatedTrackingRecord))
                return;

            simulatedTrackingRecord.registeredPoseAnchors.Remove(poseAnchor);

            if (simulatedTrackingRecord.registeredPoseAnchors.Count > 0)
                return;

            if (simulatedTrackingRecord.simulatedTracker.GameObject != null)
                GameObject.Destroy(simulatedTrackingRecord.simulatedTracker.GameObject);
            else
                OnTrackerDestroyed(poseAnchor.bone);
        }

        private void OnTrackerDestroyed(uint boneType)
        {
            if (simulatedTrackingRecords.ContainsKey(boneType))
                simulatedTrackingRecords.Remove(boneType);

            skeleton.TrackedSubskeleton.RemoveTracker(boneType);
        }
    }
}