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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
                state = Estate.Loaded;
                downloadedPath = null;
                this.url = url;
                a = rx.Match(url);
            }

            public ObjectData(string url, string extension, string authorization, HashSet<ulong> entityId)
            {
                value = null;
                entityIds = entityId;
                libraryIds = new HashSet<string>();
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

        /// <inheritdoc/>
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

        public void ClearCache()
        {
            if (CacheCollection != null)
            {
                foreach (ObjectData ObjectValue in CacheCollection)
                {
                    if (ObjectValue.state == ObjectData.Estate.Loading )
                    {
                        ObjectValue.state = ObjectData.Estate.NotLoaded;
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
        public static async Task LoadLibrary(string libraryId, ulong SceneId = 0)
        {
            await (_LoadLibrary(libraryId, SceneId));
        }

        public static async Task _LoadLibrary(string libraryId, ulong SceneId)
        {
            KeyValuePair<DataFile, HashSet<ulong>> lib = Instance.libraries.Where((p) => { return p.Key == libraryId; }).Select((p) => { return p.Value; }).FirstOrDefault();
            if (lib.Key != null && SceneId != 0)
                lib.Value.Add(SceneId);

            var downloaded = Instance.CacheCollection.Where((od) => { return od.state == ObjectData.Estate.NotLoaded && od.libraryIds.Contains(libraryId); }).
                Select(async pair =>
                {
                    
                    try
                    {
                        string extension = System.IO.Path.GetExtension(pair.url);
                        IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(extension);
                        
                        if (loader != null)
                        {
                            await LoadFile(pair.entityIds.First(), pair, loader);

                        }
                    }
                    catch(Exception e) { Debug.LogException(e); }
                }).ToList();
            await Task.WhenAll(downloaded);
        }

        /// <summary>
        /// LOad a collection of libraries.
        /// </summary>
        /// <param name="ids">libraries id to load</param>
        /// <param name="loadedResources">call each time a library have been loaded with the count of all loaded libraries in parameter.</param>
        /// <param name="resourcesToLoad">call with the total count of libraries to load in parameter.</param>
        /// <returns></returns>
        public static async Task LoadLibraries(List<string> ids, Progress progress)
        {
            progress.AddTotal();
            var downloaded = Instance.CacheCollection.Where((p) => { return p.downloadedPath != null && p.state == ObjectData.Estate.NotLoaded && p.libraryIds.Any(i => ids.Contains(i)); })
                .Select(async (data) =>
                {
                    progress.AddTotal();
                    try
                    {
                        string extension = System.IO.Path.GetExtension(data.url);
                        IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(extension);
                        if (loader != null)
                        {
                            ulong? id = data.entityIds?.FirstOrDefault();
                            if (id == null)
                            {
                                string libId = data.libraryIds?.FirstOrDefault();
                                if (libId != null && Instance.librariesMap.ContainsValue(libId))
                                {
                                    id = Instance.librariesMap.FirstOrDefault(l => l.Value == libId).Key;
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
                    string path = GetFilePath(objectData.url, objectData.libraryIds.FirstOrDefault());
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
            catch(Exception e)
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

        protected virtual async Task<object> _UrlToObject1(IResourcesLoader loader ,string url, string extension, string authorization, string pathIfObjectInBundle, int count = 0)
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
            return await _UrlToObject1(loader ,url, extension, authorization, pathIfObjectInBundle, count + 1);
        }

        private async Task<object> _LoadFile(ulong id, FileDto file, IResourcesLoader loader)
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
            return await _LoadFile(id, objectData, loader, file.pathIfInBundle);
        }

        private string GetFilePath(string url, string libraryKey = null)
        {
            Match matchUrl = ObjectData.rx.Match(url);

            ObjectData objectData = CacheCollection.Find((o) =>
            {

                return o.MatchUrl(matchUrl, url, libraryKey);
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

        public static async Task<byte[]> GetFile(string url, string libraryKey = null)
        {
            //ObjectData objectData = Instance.CacheCollection.Find((o) => { return o.MatchUrl(url, libraryKey); });
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = Instance.CacheCollection.Find((o) =>
            {
                return o.MatchUrl(matchUrl, url, libraryKey);
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

        public static async Task DownloadLibraries(LibrariesDto libraries, string applicationName, MultiProgress progress)
        {
            await Instance.DownloadResources(libraries.libraries, applicationName, progress);
        }

        private async Task DownloadResources(List<AssetLibraryDto> assetlibraries, string applicationName, MultiProgress progress)
        {
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

            try
            {
                progress1.AddComplete();
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
                                progress.SetAsCompleted();
                                return;
                            }
                            applications = dt.applications;
                            if (!applications.Contains(application))
                                applications.Add(application);
                        }
                    }
                    catch (Exception e)
                    {
                        UMI3DLogger.LogException(e, scope);
                    }
                    RemoveLibrary(assetLibrary.libraryId);
                }

                UMI3DLocalAssetDirectory variant = UMI3DEnvironmentLoader.Parameters.ChooseVariant(assetLibrary);

                var bytes = await UMI3DClientServer.GetFile(Path.Combine(assetLibrary.baseUrl, variant.path), false);
                progress1.AddComplete();
                var dto = await deserializer.FromBson(bytes);
                progress1.AddComplete();
                string assetDirectoryPath = Path.Combine(directoryPath, assetDirectory);
                if (dto is FileListDto)
                {
                    var data = await
                        DownloadFiles(
                            assetLibrary.libraryId,
                            directoryPath,
                            assetDirectoryPath,
                            applications,
                            assetLibrary.date,
                            assetLibrary.format,
                            assetLibrary.culture,
                            dto as FileListDto,
                            progress2
                            );
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);
                    SetData(data, directoryPath);
                }
                progress3.AddComplete();

            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
                RemoveLibrary(assetLibrary.libraryId);
                if (!await progress.ResumeAfterFail(e))
                    throw;
            }
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
                UnloadLibrary(libraryID, SceneId);
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
        private async Task<DataFile> DownloadFiles(string key, string rootDirectoryPath, string directoryPath, List<string> applications, string date, string format, string culture, FileListDto list, Progress progress)
        {
            var data = new DataFile(key, rootDirectoryPath, applications, date, format, culture);
            progress.SetTotal(list.files.Count);
            foreach (string name in list.files)
            {
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
                    UMI3DLogger.LogException(e, scope);
                    if (!await progress.AddFailed(e))
                        throw;
                }
            }
            libraries.Add(data.key, new KeyValuePair<DataFile, HashSet<ulong>>(data, new HashSet<ulong>()));
            return (data);
        }

        private async Task DownloadFile(string key, string directoryPath, string filePath, string url, string fileRelativePath)
        {
            Match matchUrl = ObjectData.rx.Match(url);
            ObjectData objectData = CacheCollection.Find((o) =>
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

            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            File.WriteAllBytes(filePath, bytes);
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

        public bool IsSubModelsSetFor(string modelUrlInCach)
        {
            return subModelsCache.ContainsKey(modelUrlInCach);
        }

        public void AddSubModels(string modelUrlInCache, SubmodelDataCollection nodes)
        {
            subModelsCache[modelUrlInCache] = nodes;
        }

        public async void GetSubModel(string modelUrlInCache, string subModelName, List<int> indexes, List<string> names, Action<object> callback)
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

                while (objectData.state != ObjectData.Estate.Loaded)
                    await UMI3DAsyncManager.Yield();

                GetSubModel(modelUrlInCache, subModelName, indexes, names, (obj) => { callback.Invoke(obj); });
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