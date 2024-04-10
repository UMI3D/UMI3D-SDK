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

namespace umi3d.common.userCapture.tracking.constraint
{
    /// <summary>
    /// Define a constraint on the bone, exerced by another object.
    /// </summary>
    public abstract class AbstractBoneConstraintDto : AbstractEntityDto, IEntity
    {
        /// <summary>
        /// If true, the constraint could be applied if possible.
        /// </summary>
        public bool ShouldBeApplied { get; set; }

        /// <summary>
        /// The bone that should receive the constraint.
        /// </summary>
        public uint ConstrainedBone { get; set; }

        /// <summary>
        /// Positional offset from the constraining object.
        /// </summary>
        public Vector3Dto PositionOffset { get; set; } = new Vector3Dto();

        /// <summary>
        /// Rotational offset from the constraining object.
        /// </summary>
        public Vector4Dto RotationOffset { get; set; } = new Vector4Dto();
    }
}