/*
Copyright 2019 - 2022 Inetum

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
using System.Diagnostics;
using System.Threading.Tasks;

class Command
{
    public static async Task ExecuteCommand(string command, string args, Action<string> output, Action<string> error)
    {
        var processInfo = new ProcessStartInfo(command, args)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        var process = Process.Start(processInfo);

        if (output != null)
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => output(e.Data);
        else
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            UnityEngine.Debug.Log("Information while executing command { <i>" + command + " " + args + "</i> } : <b>D>" + e.Data + "</b>");

        process.BeginOutputReadLine();

        if (error != null)
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => error(e.Data);
        else
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            UnityEngine.Debug.Log("Error while executing command { <i>" + command + " " + args + "</i> } : <b>E>" + e.Data + "</b>");

        process.BeginErrorReadLine();

        while (!process.HasExited)
        {
            await Task.Yield();
        }

        process.Close();
    }
}