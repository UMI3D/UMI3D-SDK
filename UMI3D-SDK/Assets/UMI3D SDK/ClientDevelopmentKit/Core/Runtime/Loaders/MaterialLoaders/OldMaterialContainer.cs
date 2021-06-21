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
using UnityEngine;

namespace umi3d.cdk
{
    public class OldMaterialContainer : MonoBehaviour
    {
        public Material[] oldMats
        {
            get {
                if (_oldMats == null)
                {
                    InitOldMats();
                }
                return _oldMats;
            }
            set { _oldMats = value; }
        }

        /// <summary>
        /// contains a copy of the original array material 
        /// </summary>
        private Material[] _oldMats = null;


        /// <summary>
        /// Init the array
        /// </summary>
        private void InitOldMats()
        {

            int lenght = GetComponent<Renderer>().sharedMaterials.Length;
            _oldMats = new Material[lenght];

        }

    }
}
