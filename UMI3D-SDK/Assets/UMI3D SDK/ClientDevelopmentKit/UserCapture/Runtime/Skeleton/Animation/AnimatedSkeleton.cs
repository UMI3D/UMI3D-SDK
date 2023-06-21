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

using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    public class AnimatedSkeleton : ISubSkeleton
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Animation | DebugScope.UserCapture;

        /// <summary>
        /// Reference to the skeleton mapper that computes related links into a pose.
        /// </summary>
        public virtual SkeletonMapper Mapper { get; protected set; }

        /// <summary>
        /// Priority level of the animated skeleton.
        /// </summary>
        public virtual uint Priority { get; protected set; }

        /// <summary>
        /// Animation id of the animated skeleton.
        /// </summary>
        public virtual UMI3DAbstractAnimation[] Animations { get; protected set; }
        public virtual UMI3DAnimatorAnimation[] Animations { get; protected set; }

        public AnimatedSkeleton(SkeletonMapper mapper, UMI3DAbstractAnimation[] animations, uint priority = 0)
        {
            Mapper = mapper;
            Priority = priority;
            Animations = animations;
        }

        ///<inheritdoc/>
        /// Always returns null for AnimatonSkeleton.
        public virtual UserCameraPropertiesDto GetCameraDto()
        {
            return null;
        }

        /// <summary>
        /// Get the skeleton pose based on the position of this AnimationSkeleton.
        /// </summary>
        /// <returns></returns>
        public virtual PoseDto GetPose()
        {
            foreach (var anim in Animations)
            {
                if (anim?.IsPlaying() ?? false)
                {
                    return Mapper.GetPose();
                }
            }
            return null;
        }
    }
}