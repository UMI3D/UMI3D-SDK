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

using inetum.unityUtils;
using System;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    /// <summary>
    /// Class to be sent to try to send a request again.
    /// </summary>
    public class RequestFailedArgument
    {
        public DateTime date { get; private set; }

        private readonly UnityWebRequest request;
        private readonly long responseCode = 0;
        private readonly string computedString;

        public long GetRespondCode()
        {
            return request?.responseCode ?? responseCode;
        }

        public string GetUrl()
        {
            return request?.url;
        }

        public System.Collections.Generic.Dictionary<string, string> GetHeader()
        {
            return request?.GetResponseHeaders();
        }

        public string ComputeString(string info)
        {
            if (request != null)
            {
                return $"Request failed [count:{count}, date:{date.ToString("G")}, code:{request.responseCode}, url:{request.url}, info:{info}], header:{request?.GetResponseHeaders()?.ToString(e => $"{{{e.Key}:{e.Value}}}")} ";
            }
            else
            {
                return $"Request failed [count:{count}, date:{date.ToString("G")}, code:{responseCode}, info:{info}]";
            }
        }

        public override string ToString()
        {
            return computedString;
        }

        public Func<RequestFailedArgument, bool> ShouldTryAgain { get; private set; }
        public RequestFailedArgument(UnityWebRequest request, int count, DateTime date, Func<RequestFailedArgument, bool> ShouldTryAgain, string info = null)
        {
            this.request = request;
            this.count = count;
            this.date = date;
            this.ShouldTryAgain = ShouldTryAgain;
            this.computedString = ComputeString(info);
        }

        public RequestFailedArgument(long responseCode, int count, DateTime date, Func<RequestFailedArgument, bool> ShouldTryAgain, string info = null)
        {
            this.request = null;
            this.responseCode = responseCode;
            this.count = count;
            this.date = date;
            this.ShouldTryAgain = ShouldTryAgain;
            this.computedString = ComputeString(info);
        }

        public int count { get; private set; }
    }

    /// <summary>
    /// Exception related to the UMI3D protocol.
    /// </summary>
    public class Umi3dException : Exception
    {

        public Umi3dException(string message) : base(message)
        {

        }
    }

    public class Umi3dNetworkingException : Umi3dException
    {
        public Umi3dNetworkingException(UnityWebRequest webRequest, string message) : base(message)
        {
            this.errorCode = webRequest?.responseCode ?? 0;
            this.errorMessage = webRequest?.error ?? "Web request is null";
            this.url = webRequest?.url ?? "";
        }

        public Umi3dNetworkingException(long errorCode, string errorMessage, string url, string message) : base(message)
        {
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
            this.url = url;
        }

        public long errorCode { get; protected set; }
        public string errorMessage { get; protected set; }
        public string url { get; protected set; }

        public override string ToString()
        {
            return $"Networking Error : Error code : {errorCode} | Error Message : {errorMessage} | Error Message : {url} | [ {base.ToString()} ]";
        }

    }
    public class Umi3dLoadingException : Umi3dException
    {

        public Umi3dLoadingException(string message) : base(message)
        {
        }


        public override string ToString()
        {
            return $" Loading Error [ {base.ToString()} ]";
        }

    }

}
