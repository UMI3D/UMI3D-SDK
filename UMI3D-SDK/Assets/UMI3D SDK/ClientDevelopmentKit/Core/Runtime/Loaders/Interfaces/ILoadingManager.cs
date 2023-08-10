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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk
{
    public interface ILoadingManager
    {
        Material baseMaterial { get; }
        GlTFNodeLoader nodeLoader { get; }
        UMI3DSceneLoader sceneLoader { get; }
        IUMI3DAbstractLoadingParameters AbstractLoadingParameters { get; }
        IUMI3DLoadingParameters LoadingParameters { get; }

        public UnityEvent onResourcesLoaded { get; }
        public UnityEvent onEnvironmentLoaded { get; }

        Task ReadUMI3DExtension(GlTFEnvironmentDto dto, GameObject node);

        Material GetBaseMaterial();

        IEnumerator GetBaseMaterialBeforeAction(Action<Material> callback);

        Task Load(GlTFEnvironmentDto dto, MultiProgress LoadProgress);

        void WaitUntilEntityLoaded(ulong id, Action<UMI3DEntityInstance> entityLoaded, Action entityFailedToLoad = null);

        Task<UMI3DEntityInstance> WaitUntilEntityLoaded(ulong id, List<CancellationToken> tokens);
    }
}