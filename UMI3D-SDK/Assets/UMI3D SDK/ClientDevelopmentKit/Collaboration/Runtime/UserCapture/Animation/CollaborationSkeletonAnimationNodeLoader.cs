﻿/*
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
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.animation;
using umi3d.common;

namespace umi3d.cdk.collaboration.userCapture.animation
{
    /// <summary>
    /// Loader for <see cref="UMI3DSkeletonNodeDto"/> in a collaborative context.
    /// </summary>
    public class CollaborationSkeletonAnimationNodeLoader : SkeletonAnimationNodeLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Collaboration;

        #region Dependency Injection

        protected readonly ICollaborativeSkeletonsManager collaborativeSkeletonsmanager;

        public CollaborationSkeletonAnimationNodeLoader() : base()
        {
            this.collaborativeSkeletonsmanager = CollaborativeSkeletonManager.Instance;
        }

        public CollaborationSkeletonAnimationNodeLoader(IEnvironmentManager environmentManager,
                                                        ILoadingManager loadingManager,
                                                        IUMI3DResourcesManager resourcesManager,
                                                        ICoroutineService coroutineManager,
                                                        ISkeletonManager personnalSkeletonService,
                                                        ICollaborativeSkeletonsManager collaborativeSkeletonsmanager,
                                                        IUMI3DClientServer clientServer)
            : base(environmentManager, loadingManager, resourcesManager, coroutineManager, personnalSkeletonService, clientServer)
        {
            this.collaborativeSkeletonsmanager = collaborativeSkeletonsmanager;
        }

        #endregion Dependency Injection

        protected override void AttachToSkeleton(ulong userId, AnimatedSubskeleton subskeleton)
        {
            // personnal skeleton is targeted
            if (clientServer.GetUserId() == userId)
            {
                base.AttachToSkeleton(userId, subskeleton);
                return;
            }

            var skeleton = collaborativeSkeletonsmanager.TryGetSkeletonById(userId);
            if (skeleton != null)
            {
                // add animated skeleton to subskeleton list and re-order it by descending priority
                var animatedSkeletons = skeleton.Skeletons
                                        .Where(x => x is AnimatedSubskeleton)
                                        .Cast<AnimatedSubskeleton>()
                                        .Append(subskeleton)
                                        .OrderByDescending(x => x.Priority).ToList();

                skeleton.Skeletons.RemoveAll(x => x is AnimatedSubskeleton);
                skeleton.Skeletons.AddRange(animatedSkeletons);

                // if some animator parameters should be updated by the browsers itself, start listening to them
                if (subskeleton.SelfUpdatedAnimatorParameters.Length > 0)
                    subskeleton.StartParameterSelfUpdate(skeleton);
            }
            else
                UMI3DLogger.LogWarning($"Skeleton of user {userId} not found. Cannot attach skeleton node.", DEBUG_SCOPE);
        }
    }
}