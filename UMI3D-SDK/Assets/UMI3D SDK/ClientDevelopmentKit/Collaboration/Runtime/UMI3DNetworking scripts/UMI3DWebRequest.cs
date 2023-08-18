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
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    /// <summary>
    /// A class responsible for 
    /// </summary>
    internal static class UMI3DWebRequest
    {
        private static UMI3DLogger s_logger = new UMI3DLogger(mainTag: $"static_{nameof(UMI3DWebRequest)}");

        /// <summary>
        /// Send an HTTP Get Request.
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="url"></param>
        /// <param name="shouldCleanAbort"></param>
        /// <param name="onCompleteSuccess"></param>
        /// <param name="onCompleteFail"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public static IEnumerator Get(
            (string token, List<(string, string)> headers) credentials,
            string url,
            Func<bool> shouldCleanAbort,
            Action<UnityWebRequestAsyncOperation> onCompleteSuccess,
            Action<UnityWebRequestAsyncOperation> onCompleteFail,
            UMI3DLogReport report = null
        )
        {
            if (shouldCleanAbort())
            {
                s_logger.Debug($"{nameof(Get)}", $"Caller requests to abort the GetRequest in a clean way.");
                yield break;
            }

            s_logger.DebugTab(
                tabName: "Request",
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
                        null,
                        20
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
                    if (shouldCleanAbort())
                    {
                        s_logger.Debug($"{nameof(Get)}", $"Caller requests to abort the GetRequest in a clean way.");
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
    }
}
