/*
Copyright 2019 - 2023 Inetum

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
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine.Networking;
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;



    public class Github
    {
        static async public Task<release[]> GetReleases(string token, string url)
        {
            List<(string, string)> headers = new List<(string, string)>() { ("Accept", "application/vnd.github+json"), ("Authorization", $"Bearer {token}") };
            var rs = await GetRequest(url, WWWToReleases, headers);
            return rs;
        }

        static async public Task<release_note> GenerateReleaseNote(generate_note noteRequest, string token, string url)
        {
            List<(string, string)> headers = new List<(string, string)>() { ("Accept", "application/vnd.github+json"), ("Authorization", $"Bearer {token}") };
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJson(noteRequest, Newtonsoft.Json.TypeNameHandling.None));
            return await PostRequest(url, bytes, WWWToNote, headers, "application/json", true);
        }

        static public async Task<release> Release(generate_release release, string token, string url)
        {
            List<(string, string)> headers = new List<(string, string)>() { ("Accept", "application/vnd.github+json"), ("Authorization", $"Bearer {token}") };
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJson(release, Newtonsoft.Json.TypeNameHandling.None));
            return await PostRequest(url, bytes, WWWToRelease, headers, "application/json", true);
        }

        static public async Task<string> AddFileToRelease(release release, string filePath, string fileName, string token)
        {
            var url = release.upload_url.Split('{')[0] + $"?name={fileName}";
            List<(string, string)> headers = new List<(string, string)>() { ("Accept", "application/vnd.github+json"), ("Authorization", $"Bearer {token}") };
            byte[] bytes = File.ReadAllBytes(filePath);
            return await PostRequest(url, bytes, WWWToDebug, headers, "application/octet-stream", true);
        }

        #region WWW methods
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
                    throw new Exception("Request Failed\n" + www.error + "\n" + www.url);
                }
                if (ComputeData != null)
                    return ComputeData(www);
                return default;
            }
        }

        static async Task<T> PostRequest<T>(string url, byte[] bytes, Func<UnityWebRequest, T> ComputeData, List<(string, string)> headers = null, string contentType = null, bool withResult = false)
        {
            using (UnityWebRequest www = CreatePostRequest(url, bytes, headers, contentType, withResult))
            {
                UnityWebRequestAsyncOperation operation = www.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();
                if (www.isNetworkError || www.isHttpError)
                {
                    UnityEngine.Debug.Log(System.Text.Encoding.ASCII.GetString(bytes));
                    throw new Exception("Request Failed\n" + www.error + "\n" + www.url);
                }
                if (ComputeData != null)
                    return ComputeData(www);
                return default;
            }
        }
        private static UnityWebRequest CreatePostRequest(string url, byte[] bytes, List<(string, string)> headers = null, string contentType = null, bool withResult = false)
        {
            var requestU = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            var uH = new UploadHandlerRaw(bytes);
            if (contentType != null)
                uH.contentType = contentType;
            requestU.chunkedTransfer = false;
            requestU.uploadHandler = uH;
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

        static async Task<T> PostRequest<T>(string url, WWWForm form, Func<UnityWebRequest, T> ComputeData, List<(string, string)> headers = null, string contentType = null, bool withResult = false)
        {
            using (UnityWebRequest www = CreatePostRequest(url, form, headers, withResult))
            {
                UnityWebRequestAsyncOperation operation = www.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();
                if (www.isNetworkError || www.isHttpError)
                {
                    throw new Exception("Request Failed\n" + www.error + "\n" + www.url);
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
        #endregion
    }

    #region Github Data
    public class generate_release
    {
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
        public bool generate_release_notes { get; set; }
    }

    public class generate_note
    {
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        //public string previous_tag_name { get; set; }
    }

    public class generate_note_tag : generate_note
    {
        public string previous_tag_name { get; set; }
    }

    public class release_note
    {
        public string name { get; set; }
        public string body { get; set; }

        public string getFullChangeLogLine()
        {
            return body.Split('\n').Last();
        }
    }

    public class release_author
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
    public class release_asset
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

    public class release_asset_uploader
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

    public class release
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
    #endregion
}