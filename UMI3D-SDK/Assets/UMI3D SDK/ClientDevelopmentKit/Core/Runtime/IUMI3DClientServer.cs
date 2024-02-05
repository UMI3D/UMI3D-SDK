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

using System.Threading.Tasks;

using umi3d.common;

using UnityEngine.Events;

namespace umi3d.cdk
{
    public interface IUMI3DClientServer
    {
        bool AuthorizationInHeader { get; }

        /// <summary>
        /// Triggered when the browser logout of the environment on purpose.
        /// </summary>
        UnityEvent OnLeaving { get; }

        /// <summary>
        /// Triggered when the browser logout of the environment, purposefully or not.
        /// </summary>
        UnityEvent OnLeavingEnvironment { get; }

        UnityEvent OnConnectionCheck { get; }

        UnityEvent OnConnectionLost { get; }

        UnityEvent OnConnectionRetreived { get; }

        UnityEvent OnNewToken { get; }

        UnityEvent OnReconnect { get; }

        /// <summary>
        /// Triggered when a connection to a new environment occurs.
        /// </summary>
        UnityEvent OnRedirection { get; }

        UnityEvent OnRedirectionAborted { get; }

        UnityEvent OnRedirectionStarted { get; }

        UMI3DVersion.Version version { get; }

        object GetHttpClient();

        double GetRoundTripLAtency();

        ulong GetTime();

        ulong GetUserId();

        Task<bool> TryAgainOnHttpFail(RequestFailedArgument argument);

        void SendTracking(AbstractBrowserRequestDto dto);

        void _SendRequest(AbstractBrowserRequestDto dto, bool reliable);
    }
}