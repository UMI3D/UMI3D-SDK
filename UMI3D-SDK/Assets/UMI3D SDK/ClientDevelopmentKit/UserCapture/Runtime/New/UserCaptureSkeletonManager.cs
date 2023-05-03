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

namespace umi3d.cdk.userCapture
{
    public class UserCaptureSkeletonManager : Singleton<UserCaptureSkeletonManager>, ISkeletonManager 
    {
        public PersonalSkeleton skeleton
        {
            get
            {
                if (_skeleton == null)
                {
                    _skeleton = loadingServiceAnchor.GetComponentInChildren<PersonalSkeleton>();
                    return _skeleton;
                }
                else
                    return _skeleton;
            }
            protected set => _skeleton = value;
        }
        private PersonalSkeleton _skeleton;

        #region Dependency Injection
        private readonly UMI3DLoadingHandler loadingServiceAnchor;

        public UserCaptureSkeletonManager()
        {
            Init();
            loadingServiceAnchor = UMI3DLoadingHandler.Instance;
        }
        #endregion Dependency Injection

        protected virtual void Init()
        {
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() =>
            {
                skeleton = loadingServiceAnchor.GetComponentInChildren<PersonalSkeleton>();
            });
        }
    }
}
