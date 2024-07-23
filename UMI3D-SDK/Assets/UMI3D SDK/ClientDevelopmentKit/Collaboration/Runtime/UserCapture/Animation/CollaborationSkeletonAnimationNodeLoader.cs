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

using inetum.unityUtils;

using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.animation;
using umi3d.common;
using umi3d.common.utils;

namespace umi3d.cdk.collaboration.userCapture.animation
{
    /// <summary>
    /// Loader for <see cref="common.userCapture.animation.SkeletonAnimationNodeDto"/> in a collaborative context.
    /// </summary>
    /// Main difference with <see cref="SkeletonAnimationNodeLoader"/> is that it uses collaborative skeletons.
    public class CollaborationSkeletonAnimationNodeLoader : SkeletonAnimationNodeLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Collaboration;

        #region Dependency Injection

        protected readonly ICollaborationSkeletonsManager collaborativeSkeletonsmanager;

        public CollaborationSkeletonAnimationNodeLoader() : base()
        {
            this.collaborativeSkeletonsmanager = CollaborationSkeletonsManager.Instance;
        }

        public CollaborationSkeletonAnimationNodeLoader(IEnvironmentManager environmentManager,
                                                        ILoadingManager loadingManager,
                                                        IUMI3DResourcesManager resourcesManager,
                                                        ICoroutineService coroutineManager,
                                                        ISkeletonManager personnalSkeletonService,
                                                        ICollaborationSkeletonsManager collaborativeSkeletonsmanager,
                                                        IUMI3DClientServer clientServer)
            : base(environmentManager, loadingManager, resourcesManager, coroutineManager, personnalSkeletonService, clientServer)
        {
            this.collaborativeSkeletonsmanager = collaborativeSkeletonsmanager;
        }

        #endregion Dependency Injection

        protected override ISkeleton GetParentSkeleton(ulong environmentId, ulong userId)
        {
            if (clientServer.GetUserId() == userId)
                return base.GetParentSkeleton(environmentId, userId);

            return collaborativeSkeletonsmanager.TryGetSkeletonById(environmentId, userId);
        }

        protected override void AttachToSkeleton(ISkeleton parentSkeleton, AnimatedSubskeleton animatedSubskeleton)
        {
            if (parentSkeleton == personalSkeletonService.PersonalSkeleton)
            {
                base.AttachToSkeleton(parentSkeleton, animatedSubskeleton);
                return;
            }

            if (parentSkeleton != null)
                parentSkeleton.AddSubskeleton(animatedSubskeleton);
            else
                UMI3DLogger.LogWarning($"Skeleton of user {parentSkeleton.UserId} not found. Cannot attach skeleton node.", DEBUG_SCOPE);
        }

        protected override void Delete(ulong userId)
        {
            if (clientServer.GetUserId() == userId)
            {
                base.Delete(userId);
                return;
            }
        }
    }
}