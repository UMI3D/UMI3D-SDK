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

using System.Collections.Generic;
using System.Threading.Tasks;
using inetum.unityUtils;

public class ReleaseSdk
{
    public static async void Release(string token,string version, string branch, List<(string path,string name)> files, string changelog)
    {
        changelog += await ComputeChangeLog(branch, version, token);
        var release = await _Release(branch, version, changelog, true, false, token);
        foreach(var file in files)
            await Github.AddFileToRelease(release, file.path, file.name, token);
    }

    static async Task<release> _Release(string branch, string Version, string changeLog, bool preRelease, bool draft, string token)
    {
        generate_release release = new generate_release() 
        {
            name = $"Sdk {Version}", 
            tag_name = Version, 
            body = changeLog,
            draft = draft, 
            generate_release_notes = false,
            prerelease = preRelease,
            target_commitish = branch 
        };

        const string url = "https://api.github.com/repos/UMI3D/UMI3D-SDK/releases";
        return await Github.Release(release, token, url);
    }

    static async Task<string> ComputeChangeLog(string branch, string tag, string token, string baseTag = null)
    {
        //var rs = await GetReleases();
        generate_note note = (baseTag != null) ? new generate_note_tag() { previous_tag_name = baseTag } : new generate_note();
        note.tag_name = tag;
        note.target_commitish = branch;

        return await GenerateReleaseNote(note, token);
    }

    static async Task<string> GenerateReleaseNote(generate_note noteRequest, string token)
    {
        const string url = "https://api.github.com/repos/UMI3D/UMI3D-SDK/releases/generate-notes";
        return (await Github.GenerateReleaseNote(noteRequest,token,url)).getFullChangeLogLine();
    }

    static async Task<release[]> GetRelease(string token)
    {
        const string url = @"https://api.github.com/repos/UMI3D/UMI3D-SDK/releases";
        return await Github.GetReleases(token, url);
    }
}
