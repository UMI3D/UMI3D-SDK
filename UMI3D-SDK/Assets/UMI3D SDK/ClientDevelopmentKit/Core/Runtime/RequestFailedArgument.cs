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
using UnityEngine.Networking;

namespace umi3d.cdk
{
    /// <summary>
    /// Class to be send to try to send a request again.
    /// </summary>
    public class RequestFailedArgument
    {

        Action tryAgain;
        public DateTime date { get; private set; }
        UnityWebRequest request;
        long responseCode = 0;

        public long GetRespondCode()
        {
            return request?.responseCode ?? responseCode;
        }


        public Func<RequestFailedArgument, bool> ShouldTryAgain { get; private set; }
        public RequestFailedArgument(UnityWebRequest request, Action tryAgain, int count, DateTime date, Func<RequestFailedArgument, bool> ShouldTryAgain)
        {
            this.request = request;
            this.tryAgain = tryAgain;
            this.count = count;
            this.date = date;
            this.ShouldTryAgain = ShouldTryAgain;
        }

        public RequestFailedArgument(long responseCode, Action tryAgain, int count, DateTime date, Func<RequestFailedArgument, bool> ShouldTryAgain)
        {
            this.request = null;
            this.responseCode = responseCode;
            this.tryAgain = tryAgain;
            this.count = count;
            this.date = date;
            this.ShouldTryAgain = ShouldTryAgain;
        }

        public int count { get; private set; }

        public virtual void TryAgain()
        {
            tryAgain.Invoke();
        }

    }

    public class Umi3dExecption : Exception
    {
        public Umi3dExecption(long errorCode, string message) : base(message)
        {
            this.errorCode = errorCode;
        }

        public Umi3dExecption(string message) : base(message)
        {
            this.errorCode = 0;
        }

        public long errorCode { get; protected set; }
    }

}
