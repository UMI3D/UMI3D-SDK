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
using umi3d.cdk.scene.sceneModel;
using umi3d.debug;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace umi3d.cdk.scene.sceneController
{
    public class SceneLoader : MonoBehaviour
    {
        UMI3DLogger logger = new(mainTag: nameof(SceneLoader));

        [SerializeField, Tooltip("The current scene.")]
        protected SerializedAddressableT<Scene_VM> currentScene = default;
        [SerializeField, Tooltip("The next scene to load.")]
        protected SerializedAddressableT<Scene_VM> nextScene = default;
        public LoadSceneMode loadMode = LoadSceneMode.Single;

        protected virtual void Awake()
        {
            if (currentScene.IsValid)
            {
                currentScene.LoadAssetAsync();
            }
            if (nextScene.IsValid)
            {
                nextScene.LoadAssetAsync().Completed += NextSceneVMLoaded;
            }
            else
            {
                logger.Error($"{nameof(Awake)}", $"next scene is not valid.");
            }
        }

        protected virtual void NextSceneVMLoaded(AsyncOperationHandle<Scene_VM> opHandle)
        {
            opHandle.NowOrLater(sceneVM =>
            {
                if (sceneVM.scene.IsValid)
                {
                    sceneVM.scene.LoadSceneAsync(loadMode).NowOrLater(scene =>
                    {
                        sceneVM.unityScene.value = scene.Scene;
                    });
                }
            });
        }
    }
}
