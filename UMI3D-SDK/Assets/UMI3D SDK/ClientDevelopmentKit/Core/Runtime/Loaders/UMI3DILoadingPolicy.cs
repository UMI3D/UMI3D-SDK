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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk
{

    public interface UMI3DILoadingPolicy
    {
        /// <summary>
        /// State if the loading should be attenpting once more 
        /// </summary>
        /// <param name="failedMessage"></param>
        /// <returns>null if not, a new policy else.</returns>
        UMI3DILoadingPolicy ShouldTryAgain(string failedMessage);
    }


    public class UMI3DDefaultLoadingPolicy : UMI3DILoadingPolicy
    {
        public int trycount = 0;
        public int maxTrycount = 3;

        public UMI3DDefaultLoadingPolicy(int maxTrycount)
        {
            this.trycount = 0;
            this.maxTrycount = maxTrycount;
        }

        public UMI3DILoadingPolicy ShouldTryAgain(string failedMessage)
        {
            if(trycount < maxTrycount)
            {
                trycount++;
                return this;
            }
            return null;
        }
    }
}