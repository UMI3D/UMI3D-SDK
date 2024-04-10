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

using umi3d.common.userCapture.description;

namespace umi3d.cdk.userCapture
{
    public interface ISubskeletonDescriptionInterpolationPlayer : ISubskeletonDescriptor
    {
        /// <summary>
        /// True if the player is currently applying a pose.
        /// </summary>
        /// Note that this value is still true after some time 
        /// after calling <see cref="End()"/> because of end of application handling.
        bool IsPlaying { get; }

        /// <summary>
        /// True when the player is in the ending interpolation phase.
        /// </summary>
        bool IsEnding { get; }

        /// <summary>
        /// Complete skeleton affected by the player.
        /// </summary>
        ISkeleton Skeleton { get; }

        /// <summary>
        /// Describe the state of the subskeleton.
        /// </summary>
        ISubskeletonDescriptor Descriptor { get; }

        /// <summary>
        /// Start to apply a pose on the affected skeleton.
        /// </summary>
        /// <param name="parameters">Optional parameters to control the subskeleton playing.</param>
        void Play();

        /// <summary>
        /// Start to apply a pose on the affected skeleton.
        /// </summary>
        /// <param name="parameters">Optional parameters to control the subskeleton playing.</param>
        void Play(PlayingParameters parameters = null);

        /// <summary>
        /// Start to end a pose on the affected skeleton. 
        /// </summary>
        /// <param name="shouldStopImmediate">If true, the subskeleton displacement is ended immediately with no ending interpolation phase.</param>
        void End(bool shouldStopImmediate = false);

        /// <summary>
        /// Parameters used to request a specific application of the pose.
        /// </summary>
        public class PlayingParameters
        {
            /// <summary>
            /// Duration (in seconds) of the transition when pose is starting to be applied.
            /// </summary>
            public float startTransitionDuration = DEFAULT_TRANSITION_DURATION;

            /// <summary>
            /// Duration (in seconds) of the transition when pose is ending to be applied.
            /// </summary>
            public float endTransitionDuration = DEFAULT_TRANSITION_DURATION;
        }

        public const float DEFAULT_TRANSITION_DURATION = 0.25f;
    }
}
