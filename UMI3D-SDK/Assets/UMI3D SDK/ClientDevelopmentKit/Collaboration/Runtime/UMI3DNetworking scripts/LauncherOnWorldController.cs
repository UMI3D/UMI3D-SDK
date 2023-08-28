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
using System.Security.AccessControl;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using umi3d.debug;
using static umi3d.cdk.collaboration.HttpClient;

namespace umi3d.cdk.collaboration
{
    internal static class LauncherOnWorldController
    {
        #region private

        static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(LauncherOnWorldController)}");

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

            yield return UMI3DNetworking.Get_WR(
                credentials: (null, null),
                curentUrl,
                shouldCleanAbort,
                onCompleteSuccess: op =>
                {
                    var uwr = op.webRequest;

                    if (uwr?.downloadHandler.data == null)
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
        /// A connection is simply a redirection from nowhere.
        /// </summary>
        /// <param name="mediaDto"></param>
        /// <param name="language"></param>
        /// <param name="shouldCleanAbort"></param>
        /// <param name="formReceived"></param>
        /// <param name="formAnswerReceived"></param>
        /// <param name="connectionStarted"></param>
        /// <param name="connectionSucceeded"></param>
        /// <param name="connectionFailed"></param>
        /// <param name="maxTryCount"></param>
        /// <param name="report"></param>
        public static IEnumerator Connect(
            MediaDto mediaDto, 
            string language,
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
                globalToken: null, language,
                shouldCleanAbort,
                formReceived, formAnswerReceived,
                redirectionStarted: connectionStarted,
                redirectionSucceeded: connectionSucceeded,
                redirectionFailed: connectionFailed,
                maxTryCount, report
            );
        }

        public static IEnumerator Redirect(
            RedirectionDto redirectionDto,
            string globalToken, string language,
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
            redirectionStarted?.Invoke();
            status = StatusType.AWAY;
            ConnectionDto connectionDto = new ()
            {
                gate = redirectionDto.gate,
                globalToken = globalToken,
                language = language,
                libraryPreloading = false,
                metadata = null
            };
            string connectionUrl = redirectionDto.media.url + UMI3DNetworkingKeys.connect;

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

                IEnumerator nextTryEnumerator = null;
                bool waitForFormAnswer = false;

                yield return UMI3DNetworking.Post_WR(
                    credentials: (null, null),
                    connectionUrl,
                    data: (UMI3DNetworking.ContentTypeJson, json),
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
                            redirectionSucceeded?.Invoke();
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
                            $"{connectionUrl}".FormatString(40) +
                            "   " +
                            $"{tryCount}" +
                            $"\n{op.webRequest.error}",
                            report: failRequestReporter
                        );

                        if (tryCount < maxTryCount - 1)
                        {
                            nextTryEnumerator = SendConnectionDto(connectionDto, json, tryCount + 1);
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

                if (nextTryEnumerator != null)
                {
                    yield return nextTryEnumerator;
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
