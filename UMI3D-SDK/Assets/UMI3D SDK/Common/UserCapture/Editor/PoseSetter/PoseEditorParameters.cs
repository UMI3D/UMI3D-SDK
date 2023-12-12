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

namespace umi3d.common.userCapture.pose.editor
{
    /// <summary>
    /// Global parameters for pose editor
    /// </summary>
    public static class PoseEditorParameters
    {
        public static readonly string SKELETON_PREFAB_PATH = @"Assets/UMI3D SDK/Common/UserCapture/Editor/PoseSetter/SkeletonForPoseSetter.prefab";
        public static readonly string POSE_FORMAT_EXTENSION = "umi3dpose";

        public const string DEFAULT_POSE_NAME = "NewPose";
        public const string DEFAULT_UNSAVED_POSE_NAME = "Unsaved Pose";
    }
}