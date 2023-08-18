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
using System.Collections;
using System.Collections.Generic;
using umi3d.debug;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// The manager of the scene in a UMI3D browser.
    /// </summary>
    internal static class UMI3DSceneManager
    {
        #region public

        /// <summary>
        /// Load a scene asyncronously.
        /// 
        /// <para>
        /// The method doesn't let you load a new instance of the scene if it is already active or is already in the process of beeing active.
        /// </para>
        /// </summary>
        /// <param name="sceneToLoad"></param>
        /// <param name="shouldStopLoading"></param>
        /// <param name="loadingProgress"></param>
        /// <param name="loadingSucced"></param>
        /// <param name="loadingFail"></param>
        /// <param name="loadMode"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public static Coroutine LoadSceneAsync(
            string sceneToLoad,
            Func<bool> shouldStopLoading, Action<float> loadingProgress,
            Action loadingSucced, Action loadingFail,
            UnityEngine.SceneManagement.LoadSceneMode loadMode = UnityEngine.SceneManagement.LoadSceneMode.Additive,
            UMI3DLogReport report = null
        )
        {
            if (IsOrWillBeLoaded(sceneToLoad))
            {
                logger.Assertion($"{nameof(LoadSceneAsync)}", $"Trying to load a scene that is active.");
                return null;
            }

            var result = CoroutineManager.Instance.AttachCoroutine(_LoadSceneAsync(
                sceneToLoad,
                shouldStopLoading, loadingProgress,
                loadingSucced: () =>
                {
                    loadings.Remove(sceneToLoad);
                    loadingSucced.Invoke();
                },
                loadingFail: () =>
                {
                    loadings.Remove(sceneToLoad);
                    loadingFail.Invoke();
                },
                loadMode,
                report
            ));

            loadings.Add(sceneToLoad, result);

            return result;
        }

        /// <summary>
        /// Unload a scene asyncronously.
        /// 
        /// <para>
        /// The method doesn't let you unload a scene if it is already unloaded or is already in the process of beeing unloaded.
        /// </para>
        /// </summary>
        /// <param name="sceneToUnload"></param>
        /// <param name="unloadingProgress"></param>
        /// <param name="unloadingSucced"></param>
        /// <param name="unloadMode"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public static Coroutine UnloadSceneAsync(
            string sceneToUnload,
            Action<float> unloadingProgress, Action unloadingSucced,
            UnityEngine.SceneManagement.UnloadSceneOptions unloadMode = UnityEngine.SceneManagement.UnloadSceneOptions.UnloadAllEmbeddedSceneObjects,
            UMI3DLogReport report = null
        )
        {
            if (IsOrWillBeUnloaded(sceneToUnload))
            {
                logger.Assertion($"{nameof(LoadSceneAsync)}", $"Trying to unload a scene that is not active.");
                return null;
            }

            var result = CoroutineManager.Instance.AttachCoroutine(_UnloadSceneAsync(
                sceneToUnload,
                unloadingProgress,
                unloadingSucced: () =>
                {
                    unloadings.Remove(sceneToUnload);
                    unloadingSucced.Invoke();
                },
                unloadMode,
                report
            ));

            unloadings.Add(sceneToUnload, result);

            return result;
        }

        #endregion


        #region Private

        static UMI3DLogger logger = new UMI3DLogger(mainTag: $"{nameof(UMI3DSceneManager)}");

        static Dictionary<string, Coroutine> loadings = new Dictionary<string, Coroutine>();
        static Dictionary<string, Coroutine> unloadings = new Dictionary<string, Coroutine>();

        /// <summary>
        /// Whether or not <paramref name="sceneName"/> is currently active or will be active after loading.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        static bool IsOrWillBeLoaded(string sceneName)
        {
            if (loadings.ContainsKey(sceneName))
            {
                return true;
            }

            var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Whether or not <paramref name="sceneName"/> is currently unactive or will be unactive after unloading.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        static bool IsOrWillBeUnloaded(string sceneName)
        {
            if (unloadings.ContainsKey(sceneName))
            {
                return true;
            }

            var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return false;
                }
            }

            return true;
        }

        static IEnumerator _LoadSceneAsync(
            string toScene,
            Func<bool> shouldStopLoading, Action<float> loadingProgress,
            Action loadingSucced, Action loadingFail,
            UnityEngine.SceneManagement.LoadSceneMode loadMode,
            UMI3DLogReport report
        )
        {
            logger.Debug($"{nameof(_LoadSceneAsync)}", $"Start to load [{toScene}] async.", report: report);
            var loadAsync = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(toScene, loadMode);
            while (!loadAsync.isDone)
            {
                if (shouldStopLoading?.Invoke() ?? false)
                {
                    logger.Debug($"{nameof(_LoadSceneAsync)}", $"Stop loading before scene finished to load.", report: report);
                    loadingFail?.Invoke();

                    yield break;
                }

                loadingProgress?.Invoke(loadAsync.progress);
                yield return null;
            }

            logger.Debug($"{nameof(_LoadSceneAsync)}", $"[{toScene}] has finished to load.", report: report);
            loadingSucced?.Invoke();
        }

        static IEnumerator _UnloadSceneAsync(
            string sceneToUnload,
            Action<float> unloadingProgress, Action unloadingSucced,
            UnityEngine.SceneManagement.UnloadSceneOptions unloadMode,
            UMI3DLogReport report
        )
        {
            var unloadAsync = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneToUnload, unloadMode);

            while (!unloadAsync.isDone)
            {
                yield return null;
            }

            logger.Debug($"{nameof(_UnloadSceneAsync)}", $"[{sceneToUnload}] has finished to unload.", report: report);
            unloadingSucced?.Invoke();
        }

        #endregion
    }
}
