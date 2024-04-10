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

using System.Collections.Generic;
using umi3d.common;

namespace umi3d.edk
{
    public interface IUMI3DServer
    {
        UMI3DUserEvent OnUserActive { get; }
        UMI3DUserEvent OnUserAway { get; }
        UMI3DUserEvent OnUserCreated { get; }
        UMI3DUserEvent OnUserJoin { get; }
        UMI3DUserEvent OnUserRefreshed { get; }
        UMI3DUserEvent OnUserLeave { get; }
        UMI3DUserEvent OnUserMissing { get; }
        UMI3DUserEvent OnUserReady { get; }
        UMI3DUserEvent OnUserRecreated { get; }
        UMI3DUserEvent OnUserRegistered { get; }
        UMI3DUserEvent OnUserUnregistered { get; }

        void NotifyUserRefreshed(UMI3DUser user);
        void NotifyUserChanged(UMI3DUser user);
        void NotifyUserStatusChanged(UMI3DUser user, StatusType status);
        float ReturnServerTime();
        IEnumerable<UMI3DUser> Users();
        HashSet<UMI3DUser> UserSet();
        HashSet<UMI3DUser> UserSetWhenHasJoined();

        /// <summary>
        /// Send a <see cref="Transaction"/> to all clients.
        /// </summary>
        /// <param name="transaction"></param>
        void DispatchTransaction(Transaction transaction);
    }
}