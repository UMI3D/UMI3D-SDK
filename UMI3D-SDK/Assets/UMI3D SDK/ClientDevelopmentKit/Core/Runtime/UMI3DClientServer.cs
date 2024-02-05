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
using UnityEngine.Events;

namespace umi3d.cdk
{
    /// <summary>
    /// UMI3D server on the browser.
    /// </summary>
    public class UMI3DClientServer : inetum.unityUtils.PersistentSingleBehaviour<UMI3DClientServer>, IUMI3DClientServer
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Networking;
        /// <summary>
        /// Environment connected to.
        /// </summary>
        protected MediaDto _media;

        public UnityEvent OnLeaving { get; } = new UnityEvent();
        public UnityEvent OnLeavingEnvironment { get; } = new UnityEvent();

        public UnityEvent OnNewToken { get; } = new UnityEvent();
        public UnityEvent OnConnectionLost { get; } = new UnityEvent();
        public UnityEvent OnRedirectionStarted { get; } = new UnityEvent();
        public UnityEvent OnRedirectionAborted { get; } = new UnityEvent();
        public UnityEvent OnRedirection { get; } = new UnityEvent();
        public UnityEvent OnReconnect { get; } = new UnityEvent();

        public UnityEvent OnConnectionCheck { get; } = new UnityEvent();
        public UnityEvent OnConnectionRetreived { get; } = new UnityEvent();

        public bool IsRedirectionInProgress { get; protected set; } = false;

        protected UMI3DTransactionDispatcher _transactionDispatcher;
        public static UMI3DTransactionDispatcher transactionDispatcher
        {
            get
            {
                return Exists ? Instance._transactionDispatcher : null;
            }
            set
            {
                if (Exists)
                {
                    Instance._transactionDispatcher = value;
                }
            }
        }

        /// <summary>
        /// Environment connected to.
        /// </summary>
        protected virtual EnvironmentConnectionDto connectionDto { get; }

        public virtual UMI3DVersion.Version version { get; }

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
        public static EnvironmentConnectionDto Environement => Exists ? Instance.connectionDto : null;

        // Enable to access the Collaboration implementation. Should not be there and will be reworked.
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


        [Obsolete("See SendRequest")]
        public static void SendData(AbstractBrowserRequestDto dto, bool reliable)
        {
            SendRequest(dto, reliable);
        }

        /// <summary>
        /// Send a browser request to the server.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="reliable">Should the request be reliable? Reliable are more expensive but are always delivered.</param>
        public static void SendRequest(AbstractBrowserRequestDto dto, bool reliable)
        {
            if (Exists)
                Instance._SendRequest(dto, reliable);
        }

        /// <summary>
        /// Send a browser request to the server.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="reliable">Should the request be reliable? Reliable are more expensive but are always delivered.</param>
        /// <seealso cref="SendRequest"/>
        public virtual void _SendRequest(AbstractBrowserRequestDto dto, bool reliable)
        {
            _Send(dto, reliable);
        }

        protected virtual void _Send(AbstractBrowserRequestDto dto, bool reliable) { }

        // Enable to access the Collaboration implementation. Should not be there and will be reworked.
        public virtual void SendTracking(AbstractBrowserRequestDto dto)
        {
        }

        // Enable to access the Collaboration implementation. Should not be there and will be reworked.
        public static async Task<byte[]> GetFile(string url, bool useParameterInsteadOfHeader)
        {
            try
            {
                if (Exists)
                {
                    byte[] bytes = await Instance._GetFile(url, useParameterInsteadOfHeader);
                    if (bytes != null)
                        return (bytes);
                    throw new Umi3dLoadingException($"No Data in response for {url}");
                }
                throw new Exception($"Instance of UMI3DClientServer is null");
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
                throw;
            }

        }

        protected virtual Task<byte[]> _GetFile(string url, bool useParameterInsteadOfHeader)
        {
            throw new NotImplementedException();
        }

        // Enable to access the Collaboration implementation. Should not be there and will be reworked.
        public static async Task<LoadEntityDto> GetEntity(List<ulong> ids)
        {
            if (Exists)
            {
                LoadEntityDto dto = await Instance._GetEntity(ids);
                return (dto);
            }
            else
                throw new Exception($"Instance of UMI3DClientServer is null");
        }

        protected virtual Task<LoadEntityDto> _GetEntity(List<ulong> id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the user ID attributed by the server.
        /// </summary>
        /// <returns></returns>
        /// Default is 0.
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

        // Enable to access the Collaboration implementation. Should not be there and will be reworked.
        public virtual double GetRoundTripLAtency() { return 0; }
    }
}