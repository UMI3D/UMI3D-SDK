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
using System.Collections.Generic;
using umi3d.common;

namespace umi3d.cdk
{
    public class UMI3DClientServer : PersistentSingleton<UMI3DClientServer>
    {
        /// <summary>
        /// Environment connected to.
        /// </summary>
        protected MediaDto environment;

        /// <summary>
        /// Environment connected to.
        /// </summary>
        public static MediaDto Media
        {
            get => Exists ? Instance.environment : null;
            set
            {
                if (Exists)
                    Instance.environment = value;
            }
        }

        public static string getAuthorization()
        {
            if (Exists)
                return Instance._getAuthorization();
            return null;
        }
        protected virtual string _getAuthorization() { return null; }

        /// <summary>
        /// Retry a failed http request
        /// </summary>
        /// <param name="argument">failed request argument</param>
        /// <returns></returns>
        public virtual bool TryAgainOnHttpFail(RequestFailedArgument argument)
        {
            return false;
        }

        public static void SendData(AbstractBrowserRequestDto dto, bool reliable)
        {
            if (Exists)
                Instance._Send(dto, reliable);
        }

        protected virtual void _Send(AbstractBrowserRequestDto dto, bool reliable) { }

        public static void SendTracking(AbstractBrowserRequestDto dto)
        {
            if (Exists)
                Instance._SendTracking(dto);
        }
        protected virtual void _SendTracking(AbstractBrowserRequestDto dto) { }


        public static void GetFile(string url, Action<byte[]> callback, Action<string> onError)
        {
            if (Exists)
                Instance._GetFile(url, callback, onError);
        }

        protected virtual void _GetFile(string url, Action<byte[]> callback, Action<string> onError) { onError.Invoke("GetFile Not Implemented"); }

        public static void GetEntity(List<ulong> ids, Action<LoadEntityDto> callback, Action<string> onError)
        {
            if (Exists)
                Instance._GetEntity(ids, callback, onError);
        }

        protected virtual void _GetEntity(List<ulong> id, Action<LoadEntityDto> callback, Action<string> onError) { onError.Invoke("GetEntity Not Implemented"); }

        public virtual ulong GetId() { return 0; }

        /// <summary>
        /// return time server in millisecond, use synchronised time in collaborative cases.
        /// </summary>
        /// <returns></returns>
        public virtual ulong GetTime() { return (ulong)(long)DateTime.Now.Millisecond; }

        /// <summary>
        /// return HTTPClient if the server is a collaboration server. return null in that case
        /// </summary>
        public virtual Object GetHttpClient() { return null; }

    }
}