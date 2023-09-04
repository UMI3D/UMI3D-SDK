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
using BeardedManStudios.Forge.Networking;
using inetum.unityUtils;
using System;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.interaction;
using umi3d.common;
using umi3d.debug;
using System.Collections;

namespace umi3d.cdk.collaboration
{
    public interface IUMI3DWorldConnection : IUMI3DMasterServerConnection, IUMI3DWorldControllerConnection
    {
        
    }

    /// <summary>
    /// Interface responsible for 
    /// </summary>
    public interface IUMI3DMasterServerConnection
    {
        /// <summary>
        /// Try to connect to a master server asynchronously.
        /// 
        /// <para>
        /// The connection is established in another thread. The <paramref name="connectSucceeded"/> and <paramref name="connectFailed"/> actions are raised in the main-thread.
        /// </para>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="connectSucceeded">Action raise when the connection succeeded.</param>
        /// <param name="connectFailed">Action raise when the connection failed.</param>
        /// <returns></returns>
        UMI3DAsyncOperation Connect(string url, Action connectSucceeded, Action connectFailed);
    }

    public interface IUMI3DWorldControllerConnection
    {
        #region Observables

        /// <summary>
        /// Notifies observers that the redirection has started.
        /// </summary>
        public IUMI3DObservable<(Type observer, string purpose)> redirectionStartedObservable { get; }
        /// <summary>
        /// Notifies observers that the redirection has succeeded.
        /// </summary>
        public IUMI3DObservable<(Type observer, string purpose)> redirectionSucceededObservable { get; }
        /// <summary>
        /// Notifies observers that the redirection has failed.
        /// </summary>
        public IUMI3DObservable<(Type observer, string purpose)> redirectionFailedObservable { get; }

        #endregion

        /// <summary>
        /// Send a request to get a <see cref="MediaDto"/>.
        /// </summary>
        /// <param name="RawURL">A simplified version of the url where a media dto can be requested.</param>
        /// <param name="requestSucceeded">Action raised when a media dto has been found.</param>
        /// <param name="requestFailed">Action raised when the request failed.</param>
        /// <param name="shouldCleanAbort">Whether or not the request has been interrupted.</param>
        /// <param name="tryCount">The number of try.</param>
        /// <param name="maxTryCount">The maximum number of try before giving up.</param>
        /// <param name="report">A log report.</param>
        /// <returns></returns>
        IEnumerator RequestMediaDto(
                string RawURL,
                Action<MediaDto> requestSucceeded, Action<int> requestFailed, Func<bool> shouldCleanAbort,
                int tryCount = 0, int maxTryCount = 3, UMI3DLogReport report = null
        );

        /// <summary>
        /// Connect to a World Controller.
        /// 
        /// <para>
        ///  A connection is simply a redirection from nowhere.
        /// </para>
        /// </summary>
        /// <param name="mediaDto">The media dto of the world controller.</param>
        /// <param name="shouldCleanAbort">Whether or not the connection should be aborted.</param>
        /// <param name="formReceived">Action raised when a form is received.</param>
        /// <param name="formAnswerReceived">Return the answer to a form.</param>
        /// <param name="connectionStarted">Action raised when the connection has started.</param>
        /// <param name="connectionSucceeded">Action raised when the connection has succeeded.</param>
        /// <param name="connectionFailed">Action raised when the connection has failed.</param>
        /// <param name="maxTryCount">The maximum try count.</param>
        /// <param name="report">A log reporter.</param>
        IEnumerator Connect(
            MediaDto mediaDto,
            Func<bool> shouldCleanAbort,
            Action<ConnectionFormDto> formReceived, Func<FormConnectionAnswerDto> formAnswerReceived,
            Action connectionStarted, Action connectionSucceeded, Action connectionFailed,
            int maxTryCount = 3, UMI3DLogReport report = null
        );

        /// <summary>
        /// Redirect from one place to another.
        /// </summary>
        /// <param name="redirectionDto"></param>
        /// <param name="shouldCleanAbort">Whether or not the connection should be aborted.</param>
        /// <param name="formReceived">Action raised when a form is received.</param>
        /// <param name="formAnswerReceived">Return the answer to a form.</param>
        /// <param name="redirectionStarted">Action raised when the redirection has started.</param>
        /// <param name="redirectionSucceeded">Action raised when the redirection has succeeded.</param>
        /// <param name="redirectionFailed">Action raised when the redirection has failed.</param>
        /// <param name="maxTryCount">The maximum try count.</param>
        /// <param name="report">A log reporter.</param>
        /// <returns></returns>
        IEnumerator Redirect(
            RedirectionDto redirectionDto,
            Func<bool> shouldCleanAbort,
            Action<ConnectionFormDto> formReceived, Func<FormConnectionAnswerDto> formAnswerReceived,
            Action redirectionStarted, Action redirectionSucceeded, Action redirectionFailed,
            int maxTryCount = 3, UMI3DLogReport report = null
        );
    }
}
