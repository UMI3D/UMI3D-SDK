/*
Copyright 2019 Gfi Informatique

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

/// <summary>
/// Add this class to launch an environment on start.
/// Handle  
/// </summary>
public class UMI3DLauncher : MonoBehaviour
{
    const string Separator = "-";
    const string nameParam = Separator + "name";
    const string ipParam = Separator + "ip";
    const string authParam = Separator + "auth";
    const string tokenLifeParam = Separator + "tokenlife";
    const string httpPortParam = Separator + "httpPort";
    const string wsPortParam = Separator + "wsPort";
    const string fakeReliablePortParam = Separator + "fakertcreliableport";
    const string fakeUnreliablePortParam = Separator + "fakertcunreliableport";
    const string iceParam = Separator + "iceconfig";

    /// <summary>
    /// method called when param <see cref="nameParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetName(string arg) { }

    /// <summary>
    /// method called when param <see cref="ipParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetIp(string arg) { }

    /// <summary>
    /// method called when param <see cref="authParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetAuth(string arg) { }

    /// <summary>
    /// method called when param <see cref="tokenParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetTokenLife(string arg) { }

    /// <summary>
    /// method called when param <see cref="httpPortParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetHttpPort(string arg) { }

    /// <summary>
    /// method called when param <see cref="wsPortParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetWsPort(string arg) { }

    /// <summary>
    /// method called when param <see cref="fakeReliablePortParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetFakeReliable(string arg) { }

    /// <summary>
    /// method called when param <see cref="fakeUnreliablePortParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetFakeUnreliable(string arg) { }

    /// <summary>
    /// method called when param <see cref="iceParam"/> is found
    /// </summary>
    /// <param arg="arg">argument after parameter</param>
    protected virtual void SetIceServer(string arg) { }

    /// <summary>
    /// method called if a parameter wasn't catch.
    /// </summary>
    /// <param name="i">current index. 
    /// It should be set to last argument used.
    /// 
    /// </param>
    /// <param name="args"></param>
    protected virtual void OtherParam(ref int i, string[] args) { }


    // Start is called before the first frame update
    protected virtual void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int length = args.Length;
        for (int i = 0; i < length; i++)
        {
            if (args[i].CompareTo(nameParam) == 0)
            {
                if (++i < length)
                    SetName(args[i]);
            }
            else if (args[i].CompareTo(ipParam) == 0)
            {
                if (++i < length)
                    SetIp(args[i]);
            }
            else if (args[i].CompareTo(authParam) == 0)
            {
                if (++i < length)
                    SetAuth(args[i]);
            }
            else if (args[i].CompareTo(tokenLifeParam) == 0)
            {
                if (++i < length)
                    SetTokenLife(args[i]);
            }
            else if (args[i].CompareTo(httpPortParam) == 0)
            {
                if (++i < length)
                    SetHttpPort(args[i]);
            }
            else if (args[i].CompareTo(wsPortParam) == 0)
            {
                if (++i < length)
                    SetWsPort(args[i]);
            }
            else if (args[i].CompareTo(fakeReliablePortParam) == 0)
            {
                if (++i < length)
                    SetFakeReliable(args[i]);
            }
            else if (args[i].CompareTo(fakeUnreliablePortParam) == 0)
            {
                if (++i < length)
                    SetFakeUnreliable(args[i]);
            }
            else if (args[i].CompareTo(iceParam) == 0)
            {
                if (++i < length)
                    SetIceServer(args[i]);
            }
            else
                OtherParam(ref i, args);
        }
    }
}
