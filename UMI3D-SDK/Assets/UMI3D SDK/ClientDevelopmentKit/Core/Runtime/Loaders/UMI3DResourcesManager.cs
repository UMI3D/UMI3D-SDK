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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using Path = inetum.unityUtils.Path;

namespace umi3d.cdk
{
    public class UMI3DResourcesManager : inetum.unityUtils.PersistentSingleBehaviour<UMI3DResourcesManager>, IUMI3DResourcesManager
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public Transform CacheTransform => gameObject.transform;

        #region const
        private const string dataFile = "data.json";
        private const string libraryFolder = "libraries";
        private const string assetDirectory = "asset";
        #endregion
        #region data


        public struct Library
        {
            public string id;
            public string version;

            public Library(string id, string version)
            {
                this.id = id;
                this.version = version;
            }

            public Library(string idVersion)
            {
                if (idVersion == null)
                    throw new Exception("The idVersion should never be null. See Library.GetLibrary");

                var s = idVersion.Split(':');

                this.id = s[0];
                this.version = s.Length > 1 ? s[1] : "-";
            }

            public static Library? GetLibrary(string idVersion)
            {
                return idVersion == null ? (Library?)null : new Library(idVersion);
            }

            public override bool Equals(object obj)
            {
                if (obj is Library lib)
                    return id.Equals(lib.id) && version.Equals(lib.version);
                return false;
            }

            public static bool operator ==(Library a, Library b)
                => a.Equals(b);

            public static bool operator !=(Library a, Library b)
                => !a.Equals(b);

        }

        /// <summary>
        /// Discribe a library and its content.
        /// </summary>
        [Serializable]
        public class DataFile
        {
            public List<Data> files;
            public string path;
            public string key;
            public string version;
            public string date;
            public List<string> applications = new List<string>();

            public DateTime Date => DateTime.Parse("yyMMdd_HHmm");
            public Library library => new Library(key, version);

            public DataFile(Library library, string path, List<string> applications, DateTime date)
            {
                this.path = path;
                this.key = library.id;
                this.version = library.version;
                files = new List<Data>();
                this.applications = applications;
                this.date = date.ToString("yyMMdd_HHmm");
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
            public HashSet<Library> libraryIds;

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
            public bool MatchUrl(Match Matchurl, string url, Library? library = null)
            {
                if (url == this.url && (library == null || libraryIds.Any(lib => lib == library)))
                    return true;

                if (a.Success && Matchurl.Success)
                {
                    if (library == null)
                        return MatchUrlUrl(a, Matchurl);
                    else
                        return MatchUrlLibrary(library.Value, Matchurl);
                }

                return false;
            }

            bool MatchUrlLibrary(Library library, Match match)
            {
                return (!string.IsNullOrEmpty(fileRelativePath)
                                && library != null
                                && libraryIds.Any(lib => lib == library))
                                && match.Groups[3].Captures[0].Value.Contains(fileRelativePath);
            }

            bool MatchUrlUrl(Match matchA, Match matchB)
            {
                return (matchA.Groups[1].Captures[0].Value == matchB.Groups[1].Captures[0].Value
                                && (matchA.Groups[2].Captures.Count == matchB.Groups[2].Captures.Count)
                                && (matchA.Groups[2].Captures.Count == 0 || matchA.Groups[2].Captures[0].Value == matchB.Groups[2].Captures[0].Value)
                                && matchA.Groups[3].Captures[0].Value == matchB.Groups[3].Captures[0].Value);
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
                libraryIds = new HashSet<Library>();
                state = Estate.Loaded;
                downloadedPath = null;
                this.url = url;
                a = rx.Match(url);
            }

            public ObjectData(string url, object value, ulong entityId)
            {
                this.value = value;
                entityIds = new HashSet<ulong>() { entityId };
                libraryIds = new HashSet<Library>();
                state = Estate.Loaded;
                downloadedPath = null;
                this.url = url;
                a = rx.Match(url);
            }

            public ObjectData(string url, string extension, string authorization, HashSet<ulong> entityId)
            {
                value = null;
                entityIds = entityId;
                libraryIds = new HashSet<Library>();
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
                libraryIds = new HashSet<Library>();
                state = Estate.NotLoaded;
                downloadedPath = null;
                this.url = url;
                this.extension = extension;
                a = rx.Match(url);
                this.authorization = ComputeAuthorization(authorization);
            }

            public ObjectData(string url, string extension, string authorization, ulong entityId, Library library)
            {
                value = null;
                entityIds = new HashSet<ulong>() { entityId };
                libraryIds = new HashSet<Library>() { library };
                state = Estate.NotLoaded;
                downloadedPath = null;
                this.url = url;
                this.extension = extension;
                a = rx.Match(url);
                this.authorization = ComputeAuthorization(authorization);
            }

            public ObjectData(string url, string extension, string authorization, Library library, string downloadedPath, string fileRelativePath)
            {
                value = null;
                entityIds = new HashSet<ulong>();
                libraryIds = new HashSet<Library>() { library };
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
                libraryIds = new HashSet<Library>();
                state = Estate.NotLoaded;
                this.downloadedPath = downloadedPath;
                this.url = url;
                this.extension = extension;
                this.authorization = authorization;
                a = rx.Match(url);
            }
        }

        public Dictionary<ulong, Library> librariesMap = new Dictionary<ulong, Library>();
        private List<ObjectData> CacheCollection;
        private Dictionary<Library, KeyValuePair<DataFile, HashSet<ulong>>> libraries;
        public static List<DataFile> Libraries => Exists ? Instance.libraries.Values.Select(k => k.Key).ToList() : new List<DataFile>();

        private Dictionary<string, SubmodelDataCollection> NsubModelsCache;

        #endregion
        #region setup

        private ThreadDeserializer deserializer;

        /// <inheritdoc/>
        protected override void Awake()
        {
            NsubModelsCache = new();
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

        public static bool ClearCache(string VariantUrl, Library? LibraryId = null)
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
                if (ObjectValue.state == ObjectData.Estate.Loading)
                {
                    ObjectValue.state = ObjectData.Estate.NotLoaded;
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

        public void ClearCache(List<Library> exceptLibraries = null)
        {
            NsubModelsCache.Clear();

            if (subModelsCache == null)
                subModelsCache = new Dictionary<string, SubmodelDataCollection>();

            if (CacheCollection != null)
            {
                foreach (ObjectData ObjectValue in CacheCollection.ToList())
                {
                    if (exceptLibraries != null)
                    {
                        foreach (var id in ObjectValue.libraryIds.Where(i => !exceptLibraries.Contains(i)).ToList())
                            ObjectValue.libraryIds.Remove(id);
                    }
                    else
                        ObjectValue.libraryIds.Clear();

                    if (ObjectValue.libraryIds.Count() == 0)
                    {
                        if (subModelsCache.ContainsKey(ObjectValue.url))
                        {
                            NsubModelsCache.Add(ObjectValue.url, subModelsCache[ObjectValue.url]);
                            subModelsCache.Remove(ObjectValue.url);
                        }

                        if (ObjectValue.state == ObjectData.Estate.Loading)
                        {
                            ObjectValue.state = ObjectData.Estate.NotLoaded;
                        }

                        ObjectValue.DeleteAction?.Invoke(ObjectValue.value, "clear all cache");

                        CacheCollection.Remove(ObjectValue);
                    }
                }
            }
            else
                CacheCollection = new List<ObjectData>();

            foreach (var item in NsubModelsCache.Values)
            {
                item.Destroy();
            }

            NsubModelsCache.Clear();

            LightmapSettings.lightmaps = new LightmapData[0];

            Resources.UnloadUnusedAssets();

            StopAllCoroutines();
            libraries = new Dictionary<Library, KeyValuePair<DataFile, HashSet<ulong>>>();
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
            string path = Path.Combine(Application.persistentDataPath, libraryFolder);
            if (Directory.Exists(path))
                foreach (string Iddirectory in Directory.GetDirectories(path).ToList())
                {
                    bool all = true;
                    foreach (string directory in Directory.GetDirectories(Iddirectory).ToList())
                    {
                        DataFile data = GetData(directory);
                        if (data != null && data.path != null && data.key != null && data.files != null)
                        {
                            all = false;
                            foreach (Data file in data.files)
                            {
                                Match matchUrl = ObjectData.rx.Match(file.url);
                                ObjectData objectData = CacheCollection.Find((o) =>
                                {
                                    return o.MatchUrl(matchUrl, file.url, data.library);
                                });
                                if (objectData != null)
                                    objectData.downloadedPath = file.path;
                                else
                                    CacheCollection.Insert(0, new ObjectData(file.url, null, null, data.library, file.path, file.fileRelativePath));
                            }
                            libraries.Add(data.library, new KeyValuePair<DataFile, HashSet<ulong>>(data, new HashSet<ulong>()));
                        }
                        else
                            Directory.Delete(directory, true);
                        if (all)
                            Directory.Delete(Iddirectory, true);
                    }
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
        public static async Task LoadLibrary(string libraryId, ulong SceneId = 0)
        {
            await (_LoadLibrary(Library.GetLibrary(libraryId).Value, SceneId));
        }

        public static async Task _LoadLibrary(Library library, ulong SceneId)
        {
            KeyValuePair<DataFile, HashSet<ulong>> lib = Instance.libraries.FirstOrDefault(l => l.Key == library).Value;
            if (lib.Key != null && SceneId != 0)
                lib.Value.Add(SceneId);

            var downloaded = Instance.CacheCollection.Where((od) => { return od.state == ObjectData.Estate.NotLoaded && od.libraryIds.Any(c => c == library); }).
                Select(async pair =>
                {

                    try
                    {
                        string extension = System.IO.Path.GetExtension(pair.url);
                        IResourcesLoader loader = UMI3DEnvironmentLoader.AbstractParameters.SelectLoader(extension);

                        if (loader != null)
                        {
                            await LoadFile(pair.entityIds.First(), pair, loader);

                        }
                    }
                    catch (Exception e) { Debug.LogException(e); }
                }).ToList();
            await Task.WhenAll(downloaded);
        }


        public static async Task LoadLibraries(List<string> ids, Progress progress)
        {
            await LoadLibraries(ids.Select(id => Library.GetLibrary(id).Value).ToList(), progress);
        }

        /// <summary>
        /// LOad a collection of libraries.
        /// </summary>
        /// <param name="ids">libraries id to load with format <id>:<version></param>
        /// <param name="loadedResources">call each time a library have been loaded with the count of all loaded libraries in parameter.</param>
        /// <param name="resourcesToLoad">call with the total count of libraries to load in parameter.</param>
        /// <returns></returns>
        public static async Task LoadLibraries(List<Library> ids, Progress progress)
        {
            progress.AddTotal();

            Instance.ClearCache(ids);

            var downloaded = Instance.CacheCollection.Where((p) => { return p.downloadedPath != null && p.state == ObjectData.Estate.NotLoaded && p.libraryIds.Any(i => ids.Any(c => c == i)); })
                .Select(async (data) =>
                {
                    progress.AddTotal();
                    try
                    {
                        string extension = System.IO.Path.GetExtension(data.url);
                        IResourcesLoader loader = UMI3DEnvironmentLoader.AbstractParameters.SelectLoader(extension);
                        if (loader != null)
                        {
                            ulong? id = data.entityIds?.FirstOrDefault();
                            if (id == null)
                            {
                                var lib = data.libraryIds?.FirstOrDefault();
                                if (lib != null && Instance.librariesMap.ContainsValue(lib.Value))
                                {
                                    id = Instance.librariesMap.FirstOrDefault(l => l.Value == lib).Key;
                                }
                            }
                            if (id == null)
                                throw new Exception("id should never be null");
                            var obj = await LoadFile(id ?? 0, data, loader);
                        }
                        progress.AddComplete();
                    }
                    catch (Exception e)
                    {
                        UMI3DLogger.LogException(e, scope);
                        if (!await progress.AddFailed(e))
                            throw;
                    }
                }).ToList();
            progress.AddComplete();
            await Task.WhenAll(downloaded);
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

        public static async Task<object> LoadFile(ulong id, FileDto file, IResourcesLoader loader)
        {
            return await Instance._LoadFile(id, file, loader);
        }

        private static async Task<object> LoadFile(ulong id, ObjectData file, IResourcesLoader loader)
        {
            return await Instance._LoadFile(id, file, loader);
        }

        private async Task<object> _LoadFile(ulong id, ObjectData objectData, IResourcesLoader loader, string PathIfInBundle = null)
        {
            if (objectData == null)
                return null;
            objectData.entityIds.Add(id);

            if (objectData.state == ObjectData.Estate.Loaded)
            {
                //callback.Invoke(objectData.value);
                // replace
                return await loader.ObjectFromCache(objectData.value, PathIfInBundle);

            }

            if (objectData.DeleteAction == null)
            {
                objectData.DeleteAction = loader.DeleteObject;
            }

            if (objectData.state == ObjectData.Estate.Loading)
            {
                while (objectData.state == ObjectData.Estate.Loading)
                {
                    await UMI3DAsyncManager.Yield();
                }
                if (objectData.state == ObjectData.Estate.NotLoaded)
                    throw new Umi3dLoadingException("Waited ObjectData failed to load");
            }
            else
            {
                try
                {
                    objectData.state = ObjectData.Estate.Loading;
                    var libID = objectData.libraryIds.FirstOrDefault();
                    string path = GetFilePath(objectData.url, libID);
                    objectData.value = await UrlToObjectWithPolicy(path, objectData.extension, objectData, null, loader);
                    objectData.state = ObjectData.Estate.Loaded;
                }
                catch
                {
                    objectData.state = ObjectData.Estate.NotLoaded;
                    throw;
                }
            }
            return await loader.ObjectFromCache(objectData.value, PathIfInBundle);

        }

        private async Task<object> UrlToObjectWithPolicy(string path, string extension, ObjectData objectData, string bundlePath, IResourcesLoader loader, Func<RequestFailedArgument, bool> ShouldTryAgain = null, int tryCount = 0)
        {
            if (ShouldTryAgain == null)
                ShouldTryAgain = DefaultShouldTryAgain;
            if (tryCount > 0)
                return null;
            DateTime date = DateTime.UtcNow;
            try
            {
                return await _UrlToObject1(loader, path, extension, objectData.authorization, bundlePath);
            }
            catch (Exception e)
            {
                var code = (e as Umi3dNetworkingException)?.errorCode ?? 0;
                if (!await UMI3DClientServer.Instance.TryAgainOnHttpFail(
                     new RequestFailedArgument(
                         code,
                         tryCount,
                         date,
                         ShouldTryAgain,
                         $"{path}\n{e.Message}\n{e.StackTrace}"
                         )))
                    throw;
            }
            return await UrlToObjectWithPolicy(path, extension, objectData, bundlePath, loader, ShouldTryAgain, tryCount + 1);
        }

        protected virtual async Task<object> _UrlToObject1(IResourcesLoader loader, string url, string extension, string authorization, string pathIfObjectInBundle, int count = 0)
        {
            try
            {
                return await loader.UrlToObject(url, extension, authorization, pathIfObjectInBundle);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (count >= 2 || (e is Umi3dNetworkingException n && n.errorCode == 404))
                    throw;
            }
            await UMI3DAsyncManager.Delay(10000);
            return await _UrlToObject1(loader, url, extension, authorization, pathIfObjectInBundle, count + 1);
        }

        public async Task<object> _LoadFile(ulong id, FileDto file, IResourcesLoader loader)
        {
            string fileName = System.IO.Path.GetFileName(file.url);
            var library = Library.GetLibrary(file.libraryKey);
            Match matchUrl = ObjectData.rx.Match(file.url);
            ObjectData objectData = CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, file.url, library);
            });

            if (objectData == null)
            {
                objectData = (library == null) ? new ObjectData(file.url, file.extension, file.authorization, id) : new ObjectData(file.url, file.extension, file.authorization, id, library.Value);
                CacheCollection.Insert(0, objectData);
            }
            return await _LoadFile(id, objectData, loader, file.pathIfInBundle);
        }

        private string GetFilePath(string url, Library? library = null)
        {
            Match matchUrl = ObjectData.rx.Match(url);

            ObjectData objectData = CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, url, library);
            });

            if (objectData != null && objectData.downloadedPath != null)
            {
                return objectData.downloadedPath;
            }
            else
            {
                return url;
            }
        }

        public static async Task<byte[]> GetFile(string url, Library? library = null)
        {
            //ObjectData objectData = Instance.CacheCollection.Find((o) => { return o.MatchUrl(url, libraryKey); });
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = Instance.CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, url, library);
            });

            if (objectData != null && objectData.downloadedPath != null)
                return (File.ReadAllBytes(objectData.downloadedPath));
            return await UMI3DClientServer.GetFile(url, false);
        }
        #endregion
        #region libraries download

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
                        var lib = new Library(assetLibrary.libraryId, assetLibrary.version);
                        if (libraries.ContainsKey(lib))
                        {
                            continue;
                        }
                    }
                    catch { };
                    toDownload.Add(assetLibrary.libraryId);
                }
            }
            return toDownload;
        }

        public static async Task DownloadLibraries(LibrariesDto libraries, string applicationName, MultiProgress progress)
        {
            await Instance.DownloadResources(libraries.libraries, applicationName, progress);
        }

        private async Task DownloadResources(List<AssetLibraryDto> assetlibraries, string applicationName, MultiProgress progress)
        {
            /* 
             * Sometimes trying to download libraries just after connection result in a 401 error: Unauthorized.
             * To fix this issue we will wait 5000ms to be sure that the browser is allowed to download.
             */
            await UMI3DAsyncManager.Delay(5000);
            if (assetlibraries != null && assetlibraries.Count > 0)
            {
                foreach (var assetlibrary in LibrariesToProgress(assetlibraries, progress))
                {
                    await DownloadResources(assetlibrary.Item2, applicationName, assetlibrary.Item1);
                }
            }
            await UMI3DAsyncManager.Yield();
        }

        List<(MultiProgress, AssetLibraryDto)> LibrariesToProgress(List<AssetLibraryDto> assetlibraries, MultiProgress progress)
        {
            return assetlibraries.Select(a =>
            {
                var progress1 = new MultiProgress($"Downloading {a.libraryId}");
                progress.Add(progress1);

                return (progress1, a);
            }).ToList();
        }


        public static async Task DownloadLibrary(AssetLibraryDto library, string application, MultiProgress progress)
        {
            await Instance._DownloadLibrary(library, application, progress);
        }

        private async Task _DownloadLibrary(AssetLibraryDto library, string application, MultiProgress progress)
        {
            await DownloadResources(library, application, progress);
        }

        private async Task DownloadResources(AssetLibraryDto assetLibrary, string application, MultiProgress progress)
        {
            Progress progress1 = new Progress(3, $"Retreiving Data for library {assetLibrary.libraryId}");
            Progress progress2 = new Progress(0, $"Downloading library {assetLibrary.libraryId}");
            Progress progress3 = new Progress(1, $"Storring library {assetLibrary.libraryId}");
            progress.Add(progress1);
            progress.Add(progress2);
            progress.Add(progress3);

            var lib = new Library(assetLibrary.libraryId, assetLibrary.version);

            try
            {
                progress1.AddComplete();
                var applications = new List<string>() { application };
                librariesMap[assetLibrary.id] = lib;
                string directoryPath = Path.Combine(Application.persistentDataPath, libraryFolder, assetLibrary.libraryId, assetLibrary.version);
                if (Directory.Exists(directoryPath))
                {
                    try
                    {
                        DataFile dt = Instance.libraries[librariesMap[assetLibrary.id]].Key;
                        if (dt.applications == null)
                            dt.applications = new List<string>();
                        if (!dt.applications.Contains(application))
                        {
                            dt.applications.Add(application);
                            SetData(dt, directoryPath);
                        }
                        progress.SetAsCompleted();
                        UMI3DLogger.Log($"{assetLibrary.id} {assetLibrary.version} already in scene.", scope);
                        return;
                    }
                    catch (Exception e)
                    {
                        UMI3DLogger.LogException(e, scope);
                    }
                    RemoveLibrary(lib);
                }

                UMI3DLocalAssetDirectoryDto variant = UMI3DEnvironmentLoader.AbstractParameters.ChooseVariant(assetLibrary);

                var bytes = await UMI3DClientServer.GetFile(Path.Combine(assetLibrary.baseUrl, variant.path), false);
                progress1.AddComplete();
                var dto = await deserializer.FromBson(bytes);
                progress1.AddComplete();
                string assetDirectoryPath = Path.Combine(directoryPath, assetDirectory);
                UnityEngine.Debug.Log($"add to {assetDirectoryPath}");
                if (dto is FileListDto)
                {
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);
                    var data = await
                        DownloadFiles(
                            lib,
                            directoryPath,
                            assetDirectoryPath,
                            applications,
                            dto as FileListDto,
                            progress2
                            );
                    SetData(data, directoryPath);
                }
                progress3.AddComplete();
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
                RemoveLibrary(lib);
                if (!await progress.ResumeAfterFail(e))
                    throw;
            }
        }

        public static bool isKnowedLibrary(ulong key)
        {
            if (Instance.librariesMap.TryGetValue(key, out Library libraryID))
                return Instance.libraries.ContainsKey(libraryID);
            return false;
        }


        public static void UnloadLibrary(ulong id, ulong SceneId = 0)
        {

            if (Instance.librariesMap.ContainsKey(id))
            {
                var libraryID = Instance.librariesMap[id];
                UnloadLibrary(libraryID, SceneId);
            }
        }

        public static void UnloadLibrary(string libraryIDVersion, ulong SceneId = 0)
        {
            UnloadLibrary(Library.GetLibrary(libraryIDVersion).Value, SceneId);
        }

        public static void UnloadLibrary(Library libraryID, ulong SceneId = 0)
        {


            var dataf = Instance.libraries.Where(p => p.Key == libraryID).Select(v => v.Value).FirstOrDefault();
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

        public static void RemoveLibrary(Library library, ulong SceneId = 0)
        {
            if (Exists && Instance.libraries != null && Instance.libraries.ContainsKey(library))
            {
                KeyValuePair<DataFile, HashSet<ulong>> dataf = Instance.libraries[library];

                if (dataf.Key != null)
                {
                    if (SceneId != 0 && dataf.Value.Contains(SceneId)) dataf.Value.Remove(SceneId);
                    if (dataf.Value.Count == 0)
                    {
                        foreach (Data data in dataf.Key.files)
                        {
                            Instance.UnloadFile(data.url, library, true);
                        }
                        Instance.libraries.Remove(library);
                    }
                    Directory.Delete(dataf.Key.path, true);
                }
            }
        }

        #endregion
        #region file downloading
        private async Task<DataFile> DownloadFiles(Library key, string rootDirectoryPath, string directoryPath, List<string> applications, FileListDto list, Progress progress)
        {

            var data = new DataFile(key, rootDirectoryPath, applications, DateTime.Now);
            progress.SetTotal(list.files.Count);
            foreach (string name in list.files)
            {
                UMI3DLogger.Log($"add file {name} {directoryPath}", scope);
                try
                {
                    string path = null;
                    string dicPath = null;
                    string url = null;

                    path = Path.Combine(directoryPath, name);
                    path = System.Uri.UnescapeDataString(path);
                    dicPath = System.IO.Path.GetDirectoryName(path);
                    url = Path.Combine(list.baseUrl, name);

                    await DownloadFile(key, dicPath, path, url, name);
                    data.files.Add(new Data(url, path, name));
                    progress.AddComplete();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    UMI3DLogger.LogException(e, scope);
                    if (!await progress.AddFailed(e))
                        throw;
                }
            }
            libraries.Add(key, new KeyValuePair<DataFile, HashSet<ulong>>(data, new HashSet<ulong>()));
            return (data);
        }

        private async Task DownloadFile(Library key, string directoryPath, string filePath, string url, string fileRelativePath, bool force = false)
        {
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = force ? null : CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, url, key);
            });

            if (objectData != null)
            {
                if (objectData.downloadedPath != null)
                {
                    return;
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

            var bytes = await UMI3DClientServer.GetFile(url, !UMI3DClientServer.Instance.AuthorizationInHeader);

            UMI3DLogger.Log($"<color=green>{directoryPath} {filePath}</color>", scope);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            File.WriteAllBytes(filePath, bytes);
        }

        private void UnloadFile(string url, Library id, bool delete = false)
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

        public static async Task DownloadObject(UnityWebRequest www, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            await Instance._DownloadObject(www, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e));
        }

        private async Task _DownloadObject(UnityWebRequest www, Func<RequestFailedArgument, bool> ShouldTryAgain, int tryCount = 0)
        {
            www.SendWebRequest();

            while (!www.isDone)
                await UMI3DAsyncManager.Yield();

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.DataProcessingError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                //DateTime date = DateTime.UtcNow;
                //if (!UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, () => StartCoroutine(_DownloadObject(www, callback, failCallback,ShouldTryAgain,tryCount + 1)), tryCount, date, ShouldTryAgain)))
                //{


                //}
                throw new Umi3dNetworkingException(www, $"Failed to load : " + www.url);
            }
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

        private string FindMatchingObjectDataUrl(string fileUrl, string libraryKey)
        {
            var library = Library.GetLibrary(libraryKey);
            Match matchUrl = ObjectData.rx.Match(fileUrl);
            ObjectData objectData = CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, fileUrl, library);
            });

            return (objectData != null) ? objectData.url : fileUrl;
        }

        public bool IsSubModelsSetFor(string fileUrl, string libraryKey)
        {
            return subModelsCache.ContainsKey(FindMatchingObjectDataUrl(fileUrl, libraryKey));
        }

        public void AddSubModels(string fileUrl, string libraryKey, SubmodelDataCollection nodes)
        {
            subModelsCache[FindMatchingObjectDataUrl(fileUrl, libraryKey)] = nodes;
        }

        public async void GetSubModel(string fileUrl, string libraryKey, string subModelName, List<int> indexes, List<string> names, Action<object> callback)
        {
            if (IsSubModelsSetFor(fileUrl, libraryKey))
            {
                callback.Invoke(GetSubModelNow(fileUrl, libraryKey, subModelName, indexes, names).gameObject);
            }
            else
            {
                Match matchUrl = ObjectData.rx.Match(fileUrl);
                ObjectData objectData = CacheCollection.Find((o) =>
                {
                    return o.MatchUrl(matchUrl, fileUrl);
                });

                if (objectData == null)
                    UMI3DLogger.LogError("not found in cache", scope);

                while (objectData.state != ObjectData.Estate.Loaded)
                    await UMI3DAsyncManager.Yield();

                GetSubModel(fileUrl, libraryKey, subModelName, indexes, names, (obj) => { callback.Invoke(obj); });
            }
        }

        public Transform GetSubModelNow(string fileUrl, string libraryKey, string subModelName, List<int> indexes, List<string> names)
        {
            return subModelsCache[FindMatchingObjectDataUrl(fileUrl, libraryKey)].Get(subModelName, indexes, names);
        }

        public Transform GetSubModelRoot(string fileUrl, string libraryKey)
        {
            return subModelsCache[FindMatchingObjectDataUrl(fileUrl, libraryKey)].root;
        }

        #endregion
    }
}