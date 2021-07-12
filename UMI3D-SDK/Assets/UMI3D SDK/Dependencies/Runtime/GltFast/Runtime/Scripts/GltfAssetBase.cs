﻿// Copyright 2020 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using UnityEngine;
using UnityEngine.Events;

namespace GLTFast
{
    using Loading;

    public class GltfAssetBase : MonoBehaviour
    {
        public GLTFast gLTFastInstance;

        public UnityAction<GltfAssetBase,bool> onLoadComplete;

        /// <summary>
        /// Indicates wheter the glTF was loaded (no matter if successfully or not)
        /// </summary>
        /// <value>True when loading routine ended, false otherwise.</value>
        public bool isDone {
            get { return gLTFastInstance!=null && gLTFastInstance.LoadingDone; }
        }

        /// <summary>
        /// Method for manual loading with custom <see cref="IDownloadProvider"/> and <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="url">URL of the glTF file.</param>
        /// <param name="downloadProvider">Download Provider for custom loading (e.g. caching or HTTP authorization)</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        /// <param name="materialGenerator">materialGenerator class used to initialise the new material.</param>
        public virtual void Load( string url, IDownloadProvider downloadProvider=null, IDeferAgent deferAgent=null, IMaterialGenerator materialGenerator = null ) {

            float startTime = Time.time;

            gLTFastInstance = new GLTFast(this,downloadProvider,deferAgent, materialGenerator);
            gLTFastInstance.onLoadComplete += OnLoadComplete;
            gLTFastInstance.onLoadComplete += (b) =>
            {
                //Debug.Log("Loading time = " + (Time.time - startTime).ToString() + "seconds");
            };
            gLTFastInstance.Load(url);
        }

        /// <summary>
        /// Creates an instance of a glTF file underneath the provided Transform's GameObject.
        /// </summary>
        /// <param name="transform">Transform that will become the parent of the new instance.</param>
        /// <returns>True if instatiation was successful.</returns>
        public bool Instantiate( Transform transform ) {
            if (gLTFastInstance != null) {
                return gLTFastInstance.InstantiateGltf(transform);
            }
            return false;
        }

        /// <summary>
        /// Returns an imported glTF material.
        /// Note: Asset has to have finished loading before!
        /// </summary>
        /// <param name="index">Index of material in glTF file.</param>
        /// <returns>glTF material if it was loaded successfully and index is correct, null otherwise.</returns>
        public UnityEngine.Material GetMaterial( int index = 0 ) {
            if (gLTFastInstance != null) {
                return gLTFastInstance.GetMaterial(index);
            }
            return null;
        } 

        protected virtual void OnLoadComplete(bool success) {
            gLTFastInstance.onLoadComplete -= OnLoadComplete;
            if(onLoadComplete!=null) {
                onLoadComplete(this,success);
            }
        }
/*
        protected virtual void OnDestroy()
        {
            if(gLTFastInstance!=null) {
                gLTFastInstance.Destroy();
                gLTFastInstance=null;
            }
        }*/
    }
}
