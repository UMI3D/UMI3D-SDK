/*
Copyright 2019 Gfi Informatique

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
using umi3d.common;


namespace umi3d.cdk
{
    public class AvatarMapping : MonoBehaviour
    {

        public string userId;
        public BonePairDictionary bonePairDictionary = new BonePairDictionary();

        /// <summary>
        /// Setup a Mapping
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="pairDictionary"></param>
        public void SetMapping(string id, BonePairDictionary pairDictionary)
        {
            userId = id;
            bonePairDictionary = pairDictionary;
        }
    }
}
