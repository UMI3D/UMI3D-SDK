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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using WebSocketSharp;
using Path = inetum.unityUtils.Path;

namespace umi3d.cdk
{
    public class UMI3DResourcesManager : inetum.unityUtils.PersistentSingleBehaviour<UMI3DResourcesManager>
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        #region const
        private const string dataFile = "data.json";
        private const string assetDirectory = "asset";
        #endregion
        #region data

        /// <summary>
        /// Discribe a library and its content.
        /// </summary>
        [Serializable]
        public class DataFile
        {
            public List<Data> files;
            public string path;
            public string key;
            public string date;
            public string culture;
            public string dateformat;
            public List<string> applications = new List<string>();


            public DataFile(string key, string path, List<string> applications, string date, string format, string culture)
            {
                this.path = path;
                this.key = key;
                this.date = date;
                this.dateformat = format;
                this.culture = culture;
                files = new List<Data>();
                this.applications = applications;
            }
        }
        /// <summary>
        /// Discribe a file in a library.
        /// </summary>
        [Serializable]
        public class Data
        {
            public string url;
            public string path;
            public string fileRelativePath;

            public Data(string url, string path, string fileRelativePath)
            {
                this.url = url;
                this.path = path;
                this.fileRelativePath = fileRelativePath;
            }
        }

        /// <summary>
        /// Write a DataFile in a directory.
        /// </summary>
        /// <param name="data">DataFile to write.</param>
        /// <param name="directory">Directory to write the file into.</param>
        private void SetData(DataFile data, string directory)
        {
            string path = Path.Combine(directory, dataFile);
            FileStream file;

            if (File.Exists(path)) file = File.OpenWrite(path);
            else file = File.Create(path);

            var bf = new BinaryFormatter();
            bf.Serialize(file, data);
            file.Close();
        }

        /// <summary>
        /// Read a DataFile in a directory.
        /// </summary>
        /// <param name="directory">Directory to read the file in.</param>
        /// <returns>A DataFile if the directory containe one, null otherwhise.</returns>
        private DataFile GetData(string directory)
        {
            if (Directory.Exists(directory))
            {
                string path = Path.Combine(directory, dataFile);
                if (File.Exists(path))
                {

                    FileStream file;
                    file = File.OpenRead(path);
                    var bf = new BinaryFormatter();
                    try
                    {
                        var data = (DataFile)bf.Deserialize(file);
                        file.Close();
                        return data;
                    }
                    catch
                    {
                        file.Close();
                    }
                }
            }
            return null;
        }
        #endregion
        #region Parameters
        /// <summary>
        /// An object to store all information about an object.
        /// </summary>
        private class ObjectData
        {
            public object value;
            /// <summary>
            /// entityId of all object using this object.
            /// </summary>
            public HashSet<ulong> entityIds;
            public HashSet<string> libraryIds;

            /// <summary
            /// Loading state.
            /// </summary>
            public Estate state;
            /// <summary>
            /// url of the object.
            /// </summary>
            public string url;
            public string fileRelativePath;

            /// <summary>
            /// field containing authorization string.
            /// </summary>
            public string authorization
            {
                get
                {
                    if (useServerAuthorization)
                        return UMI3DClientServer.getAuthorization();
                    else return _authorization;
                }
                set => _authorization = value;
            }

            private string _authorization;
            private bool useServerAuthorization = false;

            /// <summary>
            /// url of the object.
            /// </summary>
            public string extension;

            /// <summary>
            /// Local path of the object file (if downloaded).
            /// </summary>
            public string downloadedPath;

            /// <summary>
            /// Callback to call when the object finish loading.
            /// </summary>
            public List<Action<object>> loadCallback;
            /// <summary>
            /// Callback to call when the object finish loading with an error.
            /// </summary>
            public List<Action<Umi3dException>> loadFailCallback;
            /// <summary>
            /// Action to invoke when the object need to be deleted.
            /// </summary>
            public Action<object, string> DeleteAction;

            public enum Estate
            {
                NotLoaded, Loading, Loaded
            }

            public static Regex rx = new Regex(@"^https?://(.+?)(:\d+)*/(.*)$");
            private readonly Match a;// = rx.Match(url);

            /// <summary>
            /// Match if a url is valid for this object.
            /// Ignore difference between http and https and the port used.
            /// </summary>
            /// <param name="url">Url to match.</param>
            /// <returns></returns>
            public bool MatchUrl(Match Matchurl, string url, string libraryId = null)
            {
                if (url == this.url)
                    return true;

                if (a.Success && Matchurl.Success)
                    return (a.Groups[1].Captures[0].Value == Matchurl.Groups[1].Captures[0].Value
                                && (a.Groups[2].Captures.Count == Matchurl.Groups[2].Captures.Count)
                                && (a.Groups[2].Captures.Count == 0 || a.Groups[2].Captures[0].Value == Matchurl.Groups[2].Captures[0].Value)
                                && a.Groups[3].Captures[0].Value == Matchurl.Groups[3].Captures[0].Value)
                        || (!string.IsNullOrEmpty(fileRelativePath)
                                && !string.IsNullOrEmpty(libraryId)
                                && libraryIds.Contains(libraryId)
                                && Matchurl.Groups[3].Captures[0].Value.Contains(fileRelativePath));

                return false;
            }

            private bool MatchServerUrl()
            {
                if (UMI3DClientServer.Environement == null)
                    return false;

                string url = UMI3DClientServer.Environement.resourcesUrl + '/';

                if (url == this.url) return true;

                Match b = rx.Match(url);
                if (a.Success && b.Success)
                    return a.Groups[1].Captures[0].Value == b.Groups[1].Captures[0].Value && (a.Groups[2].Captures.Count == b.Groups[2].Captures.Count) && (a.Groups[2].Captures.Count == 0 || a.Groups[2].Captures[0].Value == b.Groups[2].Captures[0].Value);
                return false;
            }

            private string ComputeAuthorization(string authorization)
            {
                if (MatchServerUrl())
                {
                    useServerAuthorization = true;
                    return UMI3DClientServer.getAuthorization();
                }
                useServerAuthorization = false;
                if (authorization.IsNullOrEmpty()) return null;
                return "Basic" + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(authorization));
            }


            public ObjectData(string url, object value, HashSet<ulong> entityId)
            {
                this.value = value;
                entityIds = entityId;
                libraryIds = new HashSet<string>();
                loadCallback = new List<Action<object>>();
                loadFailCallback = new List<Action<Umi3dException>>();
                state = Estate.Loaded;
                downloadedPath = null;
                this.url = url;
                a = rx.Match(url);
            }

            public ObjectData(string url, object value, ulong entityId)
            {
                this.value = value;
                entityIds = new HashSet<ulong>() { entityId };
                libraryIds = new HashSet<string>();
                loadCallback = new List<Action<object>>();
                loadFailCallback = new List<Action<Umi3dException>>();
                state = Estate.Loaded;
                downloadedPath = null;
                this.url = url;
                a = rx.Match(url);
            }

            public ObjectData(string url, string extension, string authorization, HashSet<ulong> entityId, List<Action<object>> loadCallback, List<Action<Umi3dException>> loadFailCallback)
            {
                value = null;
                entityIds = entityId;
                libraryIds = new HashSet<string>();
                this.loadCallback = loadCallback;
                this.loadFailCallback = loadFailCallback;
                state = Estate.NotLoaded;
                downloadedPath = null;
                this.url = url;
                this.extension = extension;
                a = rx.Match(url);
                this.authorization = ComputeAuthorization(authorization);
            }

            public ObjectData(string url, string extension, string authorization, ulong entityId, Action<object> loadCallback, Action<Umi3dException> loadFailCallback)
            {
                value = null;
                entityIds = new HashSet<ulong>() { entityId };
                libraryIds = new HashSet<string>();
                this.loadCallback = new List<Action<object>>() { loadCallback };
                this.loadFailCallback = new List<Action<Umi3dException>>() { loadFailCallback };
                state = Estate.NotLoaded;
                downloadedPath = null;
                this.url = url;
                this.extension = extension;
                a = rx.Match(url);
                this.authorization = ComputeAuthorization(authorization);
            }

            public ObjectData(string url, string extension, string authorization, ulong entityId)
            {
                value = null;
                entityIds = new HashSet<ulong>() { entityId };
                libraryIds = new HashSet<string>();
                loadCallback = new List<Action<object>>();
                loadFailCallback = new List<Action<Umi3dException>>();
                state = Estate.NotLoaded;
                downloadedPath = null;
                this.url = url;
                this.extension = extension;
                a = rx.Match(url);
                this.authorization = ComputeAuthorization(authorization);
            }

            public ObjectData(string url, string extension, string authorization, string libraryId, string downloadedPath, string fileRelativePath)
            {
                value = null;
                entityIds = new HashSet<ulong>();
                libraryIds = new HashSet<string>() { libraryId };
                loadCallback = new List<Action<object>>();
                loadFailCallback = new List<Action<Umi3dException>>();
                state = Estate.NotLoaded;
                this.downloadedPath = downloadedPath;
                this.url = url;
                this.extension = extension;
                this.authorization = authorization;
                a = rx.Match(url);
                this.fileRelativePath = fileRelativePath;
            }

            public ObjectData(string url, string extension, string authorization, ulong entityId, string downloadedPath)
            {
                value = null;
                entityIds = new HashSet<ulong>() { entityId };
                libraryIds = new HashSet<string>();
                loadCallback = new List<Action<object>>();
                loadFailCallback = new List<Action<Umi3dException>>();
                state = Estate.NotLoaded;
                this.downloadedPath = downloadedPath;
                this.url = url;
                this.extension = extension;
                this.authorization = authorization;
                a = rx.Match(url);
            }
        }

        public Dictionary<ulong, string> librariesMap = new Dictionary<ulong, string>();
        private List<ObjectData> CacheCollection;
        private Dictionary<string, KeyValuePair<DataFile, HashSet<ulong>>> libraries;
        public static List<DataFile> Libraries => Exists ? Instance.libraries.Values.Select(k => k.Key).ToList() : new List<DataFile>();

        #endregion
        #region setup

        private ThreadDeserializer deserializer;

        ///<inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            ClearCache();
            deserializer = new ThreadDeserializer();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!Exists)
                deserializer?.Stop();
        }

        public static bool ClearCache(string VariantUrl, string LibraryId = null)
        {
            Match matchUrl = ObjectData.rx.Match(VariantUrl);
            return VariantUrl != null && Exists && Instance.ClearCache(ob => ob.MatchUrl(matchUrl, VariantUrl, LibraryId));
        }

        private bool ClearCache(Func<ObjectData, bool> predicate)
        {
            if (CacheCollection != null)
            {
                ObjectData ObjectValue = CacheCollection.FirstOrDefault(predicate);
                if (ObjectValue == null) return false;
                if (ObjectValue.state == ObjectData.Estate.Loading && ObjectValue.loadFailCallback != null)
                {
                    foreach (Action<Umi3dException> failback in ObjectValue.loadFailCallback)
                    {
                        failback.Invoke(new Umi3dException("clear requested"));
                    }
                }

                if (subModelsCache != null && subModelsCache.ContainsKey(ObjectValue.url))
                {
                    subModelsCache[ObjectValue.url].Destroy();

                    subModelsCache.Remove(ObjectValue.url);
                }

                ObjectValue.DeleteAction?.Invoke(ObjectValue.value, "clear requested");
                CacheCollection.Remove(ObjectValue);
                return true;
            }
            return false;
        }

        public void ClearCache()
        {
            if (CacheCollection != null)
            {
                foreach (ObjectData ObjectValue in CacheCollection)
                {
                    if (ObjectValue.state == ObjectData.Estate.Loading && ObjectValue.loadFailCallback != null)
                    {
                        foreach (Action<Umi3dException> failback in ObjectValue.loadFailCallback)
                        {
                            failback.Invoke(new Umi3dException("clear all cache"));
                        }
                    }

                    ObjectValue.DeleteAction?.Invoke(ObjectValue.value, "clear all cache");
                }
            }
            if (subModelsCache != null)
            {
                foreach (SubmodelDataCollection item in subModelsCache.Values)
                {
                    item.Destroy();
                }
                subModelsCache.Clear();
            }
            else
                subModelsCache = new Dictionary<string, SubmodelDataCollection>();
            CacheCollection = new List<ObjectData>();
            StopAllCoroutines();
            libraries = new Dictionary<string, KeyValuePair<DataFile, HashSet<ulong>>>();
            LoadLocalLib();
        }

        private void HardReset()
        {
            string path = Application.persistentDataPath;
            foreach (string directory in Directory.GetDirectories(path).ToList())
            {
                Directory.Delete(directory, true);
            }
            ClearCache();
        }

        private void LoadLocalLib()
        {
            string path = Application.persistentDataPath;
            foreach (string directory in Directory.GetDirectories(path).ToList())
            {
                DataFile data = GetData(directory);
                if (data != null && data.path != null && data.key != null && data.files != null)
                {
                    foreach (Data file in data.files)
                    {
                        Match matchUrl = ObjectData.rx.Match(file.url);
                        ObjectData objectData = CacheCollection.Find((o) =>
                        {
                            return o.MatchUrl(matchUrl, file.url, data.key);
                        });
                        if (objectData != null)
                            objectData.downloadedPath = file.path;
                        else
                            CacheCollection.Insert(0, new ObjectData(file.url, null, null, data.key, file.path, file.fileRelativePath));
                    }
                    libraries.Add(data.key, new KeyValuePair<DataFile, HashSet<ulong>>(data, new HashSet<ulong>()));
                }
                //else
                //    Directory.Delete(directory, true);
            }
        }
        #endregion
        #region library Load
        /// <summary>
        /// Load a library.
        /// </summary>
        /// <param name="libraryId">id of the library to load.</param>
        /// <param name="finished">finished callback.</param>
        /// <param name="SceneId">id of the scene which use this library</param>
        public static void LoadLibrary(string libraryId, Action finished, ulong SceneId = 0)
        {
            StartCoroutine(_LoadLibrary(libraryId, finished, SceneId));
        }

        public static IEnumerator _LoadLibrary(string libraryId, Action finished, ulong SceneId)
        {
            int count = 0;
            KeyValuePair<DataFile, HashSet<ulong>> lib = Instance.libraries.Where((p) => { return p.Key == libraryId; }).Select((p) => { return p.Value; }).FirstOrDefault();
            if (lib.Key != null && SceneId != 0)
                lib.Value.Add(SceneId);

            IEnumerable<ObjectData> downloaded = Instance.CacheCollection.Where((od) => { return od.state == ObjectData.Estate.NotLoaded && od.libraryIds.Contains(libraryId); });
            foreach (ObjectData pair in downloaded)
            {

                string extension = System.IO.Path.GetExtension(pair.url);
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(extension);
                if (loader != null)
                {
                    count++;
                    LoadFile(pair.entityIds.First(), pair, loader.UrlToObject, loader.ObjectFromCache, (obj) => { count--; }, (error) => { UMI3DLogger.LogError(error, scope); count--; }, loader.DeleteObject);
                }
            }
            yield return new WaitUntil(() => { return count <= 0; });
            finished?.Invoke();
        }

        /// <summary>
        /// LOad a collection of libraries.
        /// </summary>
        /// <param name="ids">libraries id to load</param>
        /// <param name="loadedResources">call each time a library have been loaded with the count of all loaded libraries in parameter.</param>
        /// <param name="resourcesToLoad">call with the total count of libraries to load in parameter.</param>
        /// <returns></returns>
        public static IEnumerator LoadLibraries(List<string> ids, Action<int> loadedResources, Action<int> resourcesToLoad)
        {
            int count = 0;
            IEnumerable<ObjectData> downloaded = Instance.CacheCollection.Where((p) => { return p.downloadedPath != null && p.state == ObjectData.Estate.NotLoaded && p.libraryIds.Any(i => ids.Contains(i)); });
            int total = downloaded.Count();
            resourcesToLoad.Invoke(total);
            loadedResources.Invoke(0);
            foreach (ObjectData pair in downloaded)
            {
                string extension = System.IO.Path.GetExtension(pair.url);
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(extension);
                if (loader != null)
                {
                    count++;
                    ulong? id = pair.entityIds?.FirstOrDefault();
                    if (id == null)
                    {
                        string libId = pair.libraryIds?.FirstOrDefault();
                        if (libId != null && Instance.librariesMap.ContainsValue(libId))
                        {
                            id = Instance.librariesMap.FirstOrDefault(l => l.Value == libId).Key;
                        }
                    }
                    if (id == null)
                        throw new Exception("id should never be null");
                    LoadFile(
                        id ?? 0,
                        pair,
                        loader.UrlToObject,
                        loader.ObjectFromCache,
                        (obj) => { count--; loadedResources.Invoke(total - count); },
                        (error) => { UMI3DLogger.LogError($"{error}[{pair.url}]", scope); count--; },
                        loader.DeleteObject);
                }
            }
            yield return new WaitUntil(() => { return count <= 0; });
        }
        #endregion
        #region file Load

        /// <summary>
        /// Returns true if <paramref name="url"/> has parameters.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool HasUrlGotParameters(string url)
        {
            return Regex.IsMatch(url, ".*\\?((.*=.*)(&?))+");
        }

        /// <summary>
        /// Returns an url with authorization set with parameters
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public string SetAuthorisationWithParameter(string fileUrl, string authorization)
        {
            if (HasUrlGotParameters(fileUrl))
                fileUrl += "&" + UMI3DNetworkingKeys.ResourceServerAuthorization + "=" + authorization;
            else
                fileUrl += "?" + UMI3DNetworkingKeys.ResourceServerAuthorization + "=" + authorization;

            return fileUrl;
        }

        public static void LoadFile(ulong id, FileDto file, Action<string, string, string, Action<object>, Action<Umi3dException>, string> urlToObject, Action<object, Action<object>, string> objectFromCache, Action<object> callback, Action<Umi3dException> failCallback, Action<object, string> deleteAction)
        {
            Instance._LoadFile(id, file, urlToObject, objectFromCache, callback, failCallback, deleteAction);
        }

        private static void LoadFile(ulong id, ObjectData file, Action<string, string, string, Action<object>, Action<Umi3dException>, string> urlToObject, Action<object, Action<object>, string> objectFromCache, Action<object> callback, Action<Umi3dException> failCallback, Action<object, string> deleteAction)
        {
            Instance._LoadFile(id, file, urlToObject, objectFromCache, callback, failCallback, deleteAction);
        }

        private void _LoadFile(ulong id, ObjectData objectData, Action<string, string, string, Action<object>, Action<Umi3dException>, string> urlToObject, Action<object, Action<object>, string> objectFromCache, Action<object> callback, Action<Umi3dException> failCallback, Action<object, string> deleteAction, string PathIfInBundle = null)
        {
            bool shouldLoad = true;

            if (objectData == null) return;
            objectData.entityIds.Add(id);

            if (objectData.state == ObjectData.Estate.Loaded)
            {
                //callback.Invoke(objectData.value);
                // replace
                objectFromCache(objectData.value, callback, PathIfInBundle);

            }
            else
            {
                objectData.loadCallback.Add((o) => { objectFromCache(o, callback, PathIfInBundle); });
                objectData.loadFailCallback.Add(failCallback);
            }
            shouldLoad = objectData.state == ObjectData.Estate.NotLoaded;

            if (objectData.DeleteAction == null)
            {
                objectData.DeleteAction = deleteAction;
            }

            if (shouldLoad)
            {
                Action<string> sucess = (path) =>
                {
                    Action<object> sucess2 = (obj) =>
                    {
                        objectData.value = obj;
                        objectData.state = ObjectData.Estate.Loaded;
                        foreach (Action<object> back in objectData.loadCallback)
                            back.Invoke(obj);
                        objectData.loadCallback.Clear();
                    };
                    Action<Umi3dException> error2 = (reason) =>
                    {
                        foreach (Action<Umi3dException> back in objectData.loadFailCallback)
                            back.Invoke(reason);
                        objectData.loadFailCallback.Clear();
                        objectData.state = ObjectData.Estate.NotLoaded;
                    };
                    StartCoroutine(UrlToObjectWithPolicy(sucess2, error2, path, objectData.extension, objectData, null, urlToObject));
                };

                Action<Umi3dException> error = (reason) =>
                {
                    //UMI3DLogger.LogWarning($"error {reason}");
                    foreach (Action<Umi3dException> back in objectData.loadFailCallback)
                        back.Invoke(reason);
                    objectData.loadFailCallback.Clear();
                    objectData.state = ObjectData.Estate.NotLoaded;
                };
                objectData.state = ObjectData.Estate.Loading;
                GetFilePath(objectData.url, sucess, error, objectData.libraryIds.FirstOrDefault());
            }
        }

        private IEnumerator UrlToObjectWithPolicy(Action<object> succes, Action<Umi3dException> error, string path, string extension, ObjectData objectData, string bundlePath, Action<string, string, string, Action<object>, Action<Umi3dException>, string> urlToObject, Func<RequestFailedArgument, bool> ShouldTryAgain = null, int tryCount = 0)
        {
            if (ShouldTryAgain == null)
                ShouldTryAgain = DefaultShouldTryAgain;
            if (tryCount > 0)
                yield return null;
            DateTime date = DateTime.UtcNow;
            Action<Umi3dException> error2 = (reason) =>
            {
                async void retry()
                {
                    if (await UMI3DClientServer.Instance.TryAgainOnHttpFail(
                         new RequestFailedArgument(
                             reason.errorCode,
                             tryCount,
                             date,
                             ShouldTryAgain
                             )))
                    {
                        StartCoroutine(UrlToObjectWithPolicy(succes, error, path, extension, objectData, bundlePath, urlToObject, ShouldTryAgain, tryCount + 1));
                    }
                    else
                    {
                        error?.Invoke(reason);
                    }
                }

                retry();
            };

            urlToObject.Invoke(path, extension, objectData.authorization, succes, error2, bundlePath);
            yield break;
        }

        private void _LoadFile(ulong id, FileDto file, Action<string, string, string, Action<object>, Action<Umi3dException>, string> urlToObject, Action<object, Action<object>, string> objectFromCache, Action<object> callback, Action<Umi3dException> failCallback, Action<object, string> deleteAction)
        {
            string fileName = System.IO.Path.GetFileName(file.url);

            Match matchUrl = ObjectData.rx.Match(file.url);
            ObjectData objectData = CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, file.url, file.libraryKey);
            });

            if (objectData == null)
            {
                objectData = new ObjectData(file.url, file.extension, file.authorization, id);
                CacheCollection.Insert(0, objectData);
            }
            _LoadFile(id, objectData, urlToObject, objectFromCache, callback, failCallback, deleteAction, file.pathIfInBundle);
        }

        private void GetFilePath(string url, Action<string> callback, Action<Umi3dException> error, string libraryKey = null)
        {
            Match matchUrl = ObjectData.rx.Match(url);

            ObjectData objectData = CacheCollection.Find((o) =>
            {

                return o.MatchUrl(matchUrl, url, libraryKey);
            });

            if (objectData != null && objectData.downloadedPath != null)
            {
                callback.Invoke(objectData.downloadedPath);
            }
            else
            {
                callback.Invoke(url);
            }
        }

        public static void GetFile(string url, Action<byte[]> callback, Action<string> error, string libraryKey = null)
        {
            //ObjectData objectData = Instance.CacheCollection.Find((o) => { return o.MatchUrl(url, libraryKey); });
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = Instance.CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, url, libraryKey);
            });
            if (objectData != null && objectData.downloadedPath != null)
            {
                callback.Invoke(File.ReadAllBytes(objectData.downloadedPath));
            }
            else
            {
                Instance.DownloadFile(url, callback, error);
            }
        }
        #endregion
        #region libraries download

        public class ProgressListener : UnityEvent<float> { }
        public ProgressListener onProgressChange = new ProgressListener();
        public UnityEvent onLibrariesDownloaded = new UnityEvent();
        private float librariesToDownload = 0;
        private float librariesDownloaded = 0;

        public static List<string> LibrariesToDownload(LibrariesDto libraries)
        {
            return Instance._LibrariesToDownload(libraries.libraries);
        }

        public List<string> _LibrariesToDownload(List<AssetLibraryDto> assetLibraries)
        {
            var toDownload = new List<string>();
            if (assetLibraries != null && assetLibraries.Count > 0)
            {
                foreach (AssetLibraryDto assetLibrary in assetLibraries)
                {
                    try
                    {
                        if (libraries.ContainsKey(assetLibrary.libraryId))
                        {
                            DataFile dt = libraries[assetLibrary.libraryId].Key;
                            var info = new CultureInfo(assetLibrary.culture);
                            var dtInfo = new CultureInfo(dt.culture);
                            if (DateTime.TryParseExact(dt.date, dt.dateformat, dtInfo, DateTimeStyles.None, out DateTime local) && DateTime.TryParseExact(assetLibrary.date, assetLibrary.format, info, DateTimeStyles.None, out DateTime server))
                            {
                                if (local.Ticks >= server.Ticks)
                                    continue;
                            }
                        }
                    }
                    catch { };
                    toDownload.Add(assetLibrary.libraryId);
                }
            }
            return toDownload;
        }

        public static void DownloadLibraries(LibrariesDto libraries, string applicationName, Action callback, Action<string> error)
        {
            StartCoroutine(Instance.DownloadResources(libraries.libraries, applicationName, callback, error));
        }

        private IEnumerator DownloadResources(List<AssetLibraryDto> assetlibraries, string applicationName, Action callback, Action<string> error)
        {
            if (assetlibraries != null && assetlibraries.Count > 0)
            {
                librariesToDownload = assetlibraries.Count;
                librariesDownloaded = 0;
                onProgressChange.Invoke(0f);
                foreach (AssetLibraryDto assetlibrary in assetlibraries)
                {
                    yield return StartCoroutine(DownloadResources(assetlibrary, applicationName));
                    librariesDownloaded += 1;
                    onProgressChange.Invoke(librariesDownloaded / librariesToDownload);
                }
            }
            onProgressChange.Invoke(1f);
            yield return new WaitForSeconds(0.3f);
            onLibrariesDownloaded.Invoke();
            callback.Invoke();
        }


        public static void DownloadLibrary(AssetLibraryDto library, string application, Action callback)
        {
            StartCoroutine(Instance._DownloadLibrary(library, application, callback));
        }

        private IEnumerator _DownloadLibrary(AssetLibraryDto library, string application, Action callback)
        {
            yield return StartCoroutine(DownloadResources(library, application));
            callback.Invoke();
        }

        private IEnumerator DownloadResources(AssetLibraryDto assetLibrary, string application)
        {
            var applications = new List<string>() { application };
            librariesMap[assetLibrary.id] = assetLibrary.libraryId;
            string directoryPath = Path.Combine(Application.persistentDataPath, assetLibrary.libraryId);
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    DataFile dt = Instance.libraries[assetLibrary.libraryId].Key;
                    var info = new CultureInfo(assetLibrary.culture);
                    var dtInfo = new CultureInfo(dt.culture);
                    if (DateTime.TryParseExact(dt.date, dt.dateformat, dtInfo, DateTimeStyles.None, out DateTime local) && DateTime.TryParseExact(assetLibrary.date, assetLibrary.format, info, DateTimeStyles.None, out DateTime server))
                    {

                        if (dt.applications == null)
                            dt.applications = new List<string>();
                        if (local.Ticks >= server.Ticks)
                        {
                            if (!dt.applications.Contains(application))
                            {
                                dt.applications.Add(application);
                                SetData(dt, directoryPath);
                            }
                            yield break;
                        }
                        applications = dt.applications;
                        if (!applications.Contains(application))
                            applications.Add(application);
                    }
                }
                catch { }
                RemoveLibrary(assetLibrary.libraryId);
            }

            bool finished = false;
            Action<byte[]> action = (bytes) =>
            {
                deserializer.FromBson(bytes,
                    (dto) =>
                    {
                        string assetDirectoryPath = Path.Combine(directoryPath, assetDirectory);
                        if (dto is FileListDto)
                            StartCoroutine(DownloadFiles(assetLibrary.libraryId, directoryPath, assetDirectoryPath, applications, assetLibrary.date, assetLibrary.format, assetLibrary.culture, dto as FileListDto, (data) => { if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath); SetData(data, directoryPath); finished = true; }));
                        else
                            finished = true;
                    });


            };
            Action<string> error = (s) =>
            {
                UMI3DLogger.LogError(s, scope);
                finished = true;
            };
            UMI3DLocalAssetDirectory variant = UMI3DEnvironmentLoader.Parameters.ChooseVariant(assetLibrary);
            UMI3DClientServer.GetFile(Path.Combine(assetLibrary.baseUrl, variant.path), action, error, false);
            yield return new WaitUntil(() => { return finished; });
        }

        public static bool isKnowedLibrary(ulong key)
        {
            if (Instance.librariesMap.TryGetValue(key, out string libraryID))
                return Instance.libraries.ContainsKey(libraryID);
            return false;
        }


        public static void UnloadLibrary(ulong id, ulong SceneId = 0)
        {

            if (Instance.librariesMap.ContainsKey(id))
            {
                string libraryID = Instance.librariesMap[id];
                UnloadLibrary(id, SceneId);
            }
        }

        public static void UnloadLibrary(string libraryID, ulong SceneId = 0)
        {
            KeyValuePair<DataFile, HashSet<ulong>> dataf = Instance.libraries.Where((p) => { return p.Key == libraryID; }).Select((p) => { return p.Value; }).FirstOrDefault();

            if (dataf.Key != null)
            {
                if (SceneId != 0 && dataf.Value.Contains(SceneId)) dataf.Value.Remove(SceneId);
                if (dataf.Value.Count == 0)
                {
                    foreach (Data data in dataf.Key.files)
                    {
                        Instance.UnloadFile(data.url, libraryID);
                    }
                }
            }
        }

        public static void RemoveLibrary(string libraryId, ulong SceneId = 0)
        {
            KeyValuePair<DataFile, HashSet<ulong>> dataf = Instance.libraries.Where((p) => { return p.Key == libraryId; }).Select((p) => { return p.Value; }).FirstOrDefault();

            if (dataf.Key != null)
            {
                if (SceneId != 0 && dataf.Value.Contains(SceneId)) dataf.Value.Remove(SceneId);
                if (dataf.Value.Count == 0)
                {
                    foreach (Data data in dataf.Key.files)
                    {
                        Instance.UnloadFile(data.url, libraryId, true);
                    }
                    Instance.libraries.Remove(libraryId);
                }
                Directory.Delete(dataf.Key.path, true);
            }
        }

        #endregion
        #region file downloading
        private IEnumerator DownloadFiles(string key, string rootDirectoryPath, string directoryPath, List<string> applications, string date, string format, string culture, FileListDto list, Action<DataFile> finished)
        {
            var data = new DataFile(key, rootDirectoryPath, applications, date, format, culture);
            foreach (string name in list.files)
            {
                string path = Path.Combine(directoryPath, name);
                path = System.Uri.UnescapeDataString(path);
                string dicPath = System.IO.Path.GetDirectoryName(path);
                string url = Path.Combine(list.baseUrl, name);
                Action callback = () => { data.files.Add(new Data(url, path, name)); };
                Action<string> error = (s) => { UMI3DLogger.LogError(s, scope); };

                yield return StartCoroutine(DownloadFile(key, dicPath, path, url, name, callback, error));
            }
            libraries.Add(data.key, new KeyValuePair<DataFile, HashSet<ulong>>(data, new HashSet<ulong>()));
            finished.Invoke(data);
        }

        private IEnumerator DownloadFile(string key, string directoryPath, string filePath, string url, string fileRelativePath, Action callback, Action<string> error)
        {
            bool finished = false;
            Action<byte[]> action = (bytes) =>
            {
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                File.WriteAllBytes(filePath, bytes);

                finished = true;
                callback.Invoke();
            };
            Action<string> error2 = (s) =>
            {
                finished = true;
                error.Invoke(s);
            };
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, url, key);
            });
            if (objectData != null)
            {
                if (objectData.downloadedPath != null)
                {
                    yield break;
                }
                else
                {
                    objectData.downloadedPath = filePath;
                }
            }
            else
            {
                CacheCollection.Insert(0, new ObjectData(url, null, null, key, filePath, fileRelativePath));
            }

            UMI3DClientServer.GetFile(url, action, error2, !UMI3DClientServer.Instance.AuthorizationInHeader);
            yield return new WaitUntil(() => { return finished; });
        }

        private void DownloadFile(string url, Action<byte[]> callback, Action<string> error)
        {
            Action<byte[]> action = (bytes) =>
            {
                callback.Invoke(bytes);
            };
            Action<string> error2 = (s) =>
            {
                error.Invoke(s);
            };
            UMI3DClientServer.GetFile(url, action, error2, false);
        }

        private void UnloadFile(string url, string id, bool delete = false)
        {
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, url, id);
            });
            if (objectData != null)
            {
                objectData.libraryIds.Remove(id);
                if (objectData.libraryIds.Count == 0 && objectData.entityIds.Count == 0)
                {
                    objectData.DeleteAction?.Invoke(objectData.value, "Unload");
                    objectData.state = ObjectData.Estate.NotLoaded;
                    if (delete)
                        CacheCollection.Remove(objectData);
                }
            }
        }

        private static bool DefaultShouldTryAgain(RequestFailedArgument argument)
        {
            return argument.GetRespondCode() == 401 && argument.count < 3;
        }

        public static void DownloadObject(UnityWebRequest www, Action callback, Action<Umi3dException> failCallback, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            StartCoroutine(Instance._DownloadObject(www, callback, failCallback, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e)));
        }

        private IEnumerator _DownloadObject(UnityWebRequest www, Action callback, Action<Umi3dException> failCallback, Func<RequestFailedArgument, bool> ShouldTryAgain, int tryCount = 0)
        {
            yield return www.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.DataProcessingError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                //DateTime date = DateTime.UtcNow;
                //if (!UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, () => StartCoroutine(_DownloadObject(www, callback, failCallback,ShouldTryAgain,tryCount + 1)), tryCount, date, ShouldTryAgain)))
                //{
                if (failCallback != null)
                {
                    failCallback.Invoke(new Umi3dException(www.responseCode, $"failed to load {www.url} [{www.error}]"));
                }
                else
                {
                    UMI3DLogger.LogWarning(www.error, scope);
                    UMI3DLogger.LogWarning("Failed to load " + www.url, scope);
                }

                www.Dispose();
                //}
                yield break;
            }

            callback.Invoke();
            www.Dispose();
        }

        #endregion
        #region sub Models
        private Dictionary<string, SubmodelDataCollection> subModelsCache;

        public class SubmodelDataCollection
        {
            private List<SubmodelData> datas;
            public Transform root;
            private bool canUseNameRef;

            public SubmodelDataCollection()
            {
                this.datas = new List<SubmodelData>();
                this.root = null;
                this.canUseNameRef = true;
            }

            public void SetRoot(Transform transform)
            {
                root = transform;
            }

            public void AddSubModel(string refByName, List<int> refByIndex, List<string> refByNames, Transform transform)
            {
                canUseNameRef = canUseNameRef && !datas.Any(d => d.MatchName(refByName));
                datas.Add(new SubmodelData(refByName, refByIndex, refByNames, transform));
            }

            public Transform Get(string name, List<int> indexes, List<string> names)
            {
                return datas.FirstOrDefault(d =>
                {
                    return (canUseNameRef && d.MatchName(name))
                            || (!canUseNameRef &&
                                    ((indexes != null && d.MatchIndexes(indexes))
                                    || (names != null && d.MatchNames(names))));
                })?.transform;
            }

            public void Destroy()
            {
                foreach (SubmodelData data in datas)
                {
                    GameObject.Destroy(data.transform.gameObject);
                }
            }

            private class SubmodelData
            {
                private string RefByName;
                private List<int> RefByIndex;
                private List<string> RefByNames;
                public Transform transform;

                public SubmodelData(string refByName, List<int> refByIndex, List<string> refByNames, Transform transform)
                {
                    RefByName = refByName;
                    RefByIndex = refByIndex ?? new List<int>();
                    RefByNames = refByNames ?? new List<string>();
                    this.transform = transform;
                }

                public bool MatchName(string name)
                {
                    return RefByName == name;
                }
                public bool MatchNames(List<string> names)
                {
                    if (names.Count != RefByNames.Count)
                        return false;
                    return !names.Zip(RefByNames, (a, b) => a == b).Any(s => !s);
                }
                public bool MatchIndexes(List<int> indexes)
                {
                    if (indexes.Count != RefByIndex.Count)
                        return false;
                    return !indexes.Zip(RefByIndex, (a, b) => a == b).Any(s => !s);
                }
            }
        }

        public bool IsSubModelsSetFor(string modelUrlInCach)
        {
            return subModelsCache.ContainsKey(modelUrlInCach);
        }

        public void AddSubModels(string modelUrlInCache, SubmodelDataCollection nodes)
        {
            subModelsCache[modelUrlInCache] = nodes;
        }

        public void GetSubModel(string modelUrlInCache, string subModelName, List<int> indexes, List<string> names, Action<object> callback)
        {
            if (IsSubModelsSetFor(modelUrlInCache))
            {
                callback.Invoke(GetSubModelNow(modelUrlInCache, subModelName, indexes, names).gameObject);
            }
            else
            {
                Match matchUrl = ObjectData.rx.Match(modelUrlInCache);
                ObjectData objectData = CacheCollection.Find((o) =>
                {
                    return o.MatchUrl(matchUrl, modelUrlInCache);
                });
                if (objectData == null)
                    UMI3DLogger.LogError("not found in cache", scope);
                objectData.loadCallback.Add((o) =>
                {
                    GetSubModel(modelUrlInCache, subModelName, indexes, names, (obj) => { callback.Invoke(obj); });
                });
            }
        }

        public Transform GetSubModelNow(string modelUrlInCache, string subModelName, List<int> indexes, List<string> names)
        {
            return subModelsCache[modelUrlInCache].Get(subModelName, indexes, names);
        }
        public Transform GetSubModelRoot(string modelUrlInCache)
        {
            return subModelsCache[modelUrlInCache].root;
        }

        #endregion
    }
}