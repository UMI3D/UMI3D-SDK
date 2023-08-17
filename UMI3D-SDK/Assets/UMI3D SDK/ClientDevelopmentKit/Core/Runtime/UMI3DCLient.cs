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
using System;
using System.Collections.Generic;
using umi3d.debug;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// The main class to interact with a UMI3D browser.
    /// </summary>
    public static class UMI3DClient
    {
        #region Data Persistence

        /// <summary>
        /// Persistent data directory. 
        /// </summary>
        public const string directory = UMI3DDataPersistence.directory;

        /// <summary>
        /// Stores <paramref name="data"/> in the file <paramref name="fileName"/> at <see cref="Application.persistentDataPath"/>/<paramref name="directories"/>/<paramref name="fileName"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to store.</typeparam>
        /// <param name="data">The data to store.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="directories">The possible directories from <see cref="Application.persistentDataPath"/> to <paramref name="fileName"/>.</param>
        public static void StoreData_DP<T>(T data, string fileName, string directories = null)
        {
            UMI3DDataPersistence.StoreData(data, fileName, directories);
        }

        /// <summary>
        /// Trys to get the data located at <see cref="Application.persistentDataPath"/>/<paramref name="directories"/>/<paramref name="fileName"/>.
        /// 
        /// <para>
        /// <list type="bullet">
        /// <item>True: if the data has been found and the process of deserialization succeeded.</item>
        /// <item>False: if the data has not been found or the deserialization process failed.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of data stored.</typeparam>
        /// <param name="data">The data stored.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="directories">The possible directories from <see cref="Application.persistentDataPath"/> to <paramref name="fileName"/>.</param>
        /// <returns></returns>
        public static bool TryGetData_DP<T>(out T data, string fileName, string directories = null)
            where T : new()
        {
            return UMI3DDataPersistence.TryGetData(out data, fileName, directories);
        }

        #endregion

        #region Connection Data

        /// <summary>
        /// Whether or not the internal connection collection has been modified without beeing saved.
        /// </summary>
        public static bool HasConnectionsUnsaved
        {
            get
            {
                return UMI3DConnectionDataCollection.hasUnsavedModifications;
            }
        }

        #region Contain

        /// <summary>
        /// Whether or not <paramref name="connectionData"/> is part of the collection.
        /// </summary>
        /// <param name="connectionData"></param>
        /// <returns></returns>
        public static bool Contains_CD(UMI3DConnectionData connectionData)
        {
            return UMI3DConnectionDataCollection.Contains(connectionData);
        }

        /// <summary>
        /// Whether or not there is a <see cref="UMI3DConnectionData"/> with this url in the collection.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool Contains_CD(string url)
        {
            return UMI3DConnectionDataCollection.Contains(url);
        }

        #endregion

        #region Adds, Removes and Updates

        /// <summary>
        /// Adds a new connection to the collection.
        /// 
        /// <para>
        /// Return:
        /// <list type="bullet">
        /// <item>True: if the connection has been added.</item>
        /// <item>False: if the connection has not been added (because it is already present in the collection or the connection is null).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="connectionData"></param>
        /// <returns></returns>
        public static bool Add_CD(UMI3DConnectionData connectionData)
        {
            return UMI3DConnectionDataCollection.Add(connectionData);
        }

        /// <summary>
        /// Removes <paramref name="connectionData"/> from the collection.
        /// 
        /// <para>
        /// Return:
        /// <list type="bullet">
        /// <item>True: if the connection has been removed.</item>
        /// <item>False: if the connection has not been removed (because it is not present in the collection or the connection is null).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="connectionData"></param>
        /// <returns></returns>
        public static bool Remove_CD(UMI3DConnectionData connectionData)
        {
            return UMI3DConnectionDataCollection.Remove(connectionData);
        }

        /// <summary>
        /// Removes the <see cref="UMI3DConnectionData"/> corresponding to <paramref name="url"/> from the collection.
        /// 
        /// <para>
        /// Return:
        /// <list type="bullet">
        /// <item>True: if the connection has been removed.</item>
        /// <item>False: if the connection has not been removed (because it is not present in the collection or the url is null).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="connectionData"></param>
        /// <returns></returns>
        public static bool Remove_CD(string url)
        {
            return UMI3DConnectionDataCollection.Remove(url);
        }

        /// <summary>
        /// Updates the connections stored 
        /// </summary>
        /// <param name="connectionData"></param>
        /// <returns></returns>
        public static bool Update_CD(UMI3DConnectionData connectionData)
        {
            return UMI3DConnectionDataCollection.Update(connectionData);
        }

        #endregion

        #region Queries

        /// <summary>
        /// Returns favorite connection sorted by <paramref name="sortBy"/>.
        /// </summary>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        public static List<UMI3DConnectionData> GetFavorites_CD(Comparison<UMI3DConnectionData> sortBy = null)
        {
            return UMI3DConnectionDataCollection.Query(
                predicate: connection =>
                {
                    return connection.isFavorite;
                },
                sortBy
            );
        }

        /// <summary>
        /// Queries the connections that correspond to the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        public static List<UMI3DConnectionData> Query_CD(Predicate<UMI3DConnectionData> predicate, Comparison<UMI3DConnectionData> sortBy = null)
        {
            return UMI3DConnectionDataCollection.Query(predicate, sortBy);
        }

        /// <summary>
        /// Finds the <see cref="UMI3DConnectionData"/> that correspond to the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static UMI3DConnectionData FindConnection_CD(Predicate<UMI3DConnectionData> predicate)
        {
            return UMI3DConnectionDataCollection.FindConnection(predicate);
        }

        /// <summary>
        /// Find the <see cref="UMI3DConnectionData"/> that match the <paramref name="url"/>.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static UMI3DConnectionData FindConnection_CD(string url)
        {
            return UMI3DConnectionDataCollection.FindConnection(connection =>
            {
                return connection.url == url;
            });
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Fetch the connection collection that is currently saved.
        /// </summary>
        /// <returns></returns>
        public static List<UMI3DConnectionData> Fetch_CD()
        {
            return UMI3DConnectionDataCollection.Fetch();
        }

        /// <summary>
        /// Save the current state of the connection collection.
        /// </summary>
        public static void Save_CD()
        {
            UMI3DConnectionDataCollection.Save();
        }

        #endregion

        #endregion

        #region Scene Manager

        /// <summary>
        /// Load a scene asyncronously.
        /// 
        /// <para>
        /// The method doesn't let you load a new instance of the scene if it is already active or is already in the process of beeing active.
        /// </para>
        /// </summary>
        /// <param name="sceneToLoad"></param>
        /// <param name="shouldStopLoading"></param>
        /// <param name="loadingProgress"></param>
        /// <param name="loadingSucced"></param>
        /// <param name="loadingFail"></param>
        /// <param name="loadMode"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public static Coroutine LoadSceneAsync_SM(
            string sceneToLoad,
            Func<bool> shouldStopLoading, Action<float> loadingProgress,
            Action loadingSucced, Action loadingFail,
            UnityEngine.SceneManagement.LoadSceneMode loadMode = UnityEngine.SceneManagement.LoadSceneMode.Additive,
            UMI3DLogReport report = null
        )
        {
            return UMI3DSceneManager.LoadSceneAsync(sceneToLoad, shouldStopLoading, loadingProgress, loadingSucced, loadingFail, loadMode, report);
        }

        /// <summary>
        /// Unload a scene asyncronously.
        /// 
        /// <para>
        /// The method doesn't let you unload a scene if it is already unloaded or is already in the process of beeing unloaded.
        /// </para>
        /// </summary>
        /// <param name="sceneToUnload"></param>
        /// <param name="unloadingProgress"></param>
        /// <param name="unloadingSucced"></param>
        /// <param name="unloadMode"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public static Coroutine UnloadSceneAsync_SM(
            string sceneToUnload,
            Action<float> unloadingProgress, Action unloadingSucced,
            UnityEngine.SceneManagement.UnloadSceneOptions unloadMode = UnityEngine.SceneManagement.UnloadSceneOptions.UnloadAllEmbeddedSceneObjects,
            UMI3DLogReport report = null
        )
        {
            return UMI3DSceneManager.UnloadSceneAsync(sceneToUnload, unloadingProgress, unloadingSucced, unloadMode, report);
        }

        #endregion
    }
}
