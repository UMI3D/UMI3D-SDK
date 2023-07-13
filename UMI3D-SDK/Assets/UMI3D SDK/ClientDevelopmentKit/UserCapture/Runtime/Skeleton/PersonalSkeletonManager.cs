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
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// User's skeleton manager.
    /// </summary>
    public class PersonalSkeletonManager : Singleton<PersonalSkeletonManager>, ISkeletonManager
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        /// <inheritdoc/>
        public PersonalSkeleton personalSkeleton
        {
            get
            {
                if (_skeleton == null)
                {
                    InitPersonalSkeleton();
                    return _skeleton;
                }
                else
                    return _skeleton;
            }
            protected set => _skeleton = value;
        }

        /// <inheritdoc/>
        public UMI3DSkeletonHierarchy StandardHierarchy
        {
            get
            {
                _standardHierarchy ??= new UMI3DSkeletonHierarchy((environmentLoaderService.LoadingParameters as UMI3DUserCaptureLoadingParameters).SkeletonHierarchyDefinition);
                return _standardHierarchy;
            }
        }

        public IDictionary<uint, float> BonesAsyncFPS => personalSkeleton.BonesAsyncFPS;

        public Vector3 worldSize => personalSkeleton.worldSize;

        private UMI3DSkeletonHierarchy _standardHierarchy;

        private PersonalSkeleton _skeleton;

        #region Dependency Injection

        private readonly IEnvironmentManager environmentManager;
        private readonly ILoadingManager environmentLoaderService;
        private readonly ILateRoutineService lateRoutineService;

        public PersonalSkeletonManager()
        {
            environmentManager = UMI3DEnvironmentLoader.Instance;
            environmentLoaderService = UMI3DEnvironmentLoader.Instance;
            lateRoutineService = CoroutineManager.Instance;
            Init();
        }

        public PersonalSkeletonManager(IEnvironmentManager environmentManager, ILoadingManager environmentLoaderService, ILateRoutineService lateRoutineService)
        {
            this.environmentManager = environmentManager;
            this.environmentLoaderService = environmentLoaderService;
            this.lateRoutineService = lateRoutineService;
            Init();
        }

        #endregion Dependency Injection

        protected virtual void Init()
        {
            environmentLoaderService.onEnvironmentLoaded.AddListener(InitPersonalSkeleton);
        }

        private void InitPersonalSkeleton()
        {
            personalSkeleton = environmentManager.gameObject.GetComponentInChildren<PersonalSkeleton>();
            personalSkeleton.SkeletonHierarchy = StandardHierarchy;
            lateRoutineService.AttachLateRoutine(ComputeCoroutine());
        }

        private IEnumerator ComputeCoroutine()
        {
            while (personalSkeleton != null)
            {
                personalSkeleton.Compute();
                yield return null;
            }
        }
    }
}