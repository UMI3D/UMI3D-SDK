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
using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    public class PersonalSkeletonManager : Singleton<PersonalSkeletonManager>, ISkeletonManager
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

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

        public UMI3DSkeletonHierarchy StandardHierarchy
        {
            get
            {
                _standardHierarchy ??= new UMI3DSkeletonHierarchy((environmentLoaderService.LoadingParameters as UMI3DUserCaptureLoadingParameters).SkeletonHierarchyDefinition);
                return _standardHierarchy;
            }
        }
        private UMI3DSkeletonHierarchy _standardHierarchy;


        private PersonalSkeleton _skeleton;

        #region Dependency Injection

        private readonly UMI3DLoadingHandler loadingServiceAnchor;
        private readonly UMI3DEnvironmentLoader environmentLoaderService;

        public PersonalSkeletonManager()
        {
            loadingServiceAnchor = UMI3DLoadingHandler.Instance;
            environmentLoaderService = UMI3DEnvironmentLoader.Instance;
            Init();
        }

        public PersonalSkeletonManager(UMI3DLoadingHandler loadingServiceAnchor, UMI3DEnvironmentLoader environmentLoaderService, ILateRoutineService lateRoutineService)
        {
            this.loadingServiceAnchor = loadingServiceAnchor;
            this.environmentLoaderService = environmentLoaderService;
            Init();
        }

        #endregion Dependency Injection

        protected virtual void Init()
        {
            environmentLoaderService.onEnvironmentLoaded.AddListener(InitPersonalSkeleton);
        }

        private void InitPersonalSkeleton()
        {
            personalSkeleton = loadingServiceAnchor.GetComponentInChildren<PersonalSkeleton>();
            personalSkeleton.SkeletonHierarchy = StandardHierarchy;
        }

            {
        }
    }
}