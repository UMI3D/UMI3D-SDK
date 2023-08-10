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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// Helper that contains functions for <see cref="Task"/> with further security related to the application lifecycle. 
/// </summary>
/// Throw excepton if a task is launched while the application is not running or is stopping.
public static class UMI3DAsyncManager
{
    /// <summary>
    /// Similar to <see cref="Task.Yield()"/> with security if the app is quitted in the meanwhile.
    /// </summary>
    /// <returns></returns>
    public static async Task Yield(List<CancellationToken> tokens = null)
    {
        ErrorIfQuitting(tokens);
        await Task.Yield();
    }

    /// <summary>
    /// Similar to <see cref="Task.Delay(int)"/> with security if the app is quitted in the meanwhile.
    /// </summary>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static async Task Delay(int milliseconds, List<CancellationToken> tokens = null)
    {
        ErrorIfQuitting(tokens);
        await Task.Delay(milliseconds);
        ErrorIfQuitting(tokens);

    }

    public static void TestTokens(this List<CancellationToken> tokens)
    {
        if (tokens != null)
            foreach (var token in tokens)
                token.ThrowIfCancellationRequested();
    }

    private static void ErrorIfQuitting(List<CancellationToken> tokens)
    {
        if (QuittingManager.ApplicationIsQuitting)
            throw new UMI3DAsyncManagerException("Application is quitting");
#if UNITY_EDITOR
        try
        {
            if (!Application.isPlaying)
            {
                throw new UMI3DAsyncManagerException("Application is not playing");
            }
        }
        catch(UnityException e)
        {
            UnityEngine.Debug.LogException(e);
        }
#endif
        TestTokens(tokens);
    }
}
public class UMI3DAsyncManagerException : System.Exception
{
    public UMI3DAsyncManagerException(string message) : base(message)
    {
    }
}

