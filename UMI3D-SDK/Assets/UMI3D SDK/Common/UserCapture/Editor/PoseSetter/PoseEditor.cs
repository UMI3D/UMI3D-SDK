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
#if UNITY_EDITOR

using System.Linq;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.common.userCapture.pose.editor
{
    public class PoseEditor
    {
        private readonly PoseEditorSkeleton skeleton;
        public PoseEditorSkeleton Skeleton => skeleton;

        private readonly HandClosureSkeleton handClosureSkeleton;

        private readonly PoseEditionService poseEditionService;
        private readonly PoseSaverService poseSaverService;

        public PoseEditor() : base()
        {
            this.poseEditionService = new();
            this.poseSaverService = new();
            this.skeleton = new();
            this.handClosureSkeleton = new();
        }

        public void SavePose(string filePath)
        {
            if (skeleton.boneComponents.Where(bc => bc.isRoot).Count() == 0)
                poseEditionService.ResetRoot(skeleton);

            var roots = skeleton.boneComponents.Where(bc => bc.isRoot);

            poseSaverService.SavePose(roots, filePath, out bool success);
        }

        public void LoadPose(string path, out bool success)
        {
            PoseDto pose = poseSaverService.LoadPose(path, out success);
            if (!success)
                return;

            poseEditionService.ResetAllBones(skeleton);

            PoseSetterBoneComponent root_boneComponent = skeleton.boneComponents.Find(bc => bc.BoneType == pose.anchor.bone);
            root_boneComponent.isRoot = true;
            skeleton.anchor = pose.anchor.bone;

            poseEditionService.UpdateBoneRotation(skeleton, pose.anchor);

            pose.bones.ForEach((bone) => poseEditionService.UpdateBoneRotation(skeleton, bone));
            success = true;
        }

        public void ResetSkeleton(GameObject skeletonRootGo)
        {
            poseEditionService.ResetAllBones(skeleton);
            poseEditionService.ResetSkeleton(skeleton, skeletonRootGo);
        }

        public void ApplySymmetry(bool isLeft, SymmetryTarget target)
        {
            poseEditionService.ApplySymmetry(skeleton, isLeft, target);
        }

        public void UpdateSkeletonGameObject(GameObject value)
        {
            poseEditionService.UpdateSkeletonGameObject(skeleton, handClosureSkeleton, value);
        }

        public void UpdateIsRoot(uint boneType, bool isRoot)
        {
            poseEditionService.ChangeIsRoot(skeleton, boneType, isRoot);
        }

        public void CloseFinger(uint handBoneType, HandClosureGroup fingerGroup, float closureRate)
        {
            poseEditionService.CloseFinger(skeleton, handClosureSkeleton, handBoneType, fingerGroup, closureRate);
        }
    }
}
#endif
