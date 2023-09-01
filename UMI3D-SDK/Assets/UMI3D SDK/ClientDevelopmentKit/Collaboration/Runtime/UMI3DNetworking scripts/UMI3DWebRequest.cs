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
using umi3d.debug;
using UnityEngine.Networking;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// A class responsible for sending and receiving web requests.
    /// </summary>
    internal class UMI3DWebRequest : IUMI3DWebRequest
    {
        const string contentTypeJson = "application/json";

        private static UMI3DLogger logger = new UMI3DLogger(mainTag: $"{nameof(UMI3DWebRequest)}");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ContentTypeJson
        {
            get
            {
                return contentTypeJson;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="url"></param>
        /// <param name="shouldCleanAbort"></param>
        /// <param name="onCompleteSuccess"></param>
        /// <param name="onCompleteFail"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public IEnumerator Get(
            (string token, List<(string, string)> headers) credentials,
            string url,
            Func<bool> shouldCleanAbort,
            Action<UnityWebRequestAsyncOperation> onCompleteSuccess,
            Action<UnityWebRequestAsyncOperation> onCompleteFail,
            UMI3DLogReport report = null
        )
        {
            if (shouldCleanAbort?.Invoke() ?? false)
            {
                logger.Debug($"{nameof(Get)}", $"Caller requests to abort the GetRequest in a clean way.", report: report);
                yield break;
            }

            logger.DebugTab(
                tabName: "Get",
                new[]
                {
                    new UMI3DLogCell(
                        "headerToken",
                        credentials.token,
                        20
                    ),
                    new UMI3DLogCell(
                        "url",
                        url,
                        40
                    )
                },
                report: report
            );

            using (var uwr = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(credentials.token))
                {
                    uwr.SetRequestHeader(name: common.UMI3DNetworkingKeys.Authorization, value: credentials.token);
                }
                if (credentials.headers != null)
                {
                    foreach ((string name, string value) item in credentials.headers)
                    {
                        uwr.SetRequestHeader(item.name, item.value);
                    }
                }

                DateTime date = DateTime.UtcNow;

                UnityWebRequestAsyncOperation operation = uwr.SendWebRequest();

                while (!operation.isDone)
                {
                    if (shouldCleanAbort?.Invoke() ?? false)
                    {
                        logger.Debug($"{nameof(Get)}", $"Caller requests to abort the GetRequest in a clean way.", report: report);
                        yield break;
                    }
                    yield return null;
                }

#if UNITY_2020_1_OR_NEWER
                if (uwr.result > UnityWebRequest.Result.Success)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    onCompleteFail?.Invoke(operation);
                }
                else
                {
                    onCompleteSuccess?.Invoke(operation);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="shouldCleanAbort"></param>
        /// <param name="onCompleteSuccess"></param>
        /// <param name="onCompleteFail"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public IEnumerator Post(
        (string token, List<(string, string)> headers) credentials,
        string url,
        (string contentType, string json) data,
        Func<bool> shouldCleanAbort,
        Action<UnityWebRequestAsyncOperation> onCompleteSuccess,
        Action<UnityWebRequestAsyncOperation> onCompleteFail,
        UMI3DLogReport report = null
        )
        {
            if (shouldCleanAbort?.Invoke() ?? false)
            {
                logger.Debug($"{nameof(Post)}", $"Caller requests to abort the PostRequest in a clean way.", report: report);
                yield break;
            }

            logger.DebugTab(
                tabName: "Post",
                new[]
                {
                    new UMI3DLogCell(
                        "headerToken",
                        credentials.token,
                        20
                    ),
                    new UMI3DLogCell(
                        "url",
                        url,
                        40
                    ),
                    new UMI3DLogCell(
                        "contentType",
                        data.contentType,
                        20
                    )
                },
                report: report
            );

            using (var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bytes = null;

                try
                {
                    bytes = System.Text.Encoding.UTF8.GetBytes(data.json);
                }
                catch (Exception e)
                {
                    WebRequestException.LogException(
                        "Trying to get bytes from json cause an exception.",
                        inner: e,
                        WebRequestException.ExceptionTypeEnum.JsonToBytes
                    );
                    yield break;
                }
                uwr.uploadHandler = new UploadHandlerRaw(data: bytes)
                {
                    contentType = string.IsNullOrEmpty(data.contentType) ? ContentTypeJson : data.contentType
                };
                uwr.downloadHandler = new DownloadHandlerBuffer();

                if (!string.IsNullOrEmpty(credentials.token))
                {
                    uwr.SetRequestHeader(name: common.UMI3DNetworkingKeys.Authorization, value: credentials.token);
                }
                if (credentials.headers != null)
                {
                    foreach ((string name, string value) item in credentials.headers)
                    {
                        uwr.SetRequestHeader(item.name, item.value);
                    }
                }

                DateTime date = DateTime.UtcNow;

                UnityWebRequestAsyncOperation operation = uwr.SendWebRequest();

                while (!operation.isDone)
                {
                    if (shouldCleanAbort?.Invoke() ?? false)
                    {
                        logger.Debug($"{nameof(Post)}", $"Caller requests to abort the PostRequest in a clean way.", report: report);
                        yield break;
                    }
                    yield return null;
                }

#if UNITY_2020_1_OR_NEWER
                if (uwr.result > UnityWebRequest.Result.Success)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    onCompleteFail?.Invoke(operation);
                }
                else
                {
                    onCompleteSuccess?.Invoke(operation);
                }
            }
        }

        /// <summary>
        /// An exception class to deal with <see cref="LauncherOnWorldController"/> issues.
        /// </summary>
        [Serializable]
        public class WebRequestException : Exception
        {
            static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(WebRequestException)}");

            public enum ExceptionTypeEnum
            {
                Unknown,
                JsonToBytes,
            }

            public ExceptionTypeEnum exceptionType;

            public WebRequestException(string message, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}")
            {
                this.exceptionType = exceptionType;
            }
            public WebRequestException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}", inner)
            {
                this.exceptionType = exceptionType;
            }

            public static void LogException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown)
            {
                logger.Exception(null, new WebRequestException(message, inner, exceptionType));
            }
        }
    }
}
