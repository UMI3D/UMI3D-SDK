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

namespace umi3d.cdk
{
    /// <summary>
    /// Load a file into an object. 
    /// </summary>
    public interface IResourcesLoader
    {
        /// <summary>
        /// State if this loader is suitable for a extension.
        /// </summary>
        /// <param name="extension"></param>
        bool IsSuitableFor(string extension);

        /// <summary>
        /// State if an extension should be ignored.
        /// </summary>
        /// <param name="extension"></param>
        bool IsToBeIgnored(string extension);

        /// <summary>
        /// Loading function.
        /// </summary>
        /// <param name="url">Path or Url where the file is located.</param>
        /// <param name="authorization">authorization string use when file isn't local.</param>
        /// <param name="callback">Function to call when loading is done.</param>
        /// <param name="failCallback">Funtion to call when loading fail.</param>
        void UrlToObject(string url, string extension, string authorization, Action<object> callback, Action<string> failCallback, string pathIfObjectIsInBundle = "");

        /// <summary>
        /// convert functio. Should test if the loaded object is an object or a bundle of objects, then return the object at pathIfObjectInBundle  
        /// </summary>
        /// <param name="objectLoaded">Object loaded by UrlToObject(), it could be a simple object or a bundle of objects.</param>
        /// <param name="callback">Function to call when loading is done.</param>
        /// <param name="pathIfObjectInBundle">Path or identifier of the object to load in the bundle, equal "" if objectLoaded is not a bundle</param>
        void ObjectFromCache(object objectLoaded, Action<object> callback, string pathIfObjectInBundle);

        /// <summary>
        /// Function to delete un object loaded by this loader
        /// </summary>
        /// <param name="objectLoaded"></param>
        /// <param name="reason"></param>
        void DeleteObject(object objectLoaded, string reason);
    }
}