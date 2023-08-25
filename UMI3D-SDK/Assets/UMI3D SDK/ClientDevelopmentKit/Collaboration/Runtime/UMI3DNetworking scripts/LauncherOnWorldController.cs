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
using umi3d.common;
using inetum.unityUtils;
using umi3d.debug;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    internal static class LauncherOnWorldController
    {
        #region private
        static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(LauncherOnWorldController)}");


        #endregion

        /// <summary>
        /// Send a request to get a <see cref="MediaDto"/>.
        /// </summary>
        /// <param name="RawURL">A simplified version of the url where a media dto can be requested.</param>
        /// <param name="requestSucceeded">Action raised when a media dto has been found.</param>
        /// <param name="requestFailed">Action raised when the request failed.</param>
        /// <param name="shouldCleanAbort">Whether or not the request has been interrupted.</param>
        /// <param name="tryCount">The number of try.</param>
        /// <param name="maxTryCount">The maximum number of try before giving up.</param>
        /// <param name="report">A log report.</param>
        /// <returns></returns>
        public static IEnumerator RequestMediaDto(
                string RawURL,
                Action<MediaDto> requestSucceeded, Action<int> requestFailed, Func<bool> shouldCleanAbort,
                int tryCount = 0, int maxTryCount = 3, UMI3DLogReport report = null
        )
        {
            if (shouldCleanAbort?.Invoke() ?? false)
            {
                logger.Debug($"{nameof(RequestMediaDto)}", $"Caller requests to abort the connection with MediaDto in a clean way.", report: report);
                yield break;
            }

            string curentUrl = RawURL;
            if (!curentUrl.EndsWith(UMI3DNetworkingKeys.media))
            {
                curentUrl += UMI3DNetworkingKeys.media;
            }
            if (!curentUrl.StartsWith("http://") && !curentUrl.StartsWith("https://"))
            {
                curentUrl = "http://" + curentUrl;
            }

            var tabReporter = logger.GetReporter("RequestMediaDTOTab");
            var assertReporter = logger.GetReporter("RequestMediaDTOAssert");

            IEnumerator nextTryEnumerator = null;

            yield return UMI3DNetworking.Get_WR(
                credentials: (null, null),
                curentUrl,
                shouldCleanAbort,
                onCompleteSuccess: op =>
                {
                    var uwr = op.webRequest;

                    if (uwr?.downloadHandler.data == null)
                    {
                        logger.DebugAssertion($"{nameof(RequestMediaDto)}", $"downloadHandler.data == null.");
                        tabReporter.Report();
                        assertReporter.Report();
                        return;
                    }

                    string json = null;
                    MediaDto mediaDto = null;
                    try
                    {
                        json = System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data);
                        mediaDto = UMI3DDtoSerializer.FromJson<MediaDto>(json, Newtonsoft.Json.TypeNameHandling.None);
                    }
                    catch (Exception e)
                    {
                        WorldControllerException.LogException(
                            $"Trying to get media dto from json cause an exception.",
                            inner: e,
                            WorldControllerException.ExceptionTypeEnum.MediaDtoFromJson
                        );
                        tabReporter.Report();
                        assertReporter.Report();
                        return;
                    }

                    requestSucceeded?.Invoke(mediaDto);
                    logger.Default($"{nameof(RequestMediaDto)}", $"Request at: {RawURL} is a success.");
                    tabReporter.Clear();
                    assertReporter.Clear();
                },
                onCompleteFail: op =>
                {
                    if (shouldCleanAbort?.Invoke() ?? false)
                    {
                        logger.Debug($"{nameof(RequestMediaDto)}", $"Caller requests to abort the connection with MediaDto in a clean way.", report: report);
                        return;
                    }

                    logger.Assertion(
                        tag: $"{nameof(RequestMediaDto)}",
                        $"MediaDto failed:   " +
                        $"{op.webRequest.result}".FormatString(19) +
                        "   " +
                        $"{curentUrl}".FormatString(40) +
                        "   " +
                        $"{tryCount}" +
                        $"\n{op.webRequest.error}",
                        report: assertReporter
                        );

                    if (tryCount < maxTryCount - 1)
                    {
                        nextTryEnumerator = RequestMediaDto(RawURL, requestSucceeded, requestFailed, shouldCleanAbort, tryCount + 1, maxTryCount, report);
                    }
                    else
                    {
                        logger.Error($"{nameof(RequestMediaDto)}", $"MediaDto failed more than 3 times. Connection has been aborted.");
                        tabReporter.Report();
                        assertReporter.Report();
                    }

                    requestFailed?.Invoke(tryCount);
                },
                tabReporter
            );

            if (nextTryEnumerator != null)
            {
                yield return nextTryEnumerator;
            }
        }


        /// <summary>
        /// An exception class to deal with <see cref="LauncherOnWorldController"/> issues.
        /// </summary>
        [Serializable]
        public class WorldControllerException : Exception
        {
            static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(WorldControllerException)}");

            public enum ExceptionTypeEnum
            {
                Unknown,
                MediaDtoFromJson
            }

            public ExceptionTypeEnum exceptionType;

            public WorldControllerException(string message, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}")
            {
                this.exceptionType = exceptionType;
            }
            public WorldControllerException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}", inner)
            {
                this.exceptionType = exceptionType;
            }

            public static void LogException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown)
            {
                logger.Exception(null, new WorldControllerException(message, inner, exceptionType));
            }
        }
    }
}
