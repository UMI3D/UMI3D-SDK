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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using umi3d.cdk;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;


namespace umi3d.cdk.collaboration
{
    public class LocalInfoSender
    {
        private static Dictionary<string, (bool, bool)> autorizations = new Dictionary<string, (bool, bool)>();

        /// <summary>
        /// Read local info in order to send to server.
        /// </summary>
        /// <param name="key"> local identifier file </param>
        /// <returns></returns>
        public static byte[] GetLocalInfo(string key)
        {
            if (!(autorizations.ContainsKey(key) && autorizations[key].Item1))
            {
                Debug.LogWarning("Unautorized to read this local data : " + key);
                return null;
            }

            string path = common.Path.Combine(Application.persistentDataPath, key+ ".umi3dData");
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            else
            {
                Debug.LogWarning(" local key field not found ");
               
                return null;
            }
        }

        
        /// <summary>
        /// Write local info in persistentDataPath from data sent by the server.
        /// </summary>
        /// <param name="key">local identifier file </param>
        /// <param name="bytesToWrite"></param>
        public static void SetLocalInfo(string key, byte[] bytesToWrite)
        {
            if (!(autorizations.ContainsKey(key) && autorizations[key].Item2))
            {
                Debug.LogWarning("Unautorized to write this local data : " + key);
                return;
            }
            string path = common.Path.Combine(Application.persistentDataPath, key+ ".umi3dData");
            if(!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            File.WriteAllBytes(path, bytesToWrite);

        }

        /// <summary>
        /// Check if the form contains a localInfoRequest and save the authorization in the dictionnary.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="sendLocalInfo">If true and if read access is true, Send the local info to server by http POST request.</param>
        public static void CheckFormToUpdateAuthorizations(FormDto form, bool sendLocalInfo = true)
        {
            foreach (AbstractParameterDto param in form.fields)
            {
                if (param is LocalInfoRequestParameterDto)
                {
                    //Debug.Log(param.ToJson());
                    string key = (param as LocalInfoRequestParameterDto).key;
                    if (autorizations.ContainsKey(key))
                    {
                        autorizations[key] = (param as LocalInfoRequestParameterDto).value;
                    }
                    else
                    {
                        autorizations.Add(key, (param as LocalInfoRequestParameterDto).value);
                    }

                    if (sendLocalInfo && autorizations[key].Item1)
                    {
                        var bytes = GetLocalInfo(key);
                        if (bytes != null)
                        {
                            ((HttpClient)UMI3DClientServer.Instance.GetHttpClient()).SendPostLocalInfo(
                                () => { },
                                (s) => Debug.LogWarning("fail to send local datas to server : " + s),
                                key,
                                bytes
                                );

                        }
                    }
                }
            }
        }
        
    }
}