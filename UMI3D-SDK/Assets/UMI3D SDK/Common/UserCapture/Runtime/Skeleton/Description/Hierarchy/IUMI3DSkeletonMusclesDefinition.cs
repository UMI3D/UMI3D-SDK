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

namespace umi3d.common.userCapture.description
{
    public interface IUMI3DSkeletonMusclesDefinition
    {
        /// <summary>
        /// Rotation restrictions to apply to bones.
        /// </summary>
        public List<Muscle> Muscles { get; }

        /// <summary>
        /// Constraint on a bone rotation.
        /// </summary>
        public struct Muscle
        {
            /// <summary>
            /// Bone type in UMI3D standards.
            /// </summary>
            public uint Bonetype;

            /// <summary>
            /// Offset for restrictions.
            /// </summary>
            /// Typically used for fingers joints.
            public Vector4Dto ReferenceFrameRotation;

            /// <summary>
            /// Rotation restriction on rotation axis X.
            /// </summary>
            public RotationRestriction? XRotationRestriction;

            /// <summary>
            /// Rotation restriction on rotation axis Y.
            /// </summary>
            public RotationRestriction? YRotationRestriction;

            /// <summary>
            /// Rotation restriction on rotation axis Z.
            /// </summary>
            public RotationRestriction? ZRotationRestriction;

            public struct RotationRestriction
            {
                /// <summary>
                /// Lowest tolerated angle in degrees.
                /// </summary>
                public float min;

                /// <summary>
                /// Greatest tolerated angle in degrees.
                /// </summary>
                public float max;
            }
        }
    }
}