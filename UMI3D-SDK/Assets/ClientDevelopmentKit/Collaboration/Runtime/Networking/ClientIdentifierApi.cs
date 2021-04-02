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
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Api to give a password for the client, return filled up Form, and state if needed libraries should be downloaded.
    /// </summary>
    public abstract class ClientIdentifierApi : ScriptableObject
    {
        /// <summary>
        /// Should return a login and a password as a string for this client via the callback. 
        /// </summary>
        /// <param name="callback">Action to return the login and the password.</param>
        public abstract void GetIdentity(Action<common.collaboration.UMI3DAuthenticator> callback);

        /// <summary>
        /// Should fill a formDto and return it via a callback.
        /// </summary>
        /// <param name="parameter">FormDto to be filled.</param>
        /// <param name="callback">Action to return the completed FormDto.</param>
        public virtual void GetParameterDtos(FormDto parameter, Action<FormDto> callback)
        {
            callback.Invoke(parameter);
        }

        /// <summary>
        /// Should State if the needed libraries should be downloaded.
        /// </summary>
        /// <param name="LibrariesId">Ids of all library that need to be downloaded or updated</param>
        /// <param name="callback">Action to return the answer</param>
        public virtual void ShouldDownloadLibraries(List<string> LibrariesId, Action<bool> callback)
        {
            callback.Invoke(true);
        }
    }
}