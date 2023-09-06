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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using umi3d.debug;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Class responsible for persistently storing data in the UMI3D browser so that it can be retrieved later.
    /// </summary>
    internal static class UMI3DDataPersistence
    {
        /// <summary>
        /// Persistent data directory. 
        /// </summary>
        public const string directory = "Data Persistence";

        /// <summary>
        /// Stores <paramref name="data"/> in the file <paramref name="fileName"/> at <see cref="Application.persistentDataPath"/>/<paramref name="directories"/>/<paramref name="fileName"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to store.</typeparam>
        /// <param name="data">The data to store.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="directories">The possible directories from <see cref="Application.persistentDataPath"/> to <paramref name="fileName"/>.</param>
        public static void StoreData<T>(T data, string fileName, string directories = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                ClientDataPersistenceException.LogException($"File name is null or empty.", null, ClientDataPersistenceException.ExceptionTypeEnum.IncorrectFileName);
                return;
            }

            string path;

            if (!string.IsNullOrEmpty(directories))
            {
                string directoriesPath = inetum.unityUtils.Path.Combine(Application.persistentDataPath, directories);
                if (!Directory.Exists(directoriesPath))
                {
                    Directory.CreateDirectory(directoriesPath);
                }
                path = inetum.unityUtils.Path.Combine(Application.persistentDataPath, $"{directories}/{fileName}");
            }
            else
            {
                path = inetum.unityUtils.Path.Combine(Application.persistentDataPath, fileName);
            }

            void serialize(FileStream file)
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, data);
            }

            if (File.Exists(path))
            {
                using (FileStream file = File.OpenWrite(path))
                {
                    serialize(file);
                }
            }
            else
            {
                using (FileStream file = File.Create(path))
                {
                    serialize(file);
                }
            }
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
        public static bool TryGetData<T>(out T data, string fileName, string directories = null)
            where T : new()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                ClientDataPersistenceException.LogException($"File name is null or empty.", null, ClientDataPersistenceException.ExceptionTypeEnum.IncorrectFileName);
                data = new T();
                return false;
            }

            string path;

            if (!string.IsNullOrEmpty(directories))
            {
                path = inetum.unityUtils.Path.Combine(Application.persistentDataPath, $"{directories}/{fileName}");
            }
            else
            {
                path = inetum.unityUtils.Path.Combine(Application.persistentDataPath, fileName);
            }

            if (!File.Exists(path))
            {
                data = new T();
                return false;
            }

            using (FileStream file = File.OpenRead(path))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    data = (T)bf.Deserialize(file);
                }
                catch (Exception e)
                {
                    ClientDataPersistenceException.LogException($"Deserialization failed for {fileName}.", e, ClientDataPersistenceException.ExceptionTypeEnum.DeserializationFailed);
                    data = new T();
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// An exception class to deal with <see cref="UMI3DClientServerConnection"/> issues.
        /// </summary>
        [Serializable]
        private class ClientDataPersistenceException : Exception
        {
            static UMI3DLogger logger = new UMI3DLogger(mainTag: $"{nameof(ClientDataPersistenceException)}");

            public enum ExceptionTypeEnum
            {
                Unknown,
                IncorrectFileName,
                DeserializationFailed
            }

            public ExceptionTypeEnum exceptionType;

            public ClientDataPersistenceException(string message, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}")
            {
                this.exceptionType = exceptionType;
            }
            public ClientDataPersistenceException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}", inner)
            {
                this.exceptionType = exceptionType;
            }

            public static void LogException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown)
            {
                logger.Exception(null, new ClientDataPersistenceException(message, inner, exceptionType));
            }
        }
    }
}
