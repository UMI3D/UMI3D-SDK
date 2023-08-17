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

namespace umi3d.cdk
{
    /// <summary>
    /// A data wrapper for the connection process.
    /// </summary>
    public class UMI3DConnectionData
    {
        /// <summary>
        /// The url of this connection.
        /// </summary>
        public string url;
        /// <summary>
        /// The name of this connection (received from the server).
        /// </summary>
        public string name;
        /// <summary>
        /// The name of this connection (chose by the user).
        /// </summary>
        public string nickname;
        /// <summary>
        /// The icon of this connection.
        /// </summary>
        public string icon;
        /// <summary>
        /// Whether or not this connection is marked as favorite by the user.
        /// </summary>
        public bool isFavorite;
        /// <summary>
        /// The date of the first connection.
        /// </summary>
        public DateTime firstConnection;
        /// <summary>
        /// The date of the last connection.
        /// </summary>
        public DateTime lastConnection;
        /// <summary>
        /// The number of succeeded connection.
        /// </summary>
        public int numberOfConnection;

        public UMI3DConnectionData()
        {
        }

        /// <summary>
        /// The ip part of the <see cref="url"/>
        /// </summary>
        public string ip
        {
            get
            {
                return url.Split(':')[0];
            }
        }

        /// <summary>
        /// The port part of the <see cref="url"/>
        /// </summary>
        public ushort? port
        {
            get
            {
                if (ushort.TryParse(url.Split(':')[1], out ushort port))
                {
                    return port;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// The collection of <see cref="UMI3DConnectionData"/>.
    /// 
    /// <para>
    /// Connections are stored at <see cref="UnityEngine.Application.persistentDataPath"/>/<see cref="UMI3DClientDataPersistence.directory"/>/<see cref="ConnectionDataCollection.connectionFile"/>.
    /// </para>
    /// <para>
    /// There is an internal connection collection that is saved on disk only when the <see cref="ConnectionDataCollection.Save"/> method is called.
    /// </para>
    /// </summary>
    internal static class UMI3DConnectionDataCollection
    {
        const string connectionFile = "Connections";

        static UMI3DLogger logger = new UMI3DLogger(mainTag: $"{nameof(UMI3DConnectionDataCollection)}");

        static List<UMI3DConnectionData> connections;

        public static bool hasUnsavedModifications = false;

        /// <summary>
        /// At the end of the method <see cref="connections"/> is not null.
        /// </summary>
        static void LazyInitCollection()
        {
            if (connections == null)
            {
                connections = Fetch();
            }
        }

        #region Contain

        /// <summary>
        /// Whether or not <paramref name="connectionData"/> is part of the collection.
        /// </summary>
        /// <param name="connectionData"></param>
        /// <returns></returns>
        public static bool Contains(UMI3DConnectionData connectionData)
        {
            if (connectionData == null)
            {
                return false;
            }

            LazyInitCollection();

            return connections.Contains(connectionData);
        }

        /// <summary>
        /// Whether or not there is a <see cref="UMI3DConnectionData"/> with this url in the collection.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool Contains(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            LazyInitCollection();

            return connections.Find(connection =>
            {
                return connection.url == url;
            }) != null;
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
        public static bool Add(UMI3DConnectionData connectionData)
        {
            if (connectionData == null)
            {
                ConnectionCollectionException.LogException(
                    "Parameter is null",
                    inner: null,
                    ConnectionCollectionException.ExceptionTypeEnum.ConnectionNullException
                );
                return false;
            }

            LazyInitCollection();

            if (Contains(connectionData))
            {
                return false;
            }

            connections.Add(connectionData);
            hasUnsavedModifications = true;
            return true;
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
        public static bool Remove(UMI3DConnectionData connectionData)
        {
            if (connectionData == null)
            {
                ConnectionCollectionException.LogException(
                    "Parameter is null",
                    inner: null,
                    ConnectionCollectionException.ExceptionTypeEnum.ConnectionNullException
                );
                return false;
            }

            LazyInitCollection();

            if (!Contains(connectionData))
            {
                return false;
            }

            connections.Remove(connectionData);
            hasUnsavedModifications = true;
            return true;
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
        public static bool Remove(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                ConnectionCollectionException.LogException(
                    "Parameter is null",
                    inner: null,
                    ConnectionCollectionException.ExceptionTypeEnum.URLNullOrEmptyException
                );
                return false;
            }

            return Remove(FindConnection(connection =>
            {
                return connection.url == url;
            }));
        }

        /// <summary>
        /// Updates the connections stored 
        /// </summary>
        /// <param name="connectionData"></param>
        /// <returns></returns>
        public static bool Update(UMI3DConnectionData connectionData)
        {
            if (connectionData == null)
            {
                ConnectionCollectionException.LogException(
                    "Parameter is null",
                    inner: null,
                    ConnectionCollectionException.ExceptionTypeEnum.ConnectionNullException
                );
                return false;
            }

            LazyInitCollection();

            int connectionIndex = connections.FindIndex(connection =>
            {
                return connection.url == connectionData.url;
            });
            if (connectionIndex < 0)
            {
                ConnectionCollectionException.LogException(
                    "There is no connection to update.",
                    inner: null,
                    ConnectionCollectionException.ExceptionTypeEnum.ConnectionNullException
                );
                return false;
            }

            connections.RemoveAt(connectionIndex);
            connections.Insert(connectionIndex, connectionData);
            hasUnsavedModifications = true;

            return true;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Queries the connections that correspond to the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        public static List<UMI3DConnectionData> Query(Predicate<UMI3DConnectionData> predicate, Comparison<UMI3DConnectionData> sortBy = null)
        {
            if (predicate == null)
            {
                ConnectionCollectionException.LogException(
                    "Parameter is null",
                    inner: null,
                    ConnectionCollectionException.ExceptionTypeEnum.PredicateNullException
                );
                return null;
            }

            LazyInitCollection();

            List<UMI3DConnectionData> result = connections.FindAll(predicate);

            if (sortBy != null)
            {
                result.Sort(sortBy);
            }

            return result;
        }

        /// <summary>
        /// Finds the <see cref="UMI3DConnectionData"/> that correspond to the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static UMI3DConnectionData FindConnection(Predicate<UMI3DConnectionData> predicate)
        {
            if (predicate == null)
            {
                ConnectionCollectionException.LogException(
                    "Parameter is null",
                    inner: null,
                    ConnectionCollectionException.ExceptionTypeEnum.PredicateNullException
                );
                return null;
            }

            LazyInitCollection();

            return connections.Find(predicate);
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Fetch the connection collection that is currently saved.
        /// </summary>
        /// <returns></returns>
        public static List<UMI3DConnectionData> Fetch()
        {
            if (!UMI3DClient.TryGetData_DP(
                out List<UMI3DConnectionData> connections,
                connectionFile,
                UMI3DClient.directory
             ))
            {
                return new List<UMI3DConnectionData>();
            }

            return connections;
        }

        /// <summary>
        /// Save the current state of the connection collection.
        /// </summary>
        public static void Save()
        {
            UMI3DClient.StoreData_DP(connections, connectionFile, UMI3DClient.directory);

            hasUnsavedModifications = false;
        }

        #endregion


        /// <summary>
        /// An exception class to deal with <see cref="ConnectionDataCollection"/> issues.
        /// </summary>
        [Serializable]
        class ConnectionCollectionException : Exception
        {
            static UMI3DLogger logger = new UMI3DLogger(mainTag: $"{nameof(ConnectionCollectionException)}");

            public enum ExceptionTypeEnum
            {
                Unknown,
                ConnectionNullException,
                PredicateNullException,
                URLNullOrEmptyException
            }

            public ExceptionTypeEnum exceptionType;

            public ConnectionCollectionException(string message, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}")
            {
                this.exceptionType = exceptionType;
            }
            public ConnectionCollectionException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}", inner)
            {
                this.exceptionType = exceptionType;
            }

            public static void LogException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown)
            {
                logger.Exception(null, new ConnectionCollectionException(message, inner, exceptionType));
            }
        }
    }
}
