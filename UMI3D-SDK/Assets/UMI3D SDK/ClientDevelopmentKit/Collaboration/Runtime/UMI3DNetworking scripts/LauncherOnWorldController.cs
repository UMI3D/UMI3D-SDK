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
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using umi3d.debug;
using static umi3d.cdk.collaboration.HttpClient;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Used to connect to a World Controller, when a Master Server is not used.
    /// </summary>
    internal static class LauncherOnWorldController
    {
        #region private

        static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(LauncherOnWorldController)}");

        static RedirectionDto redirectionDto;
        static PrivateIdentityDto privateIdentityDto;

        #endregion

        /// <summary>
        /// Whether or not a connection or redirection is in progress.
        /// </summary>
        public static bool IsConnectingOrRedirecting;

        /// <summary>
        /// The status of the user in the server.
        /// </summary>
        public static StatusType status;

        #region Observables

        static UMI3DObservable redirectionStartedObservable = new UMI3DObservable();
        static UMI3DObservable redirectionSucceededObservable = new UMI3DObservable();
        static UMI3DObservable redirectionFailedObservable = new UMI3DObservable();

        /// <summary>
        /// Notifies observers that the redirection has started.
        /// </summary>
        public static IUMI3DObservable<(Type observer, string purpose)> RedirectionStartedObservable
        {
            get
            {
                return redirectionStartedObservable;
            }
        }
        /// <summary>
        /// Notifies observers that the redirection has succeeded.
        /// </summary>
        public static IUMI3DObservable<(Type observer, string purpose)> RedirectionSucceededObservable
        {
            get
            {
                return redirectionSucceededObservable;
            }
        }
        /// <summary>
        /// Notifies observers that the redirection has failed.
        /// </summary>
        public static IUMI3DObservable<(Type observer, string purpose)> RedirectionFailedObservable
        {
            get
            {
                return redirectionFailedObservable;
            }
        }

        #endregion

        #region Dtos

        /// <summary>
        /// Called to create a new Public Identity for this client.
        /// </summary>
        public static PublicIdentityDto PublicIdentity
        {
            get
            {
                if (privateIdentityDto != null)
                {
                    return new PublicIdentityDto()
                    {
                        userId = privateIdentityDto.userId,
                        login = privateIdentityDto.login,
                        displayName = privateIdentityDto.displayName
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Called to create a new Identity for this client.
        /// </summary>
        public static IdentityDto Identity
        {
            get
            {
                if (privateIdentityDto != null)
                {
                    return new IdentityDto()
                    {
                        userId = privateIdentityDto.userId,
                        login = privateIdentityDto.login,
                        displayName = privateIdentityDto.displayName,
                        guid = privateIdentityDto.guid,
                        headerToken = privateIdentityDto.headerToken,
                        localToken = privateIdentityDto.localToken,
                        key = privateIdentityDto.key
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Create a new media dto from the one that is currently used.
        /// </summary>
        public static MediaDto MediaDto
        {
            get
            {
                if (redirectionDto != null && redirectionDto.media != null)
                {
                    return new MediaDto
                    {
                        name = redirectionDto.media.name,
                        url = redirectionDto.media.url,
                        icon2D = redirectionDto.media.icon2D,
                        icon3D = redirectionDto.media.icon3D
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Create a new gate dto from the one that is currently used.
        /// </summary>
        public static GateDto GateDto
        {
            get
            {
                if (redirectionDto != null && redirectionDto.gate != null)
                {
                    return new GateDto
                    {
                        gateId = redirectionDto.gate.gateId,
                        metaData = redirectionDto.gate.metaData
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Create a new environment connection dto from the one that is currently used.
        /// </summary>
        public static EnvironmentConnectionDto EnvironmentConnectionDto
        {
            get
            {
                if (privateIdentityDto != null && privateIdentityDto.connectionDto != null)
                {
                    return new EnvironmentConnectionDto
                    {
                        authorizationInHeader = privateIdentityDto.connectionDto.authorizationInHeader,
                        forgeHost = privateIdentityDto.connectionDto.forgeHost,
                        forgeMasterServerHost = privateIdentityDto.connectionDto.forgeMasterServerHost,
                        forgeMasterServerPort = privateIdentityDto.connectionDto.forgeMasterServerPort,
                        forgeNatServerHost = privateIdentityDto.connectionDto.forgeNatServerHost,
                        forgeNatServerPort = privateIdentityDto.connectionDto.forgeNatServerPort,
                        forgeServerPort = privateIdentityDto.connectionDto.forgeServerPort,
                        httpUrl = privateIdentityDto.connectionDto.httpUrl,
                        name = privateIdentityDto.connectionDto.name,
                        resourcesUrl = privateIdentityDto.connectionDto.resourcesUrl,
                        version = privateIdentityDto.connectionDto.version
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region Shortcut data.

        /// <summary>
        /// The current url to the media dto.
        /// </summary>
        public static string MediaDtoUrl
        {
            get
            {
                return redirectionDto?.media?.url;
            }
        }

        /// <summary>
        /// The current url where the connection dto is sent.
        /// </summary>
        public static string ConnectionDtoUrl
        {
            get
            {
                return !string.IsNullOrEmpty(MediaDtoUrl)
                    ? MediaDtoUrl + UMI3DNetworkingKeys.connect
                    : null;
            }
        }

        /// <summary>
        /// Name of the world.
        /// </summary>
        public static string WorldName
        {
            get
            {
                return redirectionDto?.media?.name;
            }
        }

        /// <summary>
        /// Name of the environment.
        /// </summary>
        public static string EnvironmentName
        {
            get
            {
                return privateIdentityDto?.connectionDto?.name;
            }
        }

        /// <summary>
        /// Create a new UMI3DVersion.Version corresponding to the version of the environment.
        /// </summary>
        public static UMI3DVersion.Version EnvironmentUMI3DVersion
        {
            get
            {
                if (privateIdentityDto != null && privateIdentityDto.connectionDto != null)
                {
                    return new UMI3DVersion.Version(privateIdentityDto.connectionDto.version);
                }
                else
                {
                    return null;
                }
            }
        }

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
        public static IEnumerator RequestMediaDto(
                string RawURL,
                Action<MediaDto> requestSucceeded, Action<int> requestFailed, Func<bool> shouldCleanAbort,
                int tryCount = 0, int maxTryCount = 3, UMI3DLogReport report = null
        )
        {
            if (shouldCleanAbort?.Invoke() ?? false)
            {
                logger.Debug($"{nameof(RequestMediaDto)}", $"Caller requests to abort the connection with MediaDto in a clean way.", report: report);
                yield break;
            }

            string curentUrl = RawURL;
            if (!curentUrl.EndsWith(UMI3DNetworkingKeys.media))
            {
                curentUrl += UMI3DNetworkingKeys.media;
            }
            if (!curentUrl.StartsWith("http://") && !curentUrl.StartsWith("https://"))
            {
                curentUrl = "http://" + curentUrl;
            }

            var getRequestReporter = logger.GetReporter("RequestMediaDTOGet");
            var failRequestReporter = logger.GetReporter("RequestMediaDTOFail");

            IEnumerator nextTryEnumerator = null;

            yield return UMI3DNetworking.webRequest.Get(
                credentials: (null, null),
                curentUrl,
                shouldCleanAbort,
                onCompleteSuccess: op =>
                {
                    var uwr = op.webRequest;

                    if (uwr?.downloadHandler?.data == null)
                    {
                        logger.DebugAssertion($"{nameof(RequestMediaDto)}", $"downloadHandler.data == null.");
                        getRequestReporter.Report();
                        failRequestReporter.Report();
                        return;
                    }

                    string json = null;
                    MediaDto mediaDto = null;
                    try
                    {
                        json = System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data);
                        mediaDto = UMI3DDtoSerializer.FromJson<MediaDto>(json, Newtonsoft.Json.TypeNameHandling.None);
                    }
                    catch (Exception e)
                    {
                        WorldControllerException.LogException(
                            $"Trying to get media dto from json cause an exception.",
                            inner: e,
                            WorldControllerException.ExceptionTypeEnum.MediaDtoFromJson
                        );
                        getRequestReporter.Report();
                        failRequestReporter.Report();
                        return;
                    }

                   logger.Default($"{nameof(RequestMediaDto)}", $"Request at: {RawURL} is a success.");
                    getRequestReporter.Clear();
                    failRequestReporter.Clear();
                    requestSucceeded?.Invoke(mediaDto);
                },
                onCompleteFail: op =>
                {
                    if (shouldCleanAbort?.Invoke() ?? false)
                    {
                        logger.Debug($"{nameof(RequestMediaDto)}", $"Caller requests to abort the connection with MediaDto in a clean way.", report: report);
                        return;
                    }

                    logger.Assertion(
                        tag: $"{nameof(RequestMediaDto)}",
                        $"MediaDto failed:   " +
                        $"{op.webRequest.result}".FormatString(19) +
                        "   " +
                        $"{curentUrl}".FormatString(40) +
                        "   " +
                        $"{tryCount}" +
                        $"\n{op.webRequest.error}",
                        report: failRequestReporter
                        );

                    if (tryCount < maxTryCount - 1)
                    {
                        nextTryEnumerator = RequestMediaDto(RawURL, requestSucceeded, requestFailed, shouldCleanAbort, tryCount + 1, maxTryCount, report);
                    }
                    else
                    {
                        logger.Error($"{nameof(RequestMediaDto)}", $"MediaDto failed more than 3 times. Connection has been aborted.");
                        getRequestReporter.Report();
                        failRequestReporter.Report();
                    }

                    requestFailed?.Invoke(tryCount);
                },
                getRequestReporter
            );

            if (nextTryEnumerator != null)
            {
                yield return nextTryEnumerator;
            }
        }

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
        public static IEnumerator Connect(
            MediaDto mediaDto,
            Func<bool> shouldCleanAbort,
            Action<ConnectionFormDto> formReceived, Func<FormConnectionAnswerDto> formAnswerReceived,
            Action connectionStarted, Action connectionSucceeded, Action connectionFailed,
            int maxTryCount = 3, UMI3DLogReport report = null
        )
        {
            return Redirect(
                new RedirectionDto
                {
                    media = mediaDto,
                    gate = null
                },
                shouldCleanAbort,
                formReceived, formAnswerReceived,
                redirectionStarted: connectionStarted,
                redirectionSucceeded: connectionSucceeded,
                redirectionFailed: connectionFailed,
                maxTryCount, report
            );
        }

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
        public static IEnumerator Redirect(
            RedirectionDto redirectionDto,
            Func<bool> shouldCleanAbort,
            Action<ConnectionFormDto> formReceived, Func<FormConnectionAnswerDto> formAnswerReceived,
            Action redirectionStarted, Action redirectionSucceeded, Action redirectionFailed,
            int maxTryCount = 3, UMI3DLogReport report = null
        )
        {
            if (shouldCleanAbort?.Invoke() ?? false)
            {
                logger.Debug($"{nameof(Redirect)}", $"Caller requests to abort the redirection in a clean way.", report: report);
                yield break;
            }

            if (IsConnectingOrRedirecting)
            {
                WorldControllerException.LogException(
                    $"Trying to redirect when a redirection is already in progress.",
                    inner: null,
                    WorldControllerException.ExceptionTypeEnum.RedirectionAlreadyInProgress
                );
                yield break;
            }

            IsConnectingOrRedirecting = true;
            status = StatusType.AWAY;

            redirectionStarted?.Invoke();
            redirectionStartedObservable.Notify();

            logger.DebugTodo($"{nameof(Redirect)}", $"Let the user choose the language of the environment.");
            ConnectionDto connectionDto = new ()
            {
                gate = redirectionDto?.gate,
                globalToken = redirectionDto?.media?.url == MediaDtoUrl ? privateIdentityDto?.globalToken : null,
                language = null,
                libraryPreloading = false,
                metadata = null
            };

            LauncherOnWorldController.redirectionDto = redirectionDto;

            IEnumerator SendConnectionDto(ConnectionDto connectionDto, string json = null, int tryCount = 0)
            {
                if (shouldCleanAbort?.Invoke() ?? false)
                {
                    logger.Debug($"{nameof(SendConnectionDto)}", $"Caller requests to abort the redirection in a clean way.", report: report);
                    yield break;
                }

                if (string.IsNullOrEmpty(json))
                {
                    try
                    {
                        json = connectionDto.ToJson(Newtonsoft.Json.TypeNameHandling.None);
                    }
                    catch (Exception e)
                    {
                        WorldControllerException.LogException(
                            "Trying to get json from connectionDto cause an exception.",
                            inner: e,
                            WorldControllerException.ExceptionTypeEnum.ConnectionDtoToJson
                        );
                        IsConnectingOrRedirecting = false;
                        yield break;
                    }
                }

                bool tryAgain = false;
                bool waitForFormAnswer = false;

                yield return UMI3DNetworking.webRequest.Post(
                    credentials: (null, null),
                    ConnectionDtoUrl,
                    data: (UMI3DNetworking.webRequest.ContentTypeJson, json),
                    shouldCleanAbort,
                    onCompleteSuccess: op =>
                    {
                        if (shouldCleanAbort?.Invoke() ?? false)
                        {
                            logger.Debug($"{nameof(SendConnectionDto)}", $"Caller requests to abort the redirection in a clean way.", report: report);
                            return;
                        }

                        string answerString = null;
                        try
                        {
                            answerString = System.Text.Encoding.UTF8.GetString(op.webRequest.downloadHandler.data);
                        }
                        catch (Exception e)
                        {
                            WorldControllerException.LogException(
                                $"Trying to get string from webRequest.downloadHandler.data cause an exception.",
                                inner: e, 
                                WorldControllerException.ExceptionTypeEnum.WRDownloadHandlerToString
                            );
                            return;
                        }

                        var answerDtoReport = logger.GetReporter("AnswerDto");
                        bool TryGetAnswerDto<DtoType>(out DtoType answer, List<JsonConverter> converters = null)
                            where DtoType : UMI3DDto
                        {
                            DtoType dto = null;
                            try
                            {
                                dto = UMI3DDtoSerializer.FromJson<DtoType>(answerString, Newtonsoft.Json.TypeNameHandling.None, converters);
                            }
                            catch (Exception e)
                            {
                                logger.Assertion(
                                    $"{nameof(TryGetAnswerDto)}", 
                                    $"Trying to get the post answer dto fail: {typeof(DtoType).Name}\n" +
                                    $"{e.Message}", 
                                    report: answerDtoReport
                                );
                            }

                            answer = dto;
                            return dto != null;
                        }

                        if (
                            TryGetAnswerDto(out PrivateIdentityDto privateIdentityDto)
                            && !string.IsNullOrEmpty(privateIdentityDto.globalToken)
                            && privateIdentityDto.connectionDto != null
                        )
                        {
                            LauncherOnWorldController.privateIdentityDto = privateIdentityDto;
                            RedirectionSucceededObservable.Notify();
                            redirectionSucceeded?.Invoke();
                            //status = StatusType.CREATED;
                        }
                        //else if (!TryGetAnswerDto<FakePrivateIdentityDto>(out postAnswerDto))
                        //{

                        //}
                        else if (
                            TryGetAnswerDto(
                                out ConnectionFormDto connectionFormDto, 
                                new List<JsonConverter>() { new ParameterConverter() }
                            )
                        )
                        {
                            waitForFormAnswer = true;
                            formReceived?.Invoke(connectionFormDto);
                        }
                        else
                        {
                            answerDtoReport.Report();
                            redirectionFailed?.Invoke();
                        }
                    },
                    onCompleteFail: op =>
                    {
                        if (shouldCleanAbort?.Invoke() ?? false)
                        {
                            logger.Debug($"{nameof(SendConnectionDto)}", $"Caller requests to abort the redirection in a clean way.", report: report);
                            return;
                        }

                        var failRequestReporter = logger.GetReporter("RequestConnectionDTOFail");
                        logger.Assertion(
                            tag: $"{nameof(SendConnectionDto)}",
                            $"Send connectionDto failed:   " +
                            $"{op.webRequest.result}".FormatString(19) +
                            "   " +
                            $"{ConnectionDtoUrl}".FormatString(40) +
                            "   " +
                            $"{tryCount}" +
                            $"\n{op.webRequest.error}",
                            report: failRequestReporter
                        );

                        if (tryCount < maxTryCount - 1)
                        {
                            tryAgain = true;
                        }
                        else
                        {
                            logger.Error($"{nameof(SendConnectionDto)}", $"MediaDto failed more than 3 times. Connection has been aborted.");
                            report.Report();
                            failRequestReporter.Report();
                        }
                    },
                    report
                );

                if (tryAgain)
                {
                    yield return SendConnectionDto(connectionDto, json, tryCount + 1);
                }
                else if (waitForFormAnswer)
                {
                    FormConnectionAnswerDto formAnswerDto = null;

                    do
                    {
                        formAnswerDto = formAnswerReceived?.Invoke();
                        if (formAnswerDto == null)
                        {
                            yield return null;
                        }
                    } while (formAnswerDto == null);

                    yield return SendConnectionDto(formAnswerDto);
                }
            }

            yield return SendConnectionDto(connectionDto);

            IsConnectingOrRedirecting = false;
        }


        /// <summary>
        /// An exception class to deal with <see cref="LauncherOnWorldController"/> issues.
        /// </summary>
        [Serializable]
        public class WorldControllerException : Exception
        {
            static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(WorldControllerException)}");

            public enum ExceptionTypeEnum
            {
                Unknown,
                MediaDtoFromJson,
                RedirectionAlreadyInProgress,
                ConnectionDtoToJson,
                WRDownloadHandlerToString
            }

            public ExceptionTypeEnum exceptionType;

            public WorldControllerException(string message, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}")
            {
                this.exceptionType = exceptionType;
            }
            public WorldControllerException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}", inner)
            {
                this.exceptionType = exceptionType;
            }

            public static void LogException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown)
            {
                logger.Exception(null, new WorldControllerException(message, inner, exceptionType));
            }
        }
    }
}
