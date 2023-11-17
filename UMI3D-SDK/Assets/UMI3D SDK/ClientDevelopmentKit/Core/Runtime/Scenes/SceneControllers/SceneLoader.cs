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
using System;
using umi3d.cdk.scene.sceneModel;
using umi3d.debug;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace umi3d.cdk.scene.sceneController
{
    public class SceneLoader : MonoBehaviour
    {
        UMI3DLogger logger = new();

        [SerializeField, Tooltip("The current scene.")]
        protected SerializedAddressableT<Scene_VM> currentScene = default;
        [SerializeField, Tooltip("The next scenes to load.")]
        protected SerializedAddressableT<Scene_VM>[] nextScenes = default;
        /// <summary>
        /// If <see cref="loadMode"/> is <see cref="LoadSceneMode.Single"/> the current scene will be unloaded. If there are several next scenes those scenes will be loaded additively.<br/>
        /// If <see cref="loadMode"/> is <see cref="LoadSceneMode.Additive"/> the current scene will not be unloaded.
        /// </summary>
        [Tooltip("The mode of loading.")]
        public LoadSceneMode loadMode = LoadSceneMode.Single;

        protected virtual void Awake()
        {
            logger.MainContext = this;
            logger.MainTag = $"{nameof(SceneLoader)}-{gameObject.scene.name}";

            if (currentScene.IsValid)
            {
                currentScene.LoadAssetAsync();
            }

            if (nextScenes != null && nextScenes.Length > 0)
            {
                AsyncOperationHandle<Scene_VM>[] nextScenesHandlers = SAUtils.LoadAllAssetAsync(nextScenes);
                AsyncOperationHandle<Scene_VM>[] allScenesHandlers = new AsyncOperationHandle<Scene_VM>[nextScenesHandlers.Length + 1];
                allScenesHandlers[0] = currentScene.operationHandler;
                Array.Copy(nextScenesHandlers, 0, allScenesHandlers, 1, nextScenesHandlers.Length);

                AOHUtils.WhenAll(SceneModelsLoaded, allScenesHandlers);
            }
            else
            {
                logger.Error($"{nameof(Awake)}", $"next scenes are not valid.");
            }
        }

        private void SceneModelsLoaded(AsyncOperationHandle<Scene_VM>[] opHandles)
        {
            AsyncOperationHandle<Scene_VM> currentOH = opHandles[0];

            AsyncOperationHandle<SceneInstance>[] scenesHandlers = new AsyncOperationHandle<SceneInstance>[opHandles.Length - 1];
            for (int i = 1; i < opHandles.Length; i++)
            {
                var nextOH = opHandles[i];
                 
                nextOH.NowOrLater(sceneVM =>
                {
                    scenesHandlers[i - 1] = sceneVM.scene.LoadSceneAsync(LoadSceneMode.Additive);
                    sceneVM.scene.operationHandler.NowOrLater(scene =>
                    {
                        sceneVM.unityScene.value = scene.Scene;
                    });
                });
            }

            AOHUtils.WhenAll(ScenesLoaded, scenesHandlers);
        }

        private void ScenesLoaded(AsyncOperationHandle<SceneInstance>[] opHandles)
        {
            if (loadMode == LoadSceneMode.Single)
            {
                SceneManager.UnloadSceneAsync(gameObject.scene);
            }
        }

        private void OnDestroy()
        {
            if (currentScene.IsValid)
            {
                currentScene.NowOrLater(sceneVM =>
                {
                    sceneVM.unityScene = null;
                    currentScene.Release();
                });
            }
            SAUtils.ReleaseAll(nextScenes);
        }
    }
}
