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

using umi3d.common;

namespace umi3d.cdk
{
    public class UMI3DAnimationManager : inetum.unityUtils.SingleBehaviour<UMI3DAnimationManager>
    {
        public static UMI3DAbstractAnimation Get(ulong id) { return UMI3DEnvironmentLoader.GetEntity(id)?.Object as UMI3DAbstractAnimation; }

        public static void Start(ulong id)
        {
            Get(id)?.Start();
        }

        public static void Stop(ulong id)
        {
            Get(id)?.Stop();
        }

    }
}