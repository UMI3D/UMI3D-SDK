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
using inetum.unityUtils.saveSystem;
using UnityEngine;

namespace umi3d.cdk.scene.sceneModel
{
    /// <summary>
    /// View model of a scene.
    /// </summary>
    [CreateAssetMenu(fileName = "New Scene", menuName = "UMI3D/Scene Data/Scene")]
    public class Scene_VM : SerializableScriptableObject
    {
#if UNITY_EDITOR
        [TextArea, Tooltip("Add a description to explain the purpose of this scene.")]
        public string description;
#endif

        public SerializedAddressableScene scene;
        public SceneTypeEnum type;
        [HideInInspector]
        public NotifyingVariable<UnityEngine.SceneManagement.Scene?> unityScene = new();

        public bool IsLoaded
        {
            get
            {
                return unityScene.value.HasValue 
                    ? unityScene.value.Value.isLoaded
                    : false;
            }
        }
    }
}
