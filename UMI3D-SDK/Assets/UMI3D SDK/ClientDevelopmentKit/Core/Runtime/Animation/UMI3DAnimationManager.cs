/*
Copyright 2019 - 2021 Inetum
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

using System;

namespace umi3d.cdk
{
    /// <summary>
    /// Helper class to manage animated entities.
    /// </summary>
    public class UMI3DAnimationManager : inetum.unityUtils.SingleBehaviour<UMI3DAnimationManager>
    {
        /// <summary>
        /// Get an animation by UMI3D id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("Use UMI3DEnvironmentLoader.Instance.GetEntity<UMI3DAbstractAnimation>() instead.")]
        public static UMI3DAbstractAnimation Get(ulong environmentId, ulong id) { return UMI3DEnvironmentLoader.GetEntity(environmentId,id)?.Object as UMI3DAbstractAnimation; }

        /// <summary>
        /// Start playing an animation.
        /// </summary>
        /// <param name="id">Animation UMI3D id.</param>
        [Obsolete("Use Instance.StartAnimation() instead.")]
        public static void Start(ulong environmentId, ulong id)
        {
            Get(environmentId, id)?.Start();
        }

        /// <summary>
        /// Start playing an animation.
        /// </summary>
        /// <param name="id">Animation UMI3D id.</param>
        public virtual void StartAnimation(ulong environmentId, ulong id)
        {
            UMI3DEnvironmentLoader.Instance.GetEntityObject<UMI3DAbstractAnimation>(environmentId, id).Start();
        }

        /// <summary>
        /// Stop playing an animation.
        /// </summary>
        /// <param name="id">Animation UMI3D id.</param>
        [Obsolete("Use Instance.StopAnimation() instead.")]
        public static void Stop(ulong environmentId, ulong id)
        {
            Get(environmentId, id)?.Stop();
        }

        /// <summary>
        /// Stop playing an animation.
        /// </summary>
        /// <param name="id">Animation UMI3D id.</param>
        public virtual void StopAnimation(ulong environmentId, ulong id)
        {
            UMI3DEnvironmentLoader.Instance.GetEntityObject<UMI3DAbstractAnimation>(environmentId, id).Stop();
        }
    }
}