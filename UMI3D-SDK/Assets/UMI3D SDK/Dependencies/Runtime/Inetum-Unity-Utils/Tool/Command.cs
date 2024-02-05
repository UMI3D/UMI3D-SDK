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
namespace inetum.unityUtils
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class Command
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


        public static void OpenFile(string path)
        {
            path = System.IO.Path.GetFullPath(path);
            path = path.Replace('/', '\\');

            if (File.Exists(path))
                OpenFileWith("explorer.exe", path, "/select,");
            else if (Directory.Exists(path))
                OpenFileWith("explorer.exe", path, "/root,");
            else
                UnityEngine.Debug.LogError("no file at " + path);
        }

        static void OpenFileWith(string exePath, string path, string arguments)
        {
            if (path == null)
                return;

            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
                if (exePath != null)
                {
                    process.StartInfo.FileName = exePath;
                    //Pre-post insert quotes for fileNames with spaces.
                    process.StartInfo.Arguments = string.Format("{0}\"{1}\"", arguments, path);
                }
                else
                {
                    process.StartInfo.FileName = path;
                    process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
                }
                if (!path.Equals(process.StartInfo.WorkingDirectory))
                {
                    process.Start();
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }
    }
}