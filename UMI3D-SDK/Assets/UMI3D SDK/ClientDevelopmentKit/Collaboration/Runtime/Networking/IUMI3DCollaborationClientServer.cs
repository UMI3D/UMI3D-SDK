﻿/*
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
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{
    public interface IUMI3DCollaborationClientServer : IUMI3DClientServer
    {
        bool IsRedirectionInProgress { get; }
        UnityEvent OnConnectionCheck { get; }
        UnityEvent OnConnectionLost { get; }
        UnityEvent OnConnectionRetreived { get; }
        OnForceLogoutEvent OnForceLogoutMessage { get; }
        UnityEvent OnNewToken { get; }
        UnityEvent OnReconnect { get; }
        UnityEvent OnRedirection { get; }
        UnityEvent OnRedirectionAborted { get; }
        StatusType status { get; set; }

        void ConnectionLost(UMI3DEnvironmentClient client);

        void ConnectionStatus(UMI3DEnvironmentClient client, bool lost);
    }
}