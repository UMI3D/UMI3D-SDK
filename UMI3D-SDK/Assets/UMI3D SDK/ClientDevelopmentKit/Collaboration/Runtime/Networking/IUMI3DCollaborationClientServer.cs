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

using umi3d.common;

namespace umi3d.cdk.collaboration
{
    public interface IUMI3DCollaborationClientServer : IUMI3DClientServer
    {
        /// <summary>
        /// Name of the environment currently connected to.
        /// </summary>
        string environementName { get; }

        OnForceLogoutEvent OnForceLogoutMessage { get; }

        /// <summary>
        /// True if a redirection is currently happening.
        /// </summary>
        bool IsRedirectionInProgress { get; }

        StatusType status { get; set; }
        string worldName { get; }

        void ConnectionLost(UMI3DEnvironmentClient client);

        void ConnectionStatus(UMI3DEnvironmentClient client, bool lost);
    }
}