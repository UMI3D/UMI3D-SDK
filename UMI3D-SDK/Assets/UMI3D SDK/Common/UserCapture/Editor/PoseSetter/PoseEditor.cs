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
using UnityEngine;

namespace umi3d.common.userCapture.pose.editor
{
    public class PoseEditor
    {
        private readonly PoseEditorSkeleton skeleton;
        public PoseEditorSkeleton Skeleton => skeleton;

        private readonly PoseEditionService poseEditionService;
        private readonly PoseSaverService poseSaverService;

        public PoseEditor() : base()
        {
            this.poseEditionService = new();
            this.poseSaverService = new();
            this.skeleton = new PoseEditorSkeleton();
        }

        public void CreatePose()
        {
            poseSaverService.CreatePose(skeleton);
        }

        public void SavePose(string poseName, string path)
        {
            if (skeleton.boneComponents.Where(bc => bc.isRoot).Count() == 0)
                poseEditionService.ResetRoot(skeleton);

            poseSaverService.SavePose(skeleton, path, poseName, out bool success);
        }

        public void LoadPose(UMI3DPose_so pose, out bool success)
        {
            poseSaverService.LoadPose(skeleton, pose, out success);
            if (!success)
                return;

            poseEditionService.ResetAllBones(skeleton);

            PoseSetterBoneComponent root_boneComponent = skeleton.boneComponents.Find(bc => bc.BoneType == pose.GetBonePoseCopy().bone);
            root_boneComponent.isRoot = true;

            poseEditionService.UpdateBoneRotation(skeleton, pose.GetBonePoseCopy());

            pose.GetBonesCopy().ForEach((bone) => poseEditionService.UpdateBoneRotation(skeleton, bone));
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
            poseEditionService.UpdateSkeletonGameObject(skeleton, value);
        }

        public void UpdateIsRoot(uint boneType, bool isRoot)
        {
            poseEditionService.ChangeIsRoot(skeleton, boneType, isRoot);
        }

    }
}
#endif
