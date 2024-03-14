/*
Copyright 2019 - 2024 Inetum

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

using inetum.unityUtils.saveSystem;
using System;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    [CreateAssetMenu(fileName = "UMI3D Projection Event System", menuName = "UMI3D/Interactions/Projection/Projection Event System")]
    public class ProjectionEventSystem : SerializableScriptableObject
    {
        public event Action<AbstractControlEntity> interactionProjected;
        public event Action<AbstractTool> toolProjected;
        public event Action<AbstractTool> toolReleased;

        internal void OnProjected(AbstractControlEntity controlEntity)
        {
            interactionProjected?.Invoke(controlEntity);
        }

        internal void OnProjected(AbstractTool tool)
        {
            toolProjected?.Invoke(tool);
        }

        internal void OnReleased(AbstractTool tool)
        {
            toolReleased?.Invoke(tool);
        }
    }
}