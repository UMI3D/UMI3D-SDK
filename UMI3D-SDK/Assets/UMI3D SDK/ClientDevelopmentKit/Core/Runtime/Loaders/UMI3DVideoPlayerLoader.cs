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

using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// This class is reponsible for delaying the load of VideoPlayer on Android devices.
    /// </summary>
    public class UMI3DVideoPlayerLoader
    {
        /// <summary>
        /// List of video players to load.
        /// </summary>
        public static Queue<UMI3DVideoPlayerDto> videoPlayersToLoad = new Queue<UMI3DVideoPlayerDto>();

        public static bool HasVideoToLoad => videoPlayersToLoad.Count > 0;

        /// <summary>
        /// Asks to load a <see cref="UMI3DVideoPlayer"/> from a <see cref="UMI3DVideoPlayerDto"/>. 
        /// If the platform is Android and if <see cref="UMI3DEnvironmentLoader"/> is loading an environement,
        /// <see cref="LoadVideoPlayers()"/> must be call to really create the videoPlayers, otherwise they are instanciated directly.
        /// </summary>
        /// <param name="videoPlayer"></param>
        public static void LoadVideo(UMI3DVideoPlayerDto videoPlayer)
        {
#if UNITY_ANDROID
            if (!UMI3DEnvironmentLoader.Instance.isEnvironmentLoaded)
                videoPlayersToLoad.Enqueue(videoPlayer);
            else
                new UMI3DVideoPlayer(videoPlayer);
#else
            new UMI3DVideoPlayer(videoPlayer);
#endif
        }

        /// <summary>
        /// Creates, one by one, <see cref="UMI3DVideoPlayer"/> for all <see cref="UMI3DVideoPlayerDto"/> queued in <see cref="videoPlayersToLoad"/>.
        /// <param name="onFinish"></param>
        /// </summary>
        public static void LoadVideoPlayers(Action onFinish)
        {
            if (HasVideoToLoad)
                UMI3DAnimationManager.StartCoroutine(LoadVideoPlayersCoroutine(onFinish));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onFinish"></param>
        /// <returns></returns>
        private static IEnumerator LoadVideoPlayersCoroutine(Action onFinish)
        {
            var wait = new WaitForSeconds(.1f);

            yield return wait;

            while (videoPlayersToLoad.Count > 0)
            {
                UMI3DVideoPlayerDto videoPlayer = videoPlayersToLoad.Dequeue();

                var player = new UMI3DVideoPlayer(videoPlayer);

                while (!player.isPrepared)
                    yield return null;

                yield return wait;
            }

            onFinish?.Invoke();
        }
    }
}