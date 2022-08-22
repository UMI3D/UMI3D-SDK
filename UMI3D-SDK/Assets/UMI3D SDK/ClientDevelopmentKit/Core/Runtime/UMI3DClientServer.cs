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
using System.Threading.Tasks;
using umi3d.common;

namespace umi3d.cdk
{
    public class UMI3DClientServer : inetum.unityUtils.PersistentSingleBehaviour<UMI3DClientServer>
    {
        /// <summary>
        /// Environment connected to.
        /// </summary>
        protected MediaDto _media;
        /// <summary>
        /// Environment connected to.
        /// </summary>
        protected virtual ForgeConnectionDto connectionDto { get; }

        /// <summary>
        /// If true, authorizations must be set in headers.
        /// </summary>
        public bool AuthorizationInHeader => connectionDto?.authorizationInHeader ?? false;

        /// <summary>
        /// Environment connected to.
        /// </summary>
        public static MediaDto Media => Exists ? Instance._media : null;
        /// <summary>
        /// Environment connected to.
        /// </summary>
        public static ForgeConnectionDto Environement => Exists ? Instance.connectionDto : null;


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
        public virtual async Task<bool> TryAgainOnHttpFail(RequestFailedArgument argument)
        {
            return await Task.FromResult(false);
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


        public static async void GetFile(string url, Action<byte[]> callback, Action<string> onError, bool useParameterInsteadOfHeader)
        {
            if (Exists)
            {
                byte[] bytes = await Instance._GetFile(url, useParameterInsteadOfHeader);
                if (bytes != null)
                    callback.Invoke(bytes);
            }
            else
                throw new Exception($"Instance of UMI3DClientServer is null");
        }

        protected virtual Task<byte[]> _GetFile(string url, bool useParameterInsteadOfHeader)
        {
            throw new NotImplementedException();
        }

        public static async void GetEntity(List<ulong> ids, Action<LoadEntityDto> callback)
        {
            if (Exists)
            {
                LoadEntityDto dto = await Instance._GetEntity(ids);
                if (dto != null)
                    callback.Invoke(dto);
            }
            else
                throw new Exception($"Instance of UMI3DClientServer is null");
        }

        protected virtual Task<LoadEntityDto> _GetEntity(List<ulong> id)
        {
            throw new NotImplementedException();
        }

        public virtual ulong GetUserId() { return 0; }

        /// <summary>
        /// return time server in millisecond, use synchronised time in collaborative cases.
        /// </summary>
        /// <returns></returns>
        public virtual ulong GetTime() { return (ulong)(long)DateTime.Now.Millisecond; }

        /// <summary>
        /// return HTTPClient if the server is a collaboration server. return null in that case
        /// </summary>
        public virtual Object GetHttpClient() { return null; }

        public virtual double GetRoundTripLAtency() { return 0; }
    }
}