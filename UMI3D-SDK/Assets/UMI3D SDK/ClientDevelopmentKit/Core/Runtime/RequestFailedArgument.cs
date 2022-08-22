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
    /// Class to be send to try to send a request again.
    /// </summary>
    public class RequestFailedArgument
    {
        public DateTime date { get; private set; }

        private readonly UnityWebRequest request;
        private readonly long responseCode = 0;

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

        public override string ToString()
        {
            if (request != null)
            {
                return $"Request failed [count:{count}, date:{date.ToString("G")}, code:{request.responseCode}, url:{request.url}], header:{request?.GetResponseHeaders()?.ToString(e => $"{{{e.Key}:{e.Value}}}")} ";
            }
            else
            {
                return $"Request failed [count:{count}, date:{date.ToString("G")}, code:{responseCode}]";
            }
        }

        public Func<RequestFailedArgument, bool> ShouldTryAgain { get; private set; }
        public RequestFailedArgument(UnityWebRequest request, int count, DateTime date, Func<RequestFailedArgument, bool> ShouldTryAgain)
        {
            this.request = request;
            this.count = count;
            this.date = date;
            this.ShouldTryAgain = ShouldTryAgain;
        }

        public RequestFailedArgument(long responseCode, int count, DateTime date, Func<RequestFailedArgument, bool> ShouldTryAgain)
        {
            this.request = null;
            this.responseCode = responseCode;
            this.count = count;
            this.date = date;
            this.ShouldTryAgain = ShouldTryAgain;
        }

        public int count { get; private set; }
    }

    public class Umi3dException : Exception
    {
        private readonly string stack = null;

        public override string StackTrace => stack ?? base.StackTrace;

        public Umi3dException(long errorCode, string message) : base(message)
        {
            this.errorCode = errorCode;
        }

        public Umi3dException(string message) : base(message)
        {
            this.errorCode = 0;
        }

        public Umi3dException(Exception exception, string message) : base(message + "\n" + exception.Message)
        {
            this.exception = exception;
            this.errorCode = 0;
            this.stack = exception.StackTrace;
        }

        public Umi3dException(Exception exception) : base(exception.Message)
        {
            this.exception = exception;
            this.errorCode = 0;
            this.stack = exception.StackTrace;
        }

        public long errorCode { get; protected set; }
        public readonly Exception exception;

        public override string ToString()
        {
            return $"code : {errorCode} | {base.ToString()} : [  {exception?.StackTrace ?? base.StackTrace} ]";
        }
    }
}
