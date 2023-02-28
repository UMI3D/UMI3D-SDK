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
using System.Collections;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Handler to define the position in the scene and in the scene graph to load objects.
    /// </summary>
    /// This handler is a place to nstantiate all the UMI3D services.
    public class UMI3DLoadingHandler : SingleBehaviour<UMI3DLoadingHandler>
    {
        #region Loading
        /// <summary>
        /// Main loader reference.
        /// </summary>
        /// This handler create a new loader if no one is available.
        protected UMI3DEnvironmentLoader environmentLoaderService;

        [Header("Loading")]
        [SerializeField]
        protected AbstractUMI3DLoadingParameters parameters;

        [SerializeField, Tooltip("Material to use when material is unavailable or inexistent.")]
        protected Material baseMaterial;
        #endregion Loading

        /// <summary>
        /// Call this method to attach a coroutine to the handler and start it.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <returns></returns>
        public Coroutine AttachCoroutine(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }

        /// <summary>
        /// Call this method to remove a coroutine from the handler and stop it.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <returns></returns>
        public void DettachCoroutine(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        protected override void Awake()
        {
            base.Awake();

            // LOADING SERVICE
            environmentLoaderService = UMI3DEnvironmentLoader.Instance;
            environmentLoaderService.SetParameters(parameters);
            environmentLoaderService.SetBaseMaterial(baseMaterial);
        }
    }
}