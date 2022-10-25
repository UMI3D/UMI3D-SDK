﻿/*
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
using System.Linq;
using System.Threading.Tasks;
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

        public static bool HasVideoToLoad => videoPlayersToLoad.Count > 0 || UMI3DEnvironmentLoader.Entities().Select(e => e?.Object as UMI3DVideoPlayer).Any(v => (!v?.isPrepared) ?? false);

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
        public static async Task LoadVideoPlayers()
        {
            if (videoPlayersToLoad.Count > 0)
                await LoadVideoPlayersCoroutine();
            while (HasVideoToLoad)
            {
                await UMI3DAsyncManager.Yield();
            }
            await UMI3DAsyncManager.Delay(100);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onFinish"></param>
        /// <returns></returns>
        private static async Task LoadVideoPlayersCoroutine()
        {
            var wait = new WaitForSeconds(.1f);

            while (videoPlayersToLoad.Count > 0)
            {
                await UMI3DAsyncManager.Delay(100);

                UMI3DVideoPlayerDto videoPlayer = videoPlayersToLoad.Dequeue();

                var player = new UMI3DVideoPlayer(videoPlayer);

                while (!player.isPrepared)
                    await UMI3DAsyncManager.Yield();
            }
        }
    }
}