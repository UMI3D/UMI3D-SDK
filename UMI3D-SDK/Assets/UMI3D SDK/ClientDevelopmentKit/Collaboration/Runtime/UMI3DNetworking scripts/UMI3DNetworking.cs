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

using System.Collections.Generic;
using System;
using umi3d.debug;
using UnityEngine.Networking;
using System.Collections;

namespace umi3d.cdk
{
    public static class UMI3DNetworking
    {
        #region WebRequest

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
        public static IEnumerator Get_WR(
            (string token, List<(string, string)> headers) credentials,
            string url,
            Func<bool> shouldCleanAbort,
            Action<UnityWebRequestAsyncOperation> onCompleteSuccess,
            Action<UnityWebRequestAsyncOperation> onCompleteFail,
            UMI3DLogReport report = null
        )
        {
            return UMI3DWebRequest.Get(
                credentials, url,
                shouldCleanAbort, onCompleteSuccess, onCompleteFail,
                report
            );
        }

        #endregion
    }
}