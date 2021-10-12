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
    public class UMI3DResourcesManager : PersistentSingleton<UMI3DResourcesManager>
    {
        #region const
        const string dataFile = "data.json";
        const string assetDirectory = "asset";
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

            public Data(string url, string path)
            {
                this.url = url;
                this.path = path;
            }
        }

        /// <summary>
        /// Write a DataFile in a directory.
        /// </summary>
        /// <param name="data">DataFile to write.</param>
        /// <param name="directory">Directory to write the file into.</param>
        void SetData(DataFile data, string directory)
        {
            string path = Path.Combine(directory, dataFile);
            FileStream file;

            if (File.Exists(path)) file = File.OpenWrite(path);
            else file = File.Create(path);

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, data);
            file.Close();
        }

        /// <summary>
        /// Read a DataFile in a directory.
        /// </summary>
        /// <param name="directory">Directory to read the file in.</param>
        /// <returns>A DataFile if the directory containe one, null otherwhise.</returns>
        DataFile GetData(string directory)
        {
            if (Directory.Exists(directory))
            {
                string path = Path.Combine(directory, dataFile);
                if (File.Exists(path))
                {

                    FileStream file;
                    file = File.OpenRead(path);
                    BinaryFormatter bf = new BinaryFormatter();
                    try
                    {
                        DataFile data = (DataFile)bf.Deserialize(file);
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
        class ObjectData
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
                set { _authorization = value; }
            }
            string _authorization;
            bool useServerAuthorization = false;

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
            public List<Action<Umi3dExecption>> loadFailCallback;
            /// <summary>
            /// Action to invoke when the object need to be deleted.
            /// </summary>
            public Action<object, string> DeleteAction;

            public enum Estate
            {
                NotLoaded, Loading, Loaded
            }

            public static Regex rx = new Regex(@"^https?://(.+?)(:\d+)*/(.*)$");
            Match a;// = rx.Match(url);

            /// <summary>
            /// Match if a url is valid for this object.
            /// Ignore difference between http and https and the port used.
            /// </summary>
            /// <param name="url">Url to match.</param>
            /// <returns></returns>
            public bool MatchUrl(Match matchUrl, string libraryId = null)
            {

                //Regex rx = new Regex(@"^https?://(.+?)(:\d+)*/(.*)$");
                //Match a = rx.Match(this.url);
                if (a.Success && matchUrl.Success)
                    return (a.Groups[1].Captures[0].Value == matchUrl.Groups[1].Captures[0].Value && a.Groups[2].Captures[0].Value == matchUrl.Groups[2].Captures[0].Value || libraryId != null && libraryId != "" && libraryIds.Contains(libraryId)) && a.Groups[3].Captures[0].Value == matchUrl.Groups[3].Captures[0].Value;
                return false;
            }

            bool MatchServerUrl()
            {
                if (UMI3DClientServer.Media == null)
                    return false;
                var url = UMI3DClientServer.Media.connection.httpUrl + '/';

                if (url == this.url) return true;

                //Regex rx = new Regex(@"^https?://(.+?)(:\d+)*/(.*)$");
                //Match a = rx.Match(this.url);
                Match b = rx.Match(url);
                if (a.Success && b.Success)
                    return (a.Groups[1].Captures[0].Value == b.Groups[1].Captures[0].Value && a.Groups[2].Captures[0].Value == b.Groups[2].Captures[0].Value);
                return false;
            }

            string ComputeAuthorization(string authorization)
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
                loadFailCallback = new List<Action<Umi3dExecption>>();
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
                loadFailCallback = new List<Action<Umi3dExecption>>();
                state = Estate.Loaded;
                downloadedPath = null;
                this.url = url;
                a = rx.Match(url);
            }

            public ObjectData(string url, string extension, string authorization, HashSet<ulong> entityId, List<Action<object>> loadCallback, List<Action<Umi3dExecption>> loadFailCallback)
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

            public ObjectData(string url, string extension, string authorization, ulong entityId, Action<object> loadCallback, Action<Umi3dExecption> loadFailCallback)
            {
                value = null;
                entityIds = new HashSet<ulong>() { entityId };
                libraryIds = new HashSet<string>();
                this.loadCallback = new List<Action<object>>() { loadCallback };
                this.loadFailCallback = new List<Action<Umi3dExecption>>() { loadFailCallback };
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
                loadFailCallback = new List<Action<Umi3dExecption>>();
                state = Estate.NotLoaded;
                downloadedPath = null;
                this.url = url;
                this.extension = extension;
                a = rx.Match(url);
                this.authorization = ComputeAuthorization(authorization);
            }

            public ObjectData(string url, string extension, string authorization, string libraryId, string downloadedPath)
            {
                value = null;
                entityIds = new HashSet<ulong>();
                libraryIds = new HashSet<string>() { libraryId };
                loadCallback = new List<Action<object>>();
                loadFailCallback = new List<Action<Umi3dExecption>>();
                state = Estate.NotLoaded;
                this.downloadedPath = downloadedPath;
                this.url = url;
                this.extension = extension;
                this.authorization = authorization;
                a = rx.Match(url);
            }

            public ObjectData(string url, string extension, string authorization, ulong entityId, string downloadedPath)
            {
                value = null;
                entityIds = new HashSet<ulong>() { entityId };
                libraryIds = new HashSet<string>();
                loadCallback = new List<Action<object>>();
                loadFailCallback = new List<Action<Umi3dExecption>>();
                state = Estate.NotLoaded;
                this.downloadedPath = downloadedPath;
                this.url = url;
                this.extension = extension;
                this.authorization = authorization;
                a = rx.Match(url);
            }

        }

        public Dictionary<ulong, string> librariesMap = new Dictionary<ulong, string>();
        List<ObjectData> CacheCollection;
        Dictionary<string, KeyValuePair<DataFile, HashSet<ulong>>> libraries;
        static public List<DataFile> Libraries { get { return Exists ? Instance.libraries.Values.Select(k => k.Key).ToList() : new List<DataFile>(); } }

        #endregion
        #region setup

        ///<inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            ClearCache();
        }

        public void ClearCache()
        {
            if (CacheCollection != null)
            {
                foreach (var ObjectValue in CacheCollection)
                {
                    if (ObjectValue.state == ObjectData.Estate.Loading && ObjectValue.loadFailCallback != null)
                        foreach (var failback in ObjectValue.loadFailCallback)
                        {
                            failback.Invoke(new Umi3dExecption(0, "clear all cache"));
                        }
                    ObjectValue.DeleteAction?.Invoke(ObjectValue.value, "clear all cache");
                }
            }
            if (subModelsCache != null)
            {
                foreach (Dictionary<string, Transform> item in subModelsCache.Values)
                {
                    foreach (Transform subModel in item.Values)
                    {
                        Destroy(subModel.gameObject);
                    }
                }
            }
            subModelsCache = new Dictionary<string, Dictionary<string, Transform>>();
            CacheCollection = new List<ObjectData>();
            StopAllCoroutines();
            libraries = new Dictionary<string, KeyValuePair<DataFile, HashSet<ulong>>>();
            LoadLocalLib();
        }

        void HardReset()
        {
            string path = Application.persistentDataPath;
            foreach (var directory in Directory.GetDirectories(path).ToList())
            {
                Directory.Delete(directory, true);
            }
            ClearCache();
        }

        void LoadLocalLib()
        {
            string path = Application.persistentDataPath;
            foreach (var directory in Directory.GetDirectories(path).ToList())
            {
                DataFile data = GetData(directory);
                if (data != null && data.path != null && data.key != null && data.files != null)
                {
                    foreach (var file in data.files)
                    {
                        Match matchUrl = ObjectData.rx.Match(file.url);
                        ObjectData objectData = CacheCollection.Find((o) =>
                        {
                            if (file.url == o.url)
                                return true;
                            else
                                return o.MatchUrl(matchUrl, data.key);
                        });
                        if (objectData != null)
                            objectData.downloadedPath = file.path;
                        else
                            CacheCollection.Insert(0, new ObjectData(file.url, null, null, data.key, file.path));
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
        static public void LoadLibrary(string libraryId, Action finished, ulong SceneId = 0)
        {
            Instance.StartCoroutine(_LoadLibrary(libraryId, finished, SceneId));
        }

        static public IEnumerator _LoadLibrary(string libraryId, Action finished, ulong SceneId)
        {
            int count = 0;
            var lib = Instance.libraries.Where((p) => { return p.Key == libraryId; }).Select((p) => { return p.Value; }).FirstOrDefault();
            if (lib.Key != null && SceneId != 0)
                lib.Value.Add(SceneId);

            var downloaded = Instance.CacheCollection.Where((od) => { return od.state == ObjectData.Estate.NotLoaded && od.libraryIds.Contains(libraryId); });
            foreach (var pair in downloaded)
            {

                string extension = System.IO.Path.GetExtension(pair.url);
                var loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(extension);
                if (loader != null)
                {
                    count++;
                    LoadFile(pair.entityIds.First(), pair, loader.UrlToObject, loader.ObjectFromCache, (obj) => { count--; }, (error) => { Debug.LogError(error); count--; }, loader.DeleteObject);
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
        static public IEnumerator LoadLibraries(List<string> ids, Action<int> loadedResources, Action<int> resourcesToLoad)
        {
            int count = 0;
            IEnumerable<ObjectData> downloaded = Instance.CacheCollection.Where((p) => { return p.downloadedPath != null && p.state == ObjectData.Estate.NotLoaded && p.libraryIds.Any(i => ids.Contains(i)); });
            int total = downloaded.Count();
            resourcesToLoad.Invoke(total);
            loadedResources.Invoke(0);
            foreach (var pair in downloaded)
            {
                string extension = System.IO.Path.GetExtension(pair.url);
                var loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(extension);
                if (loader != null)
                {
                    count++;
                    var id = pair.entityIds?.FirstOrDefault();
                    if (id == null)
                    {
                        var libId = pair.libraryIds?.FirstOrDefault();
                        if (libId != null && Instance.librariesMap.ContainsValue(libId))
                        {
                            id = Instance.librariesMap.FirstOrDefault(l => l.Value == libId).Key;
                        }
                    }
                    if (id == null)
                        throw new Exception("id should never be null");
                    LoadFile(id ?? 0, pair, loader.UrlToObject, loader.ObjectFromCache, (obj) => { count--; loadedResources.Invoke(total - count); }, (error) => { Debug.LogError($"{error}[{pair.url}]"); count--; }, loader.DeleteObject);
                }
            }
            yield return new WaitUntil(() => { return count <= 0; });
        }
        #endregion
        #region file Load

        static public void LoadFile(ulong id, FileDto file, Action<string, string, string, Action<object>, Action<Umi3dExecption>, string> urlToObject, Action<object, Action<object>, string> objectFromCache, Action<object> callback, Action<Umi3dExecption> failCallback, Action<object, string> deleteAction)
        {
            Instance._LoadFile(id, file, urlToObject, objectFromCache, callback, failCallback, deleteAction);
        }

        static void LoadFile(ulong id, ObjectData file, Action<string, string, string, Action<object>, Action<Umi3dExecption>, string> urlToObject, Action<object, Action<object>, string> objectFromCache, Action<object> callback, Action<Umi3dExecption> failCallback, Action<object, string> deleteAction)
        {
            Instance._LoadFile(id, file, urlToObject, objectFromCache, callback, failCallback, deleteAction);
        }

        void _LoadFile(ulong id, ObjectData objectData, Action<string, string, string, Action<object>, Action<Umi3dExecption>, string> urlToObject, Action<object, Action<object>, string> objectFromCache, Action<object> callback, Action<Umi3dExecption> failCallback, Action<object, string> deleteAction, string PathIfInBundle = null)
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
                        foreach (var back in objectData.loadCallback)
                            back.Invoke(obj);
                    };
                    Action<Umi3dExecption> error2 = (reason) =>
                    {
                        foreach (var back in objectData.loadFailCallback)
                            back.Invoke(reason);
                    };
                    StartCoroutine(urlToObjectWithPolicy(sucess2, error2, path, objectData.extension, objectData, null, urlToObject));
                };

                Action<Umi3dExecption> error = (reason) =>
                {
                    //Debug.LogWarning($"error {reason}");
                    foreach (var back in objectData.loadFailCallback)
                        back.Invoke(reason);
                };
                objectData.state = ObjectData.Estate.Loading;
                GetFilePath(objectData.url, sucess, error);
            }
        }

        IEnumerator urlToObjectWithPolicy(Action<object> succes, Action<Umi3dExecption> error, string path, string extension, ObjectData objectData, string bundlePath, Action<string, string, string, Action<object>, Action<Umi3dExecption>, string> urlToObject, Func<RequestFailedArgument, bool> ShouldTryAgain = null, int tryCount = 0)
        {
            if (ShouldTryAgain == null)
                ShouldTryAgain = DefaultShouldTryAgain;
            if (tryCount > 0)
                yield return new WaitForEndOfFrame();
            DateTime date = DateTime.UtcNow;
            Action<Umi3dExecption> error2 = (reason) =>
            {
                //Debug.Log($"here try again {reason.Message} [{reason.errorCode}] {tryCount}");
                if (!UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(reason.errorCode, () => StartCoroutine(urlToObjectWithPolicy(succes, error, path, extension, objectData, bundlePath, urlToObject, ShouldTryAgain, tryCount + 1)), tryCount, date, ShouldTryAgain)))
                    error?.Invoke(reason);
            };

            urlToObject.Invoke(path, extension, objectData.authorization, succes, error2, bundlePath);
            yield break;
        }

        void _LoadFile(ulong id, FileDto file, Action<string, string, string, Action<object>, Action<Umi3dExecption>, string> urlToObject, Action<object, Action<object>, string> objectFromCache, Action<object> callback, Action<Umi3dExecption> failCallback, Action<object, string> deleteAction)
        {
            Match matchUrl = ObjectData.rx.Match(file.url);
            ObjectData objectData = CacheCollection.Find((o) =>
            {
                if (file.url == o.url)
                    return true;
                else
                    return o.MatchUrl(matchUrl, file.libraryKey);
            });

            if (objectData == null)
            {
                objectData = new ObjectData(file.url, file.extension, file.authorization, id);
                CacheCollection.Insert(0, objectData);
            }
            _LoadFile(id, objectData, urlToObject, objectFromCache, callback, failCallback, deleteAction, file.pathIfInBundle);
        }


        void GetFilePath(string url, Action<string> callback, Action<Umi3dExecption> error, string libraryKey = null)
        {
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = CacheCollection.Find((o) =>
            {
                if (url == o.url)
                    return true;
                else
                    return o.MatchUrl(matchUrl, libraryKey);
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

        static public void GetFile(string url, Action<byte[]> callback, Action<string> error, string libraryKey = null)
        {
            //ObjectData objectData = Instance.CacheCollection.Find((o) => { return o.MatchUrl(url, libraryKey); });
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = Instance.CacheCollection.Find((o) =>
            {
                if (url == o.url)
                    return true;
                else
                    return o.MatchUrl(matchUrl, libraryKey);
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
        float librariesToDownload = 0;
        float librariesDownloaded = 0;

        static public List<string> LibrariesToDownload(LibrariesDto libraries)
        {
            return Instance._LibrariesToDownload(libraries.libraries);
        }

        public List<string> _LibrariesToDownload(List<AssetLibraryDto> assetLibraries)
        {
            List<string> toDownload = new List<string>();
            if (assetLibraries != null && assetLibraries.Count > 0)
            {
                DateTime local, server;
                foreach (var assetLibrary in assetLibraries)
                {
                    try
                    {
                        if (libraries.ContainsKey(assetLibrary.libraryId))
                        {
                            var dt = libraries[assetLibrary.libraryId].Key;
                            CultureInfo info = new CultureInfo(assetLibrary.culture);
                            CultureInfo dtInfo = new CultureInfo(dt.culture);
                            if (DateTime.TryParseExact(dt.date, dt.dateformat, dtInfo, DateTimeStyles.None, out local) && DateTime.TryParseExact(assetLibrary.date, assetLibrary.format, info, DateTimeStyles.None, out server))
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

        static public void DownloadLibraries(LibrariesDto libraries, string applicationName, Action callback, Action<string> error)
        {
            Instance.StartCoroutine(Instance.DownloadResources(libraries.libraries, applicationName, callback, error));
        }

        IEnumerator DownloadResources(List<AssetLibraryDto> assetlibraries, string applicationName, Action callback, Action<string> error)
        {
            if (assetlibraries != null && assetlibraries.Count > 0)
            {
                librariesToDownload = assetlibraries.Count;
                librariesDownloaded = 0;
                onProgressChange.Invoke(0f);
                foreach (var assetlibrary in assetlibraries)
                {
                    yield return StartCoroutine(DownloadResources(assetlibrary, applicationName));
                    librariesDownloaded += 1;
                    onProgressChange.Invoke(librariesDownloaded / librariesToDownload);
                }
            }
            yield return null;
            onLibrariesDownloaded.Invoke();
            callback.Invoke();
        }


        static public void DownloadLibrary(AssetLibraryDto library, string application, Action callback)
        {
            Instance.StartCoroutine(Instance._DownloadLibrary(library, application, callback));
        }

        IEnumerator _DownloadLibrary(AssetLibraryDto library, string application, Action callback)
        {
            yield return StartCoroutine(DownloadResources(library, application));
            callback.Invoke();
        }

        IEnumerator DownloadResources(AssetLibraryDto assetLibrary, string application)
        {
            List<string> applications = new List<string>() { application };
            librariesMap[assetLibrary.id] = assetLibrary.libraryId;
            string directoryPath = Path.Combine(Application.persistentDataPath, assetLibrary.libraryId);
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    DataFile dt = Instance.libraries[assetLibrary.libraryId].Key;
                    DateTime local, server;
                    CultureInfo info = new CultureInfo(assetLibrary.culture);
                    CultureInfo dtInfo = new CultureInfo(dt.culture);
                    if (DateTime.TryParseExact(dt.date, dt.dateformat, dtInfo, DateTimeStyles.None, out local) && DateTime.TryParseExact(assetLibrary.date, assetLibrary.format, info, DateTimeStyles.None, out server))
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
                var assetDirectoryPath = Path.Combine(directoryPath, assetDirectory);
                var dto = UMI3DDto.FromBson(bytes);
                if (dto is FileListDto)
                    StartCoroutine(DownloadFiles(assetLibrary.libraryId, directoryPath, assetDirectoryPath, applications, assetLibrary.date, assetLibrary.format, assetLibrary.culture, dto as FileListDto, (data) => { if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath); SetData(data, directoryPath); finished = true; }));
                else
                    finished = true;
            };
            Action<string> error = (s) =>
            {
                Debug.LogError(s);
                finished = true;
            };
            UMI3DLocalAssetDirectory variant = UMI3DEnvironmentLoader.Parameters.ChooseVariant(assetLibrary);
            UMI3DClientServer.GetFile(Path.Combine(assetLibrary.baseUrl, variant.path), action, error);
            yield return new WaitUntil(() => { return finished; });
        }

        public static bool isKnowedLibrary(ulong key)
        {
            string libraryID;
            if (Instance.librariesMap.TryGetValue(key, out libraryID))
                return Instance.libraries.ContainsKey(libraryID);
            return false;
        }


        public static void UnloadLibrary(ulong id, ulong SceneId = 0)
        {

            if (Instance.librariesMap.ContainsKey(id))
            {
                var libraryID = Instance.librariesMap[id];
                UnloadLibrary(id, SceneId);
            }
        }

        public static void UnloadLibrary(string libraryID, ulong SceneId = 0)
        {
            var dataf = Instance.libraries.Where((p) => { return p.Key == libraryID; }).Select((p) => { return p.Value; }).FirstOrDefault();

            if (dataf.Key != null)
            {
                if (SceneId != 0 && dataf.Value.Contains(SceneId)) dataf.Value.Remove(SceneId);
                if (dataf.Value.Count == 0)
                    foreach (var data in dataf.Key.files)
                    {
                        Instance.UnloadFile(data.url, libraryID);
                    }
            }
        }

        public static void RemoveLibrary(string libraryId, ulong SceneId = 0)
        {
            var dataf = Instance.libraries.Where((p) => { return p.Key == libraryId; }).Select((p) => { return p.Value; }).FirstOrDefault();

            if (dataf.Key != null)
            {
                if (SceneId != 0 && dataf.Value.Contains(SceneId)) dataf.Value.Remove(SceneId);
                if (dataf.Value.Count == 0)
                {
                    foreach (var data in dataf.Key.files)
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
        IEnumerator DownloadFiles(string key, string rootDirectoryPath, string directoryPath, List<string> applications, string date, string format, string culture, FileListDto list, Action<DataFile> finished)
        {
            var data = new DataFile(key, rootDirectoryPath, applications, date, format, culture);
            foreach (var name in list.files)
            {
                string path = Path.Combine(directoryPath, name);
                path = System.Uri.UnescapeDataString(path);
                string dicPath = System.IO.Path.GetDirectoryName(path);
                string url = Path.Combine(list.baseUrl, name);
                Action callback = () => { data.files.Add(new Data(url, path)); };
                Action<string> error = (s) => { Debug.LogError(s); };

                yield return StartCoroutine(DownloadFile(key, dicPath, path, url, callback, error));
            }
            libraries.Add(data.key, new KeyValuePair<DataFile, HashSet<ulong>>(data, new HashSet<ulong>()));
            finished.Invoke(data);
        }

        IEnumerator DownloadFile(string key, string directoryPath, string filePath, string url, Action callback, Action<string> error)
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
                if (url == o.url)
                    return true;
                else
                    return o.MatchUrl(matchUrl, key);
            });
            if (objectData != null)
            {
                if (objectData.downloadedPath != null)
                {
                    yield break;
                }
                else objectData.downloadedPath = filePath;
            }
            else CacheCollection.Insert(0, new ObjectData(url, null, null, key, filePath));

            UMI3DClientServer.GetFile(url, action, error2);
            yield return new WaitUntil(() => { return finished; });
        }

        void DownloadFile(string url, Action<byte[]> callback, Action<string> error)
        {
            Action<byte[]> action = (bytes) =>
            {
                callback.Invoke(bytes);
            };
            Action<string> error2 = (s) =>
            {
                error.Invoke(s);
            };
            UMI3DClientServer.GetFile(url, action, error2);
        }

        void UnloadFile(string url, string id, bool delete = false)
        {
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = CacheCollection.Find((o) =>
            {
                if (url == o.url)
                    return true;
                else
                    return o.MatchUrl(matchUrl, id);
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

        static bool DefaultShouldTryAgain(RequestFailedArgument argument)
        {
            return argument.GetRespondCode() == 401 && argument.count < 3;
        }

        static public void DownloadObject(UnityWebRequest www, Action callback, Action<Umi3dExecption> failCallback, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Instance.StartCoroutine(Instance._DownloadObject(www, callback, failCallback, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e)));
        }

        IEnumerator _DownloadObject(UnityWebRequest www, Action callback, Action<Umi3dExecption> failCallback, Func<RequestFailedArgument, bool> ShouldTryAgain, int tryCount = 0)
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                //DateTime date = DateTime.UtcNow;
                //if (!UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, () => StartCoroutine(_DownloadObject(www, callback, failCallback,ShouldTryAgain,tryCount + 1)), tryCount, date, ShouldTryAgain)))
                //{
                if (failCallback != null)
                {
                    failCallback.Invoke(new Umi3dExecption(www.responseCode, $"failed to load {www.url} [{www.error}]"));
                }
                else
                {
                    Debug.LogWarning(www.error);
                    Debug.LogWarning("Failed to load " + www.url);
                }
                //}
                yield break;
            }
            callback.Invoke();
        }

        #endregion
        #region sub Models
        public Dictionary<string, Dictionary<string, Transform>> subModelsCache;

        public void GetSubModel(string modelUrlInCache, string subModelName, Action<object> callback)
        {
            if (subModelsCache.ContainsKey(modelUrlInCache))
            {
                callback.Invoke(subModelsCache[modelUrlInCache][subModelName].gameObject);
            }
            else
            {
                Match matchUrl = ObjectData.rx.Match(modelUrlInCache);
                ObjectData objectData = CacheCollection.Find((o) =>
                {
                    if (modelUrlInCache == o.url)
                        return true;
                    else
                        return o.MatchUrl(matchUrl);
                }); if (objectData == null)
                    Debug.LogError("not found in cache");
                objectData.loadCallback.Add((o) =>
                {
                    if (subModelsCache[modelUrlInCache].ContainsKey(subModelName))
                        callback.Invoke(subModelsCache[modelUrlInCache][subModelName].gameObject);
                });

            }
            #endregion
        }
    }
}