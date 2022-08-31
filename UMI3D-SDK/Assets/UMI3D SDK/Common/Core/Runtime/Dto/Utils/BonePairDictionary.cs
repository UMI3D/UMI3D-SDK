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

namespace umi3d.common
{
    /// <summary>
    /// The class used to associate a BoneType with an ObjectId.
    /// </summary>
    [Obsolete]
    [Serializable]
    public class BoneObjectPair : UMI3DDto
    {
        public string boneId;
        public string objectId;
    }

    [Obsolete]
    [Serializable]
    public class BonePairDictionary : UMI3DDto
    {
        /// <summary>
        /// Contain a list of BoneType and Id association.
        /// </summary>
        public List<BoneObjectPair> ObjectPairList;

        public BonePairDictionary()
        {
            ObjectPairList = new List<BoneObjectPair>();
        }

        /// <summary>
        /// Return the length of the ObjectPairList.
        /// </summary>
        public int Count()
        {
            return this.ObjectPairList.Count;
        }

        /// <summary>
        /// Try to add a pair of BoneType and Id. Return true if passed, false otherwise.
        /// </summary>
        public bool TryAdd(BoneObjectPair pair)
        {
            foreach (BoneObjectPair pair_ in ObjectPairList)
            {
                if (pair_.boneId == pair.boneId)
                {
                    return false;
                }
            }
            this.ObjectPairList.Add(pair);
            return true;
        }

        public void Add(BoneObjectPair pair)
        {
            this.ObjectPairList.Add(pair);
        }

        /// <summary>
        /// Clear the ObjectPairList.
        /// </summary>
        public void Clear()
        {
            this.ObjectPairList.Clear();
        }

        /// <summary>
        /// Try to get the Id associated to a certain boneId. Return true if passed, false otherwise.
        /// </summary>
        public bool TryGetIdValue(string boneId, out string id)
        {
            foreach (BoneObjectPair pair_ in ObjectPairList)
            {
                if (pair_.boneId == boneId)
                {
                    id = pair_.objectId;
                    return true;
                }
            }
            id = null;
            return false;
        }

        /// <summary>
        /// Try to get the BoneType associated to a certain objectId. Return true if passed, false otherwise.
        /// </summary>
        public bool TryGetIdKey(string objectId, out string boneId)
        {
            foreach (BoneObjectPair pair_ in ObjectPairList)
            {
                if (pair_.objectId == objectId)
                {
                    boneId = pair_.boneId;
                    return true;
                }
            }
            boneId = null;
            return false;
        }

        /// <summary>
        /// Check if a boneId is contained in the ObjectPairList.
        /// </summary>
        public bool ContainsKey(string boneId)
        {
            foreach (BoneObjectPair pair_ in ObjectPairList)
            {
                if (pair_.boneId == boneId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if an Id is contained in the ObjectPairList.
        /// </summary>
        public bool ContainsValue(string id)
        {
            foreach (BoneObjectPair pair_ in ObjectPairList)
            {
                if (pair_.objectId == id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}