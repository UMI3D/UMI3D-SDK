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
using System.Linq;
using umi3d.debug;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace umi3d.cdk.scene
{
    public class UMI3DSceneLoader : MonoBehaviour
    {
        [Serializable]
        public struct SceneT
        {
            public string name;
            [Tooltip("Priority for loading the scenes. Higher priorities will be loaded first")]
            public int priority;
        }

        UMI3DLogger logger = new();

        [Tooltip("The next scenes to load.")]
        public SceneT[] nextScenes = default;
        /// <summary>
        /// If <see cref="loadMode"/> is: <br/>
        /// <list type="bullet">
        /// <item>
        /// <see cref="LoadSceneMode.Single"/> the current scene will be unloaded. 
        /// If there are several next scenes those scenes will be loaded additively.<br/>
        /// </item>
        /// 
        /// <item>
        /// <see cref="LoadSceneMode.Additive"/> the current scene will not be unloaded.
        /// </item>
        /// </list>
        /// </summary>
        [Tooltip("The mode of loading.")]
        public LoadSceneMode loadMode = LoadSceneMode.Single;

        protected virtual void Awake()
        {
            logger.MainContext = this;
            logger.MainTag = nameof(UMI3DSceneLoader);

            List<int> priorities = new();
            foreach (var scene in nextScenes)
            {
                if (!priorities.Contains(scene.priority))
                {
                    priorities.Add(scene.priority);
                }
            }
            priorities.Sort();
            priorities.Reverse();

            StartCoroutine(LoadAllScenes(priorities));
        }

        IEnumerator LoadAllScenes(List<int> priorities)
        {
            foreach (var priority in priorities)
            {
                var scenes =
                    from scene in nextScenes
                    where scene.priority == priority
                    select scene;
                var asyncLoads = LoadScenes(scenes.ToArray());
                while (asyncLoads.Count > 0)
                {
                    if (!asyncLoads[asyncLoads.Count - 1].isDone)
                    {
                        yield return null;
                    }
                    else
                    {
                        asyncLoads.RemoveAt(asyncLoads.Count - 1);
                    }
                }
            }

            if (loadMode == LoadSceneMode.Single)
            {
                SceneManager.UnloadSceneAsync(gameObject.scene.name);
            }

            yield break;
        }

        List<AsyncOperation> LoadScenes(params SceneT[] scenes)
        {
            List<AsyncOperation> asyncLoads = new();
            foreach (var scene in scenes)
            {
                AsyncOperation asyncLoad = default;
                try
                {
                    asyncLoad = SceneManager.LoadSceneAsync(
                        scene.name,
                        LoadSceneMode.Additive
                    );
                    if (asyncLoad == null)
                    {
                        throw new Exception($"[UMI3D]: Scene exception");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(nameof(LoadScenes), $"Error while loading scene: {scene.name} from {gameObject.scene.name}");
                    throw e;
                }

                asyncLoads.Add(asyncLoad);
            }

            return asyncLoads;
        }
    }
}
