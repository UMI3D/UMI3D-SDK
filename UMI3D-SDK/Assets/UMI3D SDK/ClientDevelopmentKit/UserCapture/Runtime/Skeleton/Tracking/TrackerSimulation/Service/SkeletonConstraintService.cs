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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture.tracking.constraint
{
    /// <summary>
    /// Register constraints and apply them to the skeleton.
    /// </summary>
    public class SkeletonConstraintService : Singleton<SkeletonConstraintService>, ISkeletonConstraintService
    {
        public const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        #region DI

        private readonly ISkeleton skeleton;

        public SkeletonConstraintService(ISkeleton skeleton)
        {
            this.skeleton = skeleton;
        }

        public SkeletonConstraintService() : this(skeleton: PersonalSkeletonManager.Instance.PersonalSkeleton)
        {
        }

        #endregion DI

        private readonly Dictionary<uint, List<IBoneConstraint>> constraints = new();

        public IReadOnlyDictionary<uint, IReadOnlyList<IBoneConstraint>> Constraints => constraints.ToDictionary(x => x.Key,
                                                                                                                        y => (IReadOnlyList<IBoneConstraint>)y.Value);

        /// <inheritdoc/>
        public void RegisterConstraint(IBoneConstraint constraint)
        {
            if (constraint == null)
            {
                UMI3DLogger.LogWarning("Impossible to register constraint. Constraint is null.", DEBUG_SCOPE);
                return;
            }

            if (constraint.ConstrainedBone is BoneType.None)
            {
                UMI3DLogger.LogWarning("Impossible to register constraint. Constraint on none bonetype are not allowed.", DEBUG_SCOPE);
                return;
            }

            if (constraints.TryGetValue(constraint.ConstrainedBone, out List<IBoneConstraint> boneConstraints))
            {
                if (boneConstraints.Contains(constraint))
                    return;

                bool canApply = boneConstraints.All(x => !x.IsApplied);
                boneConstraints.Add(constraint);

                if (canApply && constraint.ShouldBeApplied)
                    constraint.Apply(skeleton);

                return;
            }

            // no constraint for this bone already
            constraints.Add(constraint.ConstrainedBone, new List<IBoneConstraint> { constraint });

            if (constraint.ShouldBeApplied)
                constraint.Apply(skeleton);
        }

        /// <inheritdoc/>
        public void UpdateConstraints()
        {
            foreach (uint bone in constraints.Keys)
            {
                if (constraints[bone].Count == 0)
                    continue;

                UpdateConstraints(bone);
            }
        }

        /// <inheritdoc/>
        public void UpdateConstraints(uint boneType)
        {
            if (!constraints.ContainsKey(boneType))
                return;

            List<IBoneConstraint> boneConstraints = constraints[boneType];

            IBoneConstraint constraintToApply = boneConstraints.FirstOrDefault(x => x.ShouldBeApplied && !x.IsApplied);

            foreach (IBoneConstraint constraintToEndApply in boneConstraints.Where(x => x != constraintToApply && x.IsApplied))
            {
                constraintToEndApply.EndApply(skeleton);
            }

            constraintToApply?.Apply(skeleton);
        }

        /// <inheritdoc/>
        public void UnregisterConstraint(IBoneConstraint constraint)
        {
            if (constraint == null)
                return;

            if (!constraints.TryGetValue(constraint.ConstrainedBone, out List<IBoneConstraint> boneConstraints))
                return;

            if (!boneConstraints.Contains(constraint))
                return;

            boneConstraints.Remove(constraint);

            if (!constraint.IsApplied)
                return;

            constraint.EndApply(skeleton);

            IBoneConstraint constraintToApply = boneConstraints.FirstOrDefault(x => x.ShouldBeApplied && !x.IsApplied);
            constraintToApply?.Apply(skeleton);
        }

        /// <inheritdoc/>
        public void ForceActivateConstraint(IBoneConstraint constraint)
        {
            constraint.ShouldBeApplied = true;
            UpdateConstraints(constraint.ConstrainedBone);
        }

        /// <inheritdoc/>
        public void ForceDeactivateConstraint(IBoneConstraint constraint)
        {
            constraint.ShouldBeApplied = false;
            UpdateConstraints(constraint.ConstrainedBone);
        }
    }
}