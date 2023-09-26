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

using System.Collections.Generic;
using umi3d.common.userCapture.description;

namespace umi3d.cdk.userCapture.animation
{
    /// <summary>
    /// Subskeleton that is the target if a skeleton animation, using an Animator.
    /// </summary>
    public interface IAnimatedSubskeleton : ISubskeleton
    {
        /// <summary>
        /// Animation id of the animated skeleton.
        /// </summary>
        IReadOnlyList<UMI3DAnimatorAnimation> Animations { get; }

        /// <summary>
        /// Reference to the skeleton mapper that computes related links into a pose.
        /// </summary>
        ISkeletonMapper Mapper { get; }

        /// <summary>
        /// Parameters required by the animator that are updated by the browser itself.
        /// </summary>
        /// The key correspond to a key in <see cref="SkeletonAnimatorParameterKeys"/>.
        IReadOnlyList<AnimatedSubskeleton.SkeletonAnimationParameter> SelfUpdatedAnimatorParameters { get; }

        /// <summary>
        /// Start animation parameters self update. Parameters are recomputed each frame based on the <paramref name="skeleton"/> movement.
        /// </summary>
        void StartParameterSelfUpdate(ISkeleton skeleton);

        /// <summary>
        /// Stop animation parameters self update.
        /// </summary>
        void StopParameterSelfUpdate();
    }
}