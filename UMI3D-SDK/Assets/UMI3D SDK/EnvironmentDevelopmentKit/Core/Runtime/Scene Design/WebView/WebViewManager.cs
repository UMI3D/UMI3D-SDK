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
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk
{
    /// <summary>
    /// Class to handle <see cref="WebViewUrlChangedRequestDto"/> from browsers
    /// </summary>
    public class WebViewManager : Singleton<WebViewManager>
    {
        /// <summary>
        /// Event used when a user changes its url webView.
        /// <User, webViewId, url>
        /// </summary>
        public class WebViewEvent : UnityEvent<UMI3DUser, ulong, string>
        {

        }

        public readonly WebViewEvent onUserChangedUrlEvent = new WebViewEvent();

        /// <summary>
        /// Notify the manager a <see cref="WebViewUrlChangedRequestDto"/> was received.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="webViewId"></param>
        /// <param name="url"></param>
        public void OnUserChangedUrl(UMI3DUser user, ulong webViewId, string url)
        {
            try
            {
                onUserChangedUrlEvent?.Invoke(user, webViewId, url);
            } catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}