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
using umi3d.cdk.userCapture.animation;
using umi3d.common;

namespace umi3d.cdk.collaboration.userCapture.animation
{
    /// <summary>
    /// Loader for <see cref="UMI3DSkeletonNodeDto"/> in a collaborative context.
    /// </summary>
    public class UMI3DCollaborativeSkeletonNodeLoader : UMI3DSkeletonNodeLoader
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Collaboration;

        private bool isRegisteredForPersonalSkeletonCleanup;

        #region Dependency Injection

        private readonly ICollaborativeSkeletonsManager collaborativeSkeletonsmanager;
        private readonly UMI3DCollaborationClientServer collaborationClientServer;

        public UMI3DCollaborativeSkeletonNodeLoader()
        {
            this.collaborativeSkeletonsmanager = CollaborativeSkeletonManager.Instance;
            this.collaborationClientServer = UMI3DCollaborationClientServer.Instance;
        }

        public UMI3DCollaborativeSkeletonNodeLoader(ICollaborativeSkeletonsManager collaborativeSkeletonsmanager,
                                                    UMI3DCollaborationClientServer collaborationClientServer)
        {
            this.collaborativeSkeletonsmanager = collaborativeSkeletonsmanager;
            this.collaborationClientServer = collaborationClientServer;
        }

        #endregion Dependency Injection

        protected override void AttachToSkeleton(ulong userId, AnimatedSkeleton subskeleton)
        {
            var skeleton = collaborativeSkeletonsmanager.GetSkeletonById(userId);
            if (skeleton is not null)
            {
                // add animated skeleton to subskeleton list and re-order it by descending priority
                var animatedSkeletons = skeleton.Skeletons
                                        .Where(x => x is AnimatedSkeleton)
                                        .Cast<AnimatedSkeleton>()
                                        .Append(subskeleton)
                                        .OrderByDescending(x => x.Priority).ToList();

                skeleton.Skeletons.RemoveAll(x => x is AnimatedSkeleton);
                skeleton.Skeletons.AddRange(animatedSkeletons);

                // if it is the browser, register that it is required to delete animated skeleton on leaving
                if (userId == collaborationClientServer.GetUserId() && !isRegisteredForPersonalSkeletonCleanup)
                {
                    isRegisteredForPersonalSkeletonCleanup = true;

                    void RemoveSkeletons()
                    {
                        skeleton.Skeletons.RemoveAll(x => x is AnimatedSkeleton);
                        collaborationClientServer.OnLeavingEnvironment.RemoveListener(RemoveSkeletons);
                        isRegisteredForPersonalSkeletonCleanup = false;
                    }

                    collaborationClientServer.OnLeavingEnvironment.AddListener(RemoveSkeletons);
                }

                // if some animator parameters should be updated by the browsers itself, start listening to them
                if (subskeleton.SelfUpdatedAnimatorParameters.Length > 0)
                    subskeleton.StartParameterSelfUpdate(skeleton);
            }
            else
                UMI3DLogger.LogWarning($"Skeleton of user {userId} not found. Cannot attach skeleton node.", DEBUG_SCOPE);
        }
    }
}