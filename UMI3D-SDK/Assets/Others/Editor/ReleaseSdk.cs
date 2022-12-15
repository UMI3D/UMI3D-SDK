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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.cdk;
using UnityEngine.Networking;
using UnityEngine;
using System.Linq;
using System.IO;

public class ReleaseSdk
{
    public static async void _ReleaseSdk(string token,string version, string branch, List<(string path,string name)> files, string changelog)
    {
        changelog += await ComputeChangeLog(branch, version, token);
        var release = await Release(branch, version, changelog, true, false, token);
        foreach(var file in files)
            await AddFileToRelease(release, file.path, file.name, token);
    }

    static async Task AddFileToRelease(release release, string filePath, string fileName, string token)
    {
        var url = release.upload_url.Split('{')[0]+ $"?name={fileName}";
        List<(string, string)> headers = new List<(string, string)>() { ("Accept", "application/vnd.github+json"), ("Authorization", $"Bearer {token}") };
        byte[] bytes = File.ReadAllBytes(filePath);
        UnityEngine.Debug.Log(url+" "+bytes.Length);
        await PostRequest(url, bytes, WWWToDebug, headers, "application/octet-stream", true);
    }

    static async Task<release> Release(string branch, string Version, string changeLog, bool preRelease, bool draft, string token)
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
        List<(string, string)> headers = new List<(string, string)>() { ("Accept", "application/vnd.github+json"), ("Authorization", $"Bearer {token}") };
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJson(release, Newtonsoft.Json.TypeNameHandling.None));
        return await PostRequest(url, bytes, WWWToRelease, headers, "application/json", true);
    }

    static async Task<string> ComputeChangeLog(string branch, string tag, string token, string baseTag = null)
    {
        //var rs = await GetRelease();
        generate_note note = (baseTag != null) ? new generate_note_tag() { previous_tag_name = baseTag } : new generate_note();
        note.tag_name = tag;
        note.target_commitish = branch;

        return await GenerateReleaseNote(note, token);
    }

    static async Task<string> GenerateReleaseNote(generate_note noteRequest, string token)
    {
        const string url = "https://api.github.com/repos/UMI3D/UMI3D-SDK/releases/generate-notes";
        List<(string, string)> headers = new List<(string, string)>() { ("Accept", "application/vnd.github+json"), ("Authorization", $"Bearer {token}") };
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJson(noteRequest, Newtonsoft.Json.TypeNameHandling.None));
        var rs = await PostRequest(url, bytes, WWWToNote, headers, "application/json", true);
        return rs.getFullChangeLogLine();
    }

    static async Task<release[]> GetRelease(string token)
    {
        const string url = @"https://api.github.com/repos/UMI3D/UMI3D-SDK/releases";
        List<(string, string)> headers = new List<(string, string)>() { ("Accept", "application/vnd.github+json"), ("Authorization", $"Bearer {token}") };
        var rs = await GetRequest(url, WWWToReleases, headers);
        return rs;
    }

    static release[] WWWToReleases(UnityWebRequest www)
    {
        var s = WWWToString(www);
        return JsonConvert.DeserializeObject<release[]>(s, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
    }

    static release WWWToRelease(UnityWebRequest www)
    {
        var s = WWWToString(www);
        return JsonConvert.DeserializeObject<release>(s, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
    }

    static release_note WWWToNote(UnityWebRequest www)
    {
        var s = WWWToString(www);
        return JsonConvert.DeserializeObject<release_note>(s, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
    }

    static string WWWToDebug(UnityWebRequest www)
    {
        return $"code:{www.responseCode}\n{www?.downloadHandler}\n{www?.downloadHandler?.text}\n{www?.downloadHandler?.data}";
    }

    static string WWWToString(UnityWebRequest www)
    {
        return www?.downloadHandler?.text;
    }

    static byte[] WWWToBytes(UnityWebRequest www)
    {
        return www?.downloadHandler?.data;
    }

    static async Task<T> GetRequest<T>(string url, Func<UnityWebRequest, T> ComputeData, List<(string, string)> headers = null)
    {
        using (UnityWebRequest www = CreateGetRequest(url, headers))
        {
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();
            if (www.isNetworkError || www.isHttpError)
            {
                throw new Umi3dNetworkingException(www, "Request Failed\n"+www.error+"\n"+www.url);
            }
            if (ComputeData != null)
                return ComputeData(www);
            return default;
        }
    }



    static async Task<T> PostRequest<T>(string url, byte[] bytes, Func<UnityWebRequest,T> ComputeData, List<(string, string)> headers = null, string contentType = null, bool withResult = false)
    {
        using(UnityWebRequest www = CreatePostRequest(url, bytes,headers,contentType,withResult))
        {
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();
            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(System.Text.Encoding.ASCII.GetString(bytes));
                throw new Umi3dNetworkingException(www, "Request Failed\n" + www.error + "\n" + www.url);
            }
            if(ComputeData != null)
                return ComputeData(www);
            return default;
        }
    }
    private static UnityWebRequest CreatePostRequest(string url, byte[] bytes, List<(string,string)> headers = null, string contentType = null, bool withResult = false)
    {
        var requestU = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        var uH = new UploadHandlerRaw(bytes);
        if (contentType != null)
            uH.contentType = contentType;
        requestU.chunkedTransfer = false;
        requestU.uploadHandler = uH;
        if (withResult)
            requestU.downloadHandler = new DownloadHandlerBuffer();
        if(headers != null)
        {
            foreach(var h in headers)
            {
                requestU.SetRequestHeader(h.Item1, h.Item2);
            }
        }
        return requestU;
    }

    static async Task<T> PostRequest<T>(string url, WWWForm form, Func<UnityWebRequest, T> ComputeData, List<(string, string)> headers = null, string contentType = null, bool withResult = false)
    {
        using (UnityWebRequest www = CreatePostRequest(url, form, headers, withResult))
        {
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();
            if (www.isNetworkError || www.isHttpError)
            {
                throw new Umi3dNetworkingException(www, "Request Failed\n" + www.error + "\n" + www.url);
            }
            if (ComputeData != null)
                return ComputeData(www);
            return default;
        }
    }

    private static UnityWebRequest CreatePostRequest(string url, WWWForm form, List<(string, string)> headers = null, bool withResult = false)
    {
        var requestU = UnityWebRequest.Post(url, form);
        if (withResult)
            requestU.downloadHandler = new DownloadHandlerBuffer();
        if (headers != null)
        {
            foreach (var h in headers)
            {
                requestU.SetRequestHeader(h.Item1, h.Item2);
            }
        }
        return requestU;
    }

    private static UnityWebRequest CreateGetRequest(string url, List<(string, string)> headers = null)
    {
        var requestU = UnityWebRequest.Get(url);
        if (headers != null)
        {
            foreach (var h in headers)
            {
                requestU.SetRequestHeader(h.Item1, h.Item2);
            }
        }
        return requestU;
    }

    public static string ToJson(object dto, TypeNameHandling typeNameHandling = TypeNameHandling.None)
    {
        return JsonConvert.SerializeObject(dto, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = typeNameHandling
        });
    }

}

class generate_release
{
    public string tag_name { get; set; }
    public string target_commitish { get; set; }
    public string name { get; set; }
    public string body { get; set; }
    public bool draft { get; set; }
    public bool prerelease { get; set; }
    public bool generate_release_notes { get; set; }
}

class generate_note
{
    public string tag_name { get; set; }
    public string target_commitish { get; set; }
    //public string previous_tag_name { get; set; }
}

class generate_note_tag : generate_note
{
    public string previous_tag_name { get; set; }
}

class release_note
{
    public string name { get; set; }
    public string body { get; set; }

    public string getFullChangeLogLine()
    {
        return body.Split('\n').Last();
    }
}

class release_author
{
    public string login { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string avatar_url { get; set; }
    public string gravatar_id { get; set; }
    public string url { get; set; }
    public string html_url { get; set; }
    public string followers_url { get; set; }
    public string following_url { get; set; }
    public string gists_url { get; set; }
    public string starred_url { get; set; }
    public string subscriptions_url { get; set; }
    public string organizations_url { get; set; }
    public string repos_url { get; set; }
    public string events_url { get; set; }
    public string received_events_url { get; set; }
    public string type { get; set; }
    public bool site_admin { get; set; }
}
class release_asset
{
    public string url { get; set; }
    public string browser_download_url { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string name { get; set; }
    public string label { get; set; }
    public string state { get; set; }
    public string content_type { get; set; }
    public int size { get; set; }
    public int download_count { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
    public release_asset_uploader uploader { get; set; }
}

class release_asset_uploader
{
    public string login { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string avatar_url { get; set; }
    public string gravatar_id { get; set; }
    public string url { get; set; }
    public string html_url { get; set; }
    public string followers_url { get; set; }
    public string following_url { get; set; }
    public string gists_url { get; set; }
    public string starred_url { get; set; }
    public string subscriptions_url { get; set; }
    public string organizations_url { get; set; }
    public string repos_url { get; set; }
    public string events_url { get; set; }
    public string received_events_url { get; set; }
    public string type { get; set; }
    public bool site_admin { get; set; }
}

class release
{
    public string url { get; set; }
    public string html_url { get; set; }
    public string assets_url { get; set; }
    public string upload_url { get; set; }
    public string tarball_url { get; set; }
    public string zipball_url { get; set; }
    public string discussion_url { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string tag_name { get; set; }
    public string target_commitish { get; set; }
    public string name { get; set; }
    public string body { get; set; }
    public bool draft { get; set; }
    public bool prerelease { get; set; }
    public string created_at { get; set; }
    public string published_at { get; set; }
    public release_asset[] assets { get; set; }
    public release_author author { get; set; }
}