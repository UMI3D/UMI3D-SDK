/*
Copyright 2022 Inetum

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

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.worldController
{
    public abstract class WorldControllerAPI : ScriptableObject, IWorldController_Environment
    {
        public abstract Task NotifyUserJoin(string uid);

        public abstract Task NotifyUserLeave(string uid);

        public virtual void Setup() { }

        public virtual void SetupAfterServerStart() { }

        public virtual void Stop()
        {
        }


    }
}