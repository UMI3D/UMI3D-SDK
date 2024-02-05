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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class Iscc
{
    const string patternOutputDir = @"OutputDir=(.*)..^OutputBaseFilename=(.*).^Compression=";

    public static async Task ExecuteISCC(string isccPath, string command, Action<string> output, Action<string> error)
    {
        command = command.Replace('/', '\\');
        isccPath = isccPath.Replace('/', '\\');
        await inetum.unityUtils.Command.ExecuteCommand($"\"{isccPath}\"", $"\"{command}\"", output, error);
    }

    public static (string path, string name) UpdateInstaller(string InstallerPath, string version, string fileNamePattern, string fileName)
    {
        string setupText = File.ReadAllText(InstallerPath);
        setupText = Regex.Replace(setupText, "#define MyAppVersion \"(.*)?\"", $"#define MyAppVersion \"{version}\"");
        setupText = Regex.Replace(setupText, "OutputBaseFilename="+fileNamePattern, $"OutputBaseFilename={fileName}");
        File.WriteAllText(InstallerPath, setupText);


        Regex DirReg = new Regex(patternOutputDir, RegexOptions.Multiline | RegexOptions.Singleline);
        var md = DirReg.Match(setupText);
        string path = md.Groups[1].Captures[0].Value + "\\" + md.Groups[2].Captures[0].Value + ".exe";
        return (path, md.Groups[2].Captures[0].Value + ".exe");
    }
}
