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

using System.Linq;
using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    public class AnimatedSkeleton : ISubSkeleton
    {
        private const DebugScope debugScope = DebugScope.CDK | DebugScope.Animation | DebugScope.UserCapture;

        /// <summary>
        /// Reference to the skeleton mapper that computes related links into a pose.
        /// </summary>
        public SkeletonMapper Mapper { get; protected set; }

        private UMI3DEnvironmentLoader environmentLoader;

        public AnimatedSkeleton() { }

        public AnimatedSkeleton(SkeletonMapper mapper)
        {
            Mapper = mapper;
            environmentLoader = UMI3DEnvironmentLoader.Instance;
        }

        public AnimatedSkeleton(SkeletonMapper mapper, UMI3DEnvironmentLoader environmentLoader)
        {
            Mapper = mapper;
            this.environmentLoader = environmentLoader;
        }

        ///<inheritdoc/>
        /// Always returns null for AnimatonSkeleton.
        public virtual UserCameraPropertiesDto GetCameraDto()
        {
            return null; //! to implement only in TrackedAvatar
        }

        /// <summary>
        /// Get the skeleton pose based on the position of this AnimationSkeleton.
        /// </summary>
        /// <returns></returns>
        public virtual PoseDto GetPose()
        {
            if (!Mapper.animations
                .Select(id => UMI3DEnvironmentLoader.Instance.GetEntityObject<UMI3DAnimatorAnimation>(id))
                .Any(a => a?.IsPlaying() ?? false))
                return null;
            return Mapper.GetPose();
        }

        /// <summary>
        /// Activate / Deactivate animations accordingly to the <paramref name="trackingFrame"/>.
        /// </summary>
        /// <param name="trackingFrame"></param>
        public virtual void Update(UserTrackingFrameDto trackingFrame)
        {
            var animations = from animId in Mapper.animations 
                             select (id: animId, UMI3DAnimation: UMI3DEnvironmentLoader.Instance.GetEntityObject<UMI3DAnimatorAnimation>(animId));

            foreach (var anim in animations)
            {
                if (anim.UMI3DAnimation == null) // animation could not be found
                {
                    UMI3DLogger.LogWarning($"Trying to play/stop a unreferenced animation. ID : {anim.id}", debugScope);
                    continue;
                }
                if (trackingFrame.animationsPlaying.Any(animId => animId == anim.id))
                {
                    if (!anim.UMI3DAnimation.IsPlaying())
                        anim.UMI3DAnimation.Start();
                }
                else
                {
                    if (anim.UMI3DAnimation.IsPlaying())
                        anim.UMI3DAnimation.Stop();
                }
            }
        }

        /// <summary>
        /// Fill out <paramref name="trackingFrame"/> with currently playing animations.
        /// </summary>
        /// <param name="trackingFrame"></param>
        /// <param name="option"></param>
        public virtual void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            var activeAnimations = from animId in Mapper.animations
                                   select (id: animId, animation: UMI3DEnvironmentLoader.Instance.GetEntityObject<UMI3DAnimatorAnimation>(animId))
                                   into anim
                                   where anim.animation.IsPlaying()
                                   select anim.id;

            trackingFrame.animationsPlaying = activeAnimations.ToArray();
        }
    }
}