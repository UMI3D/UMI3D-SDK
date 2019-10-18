using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace umi3d.common
{
    /// <summary>
    /// The class used to associate a BoneType with an ObjectId.
    /// </summary>
    [Serializable]
    public class BoneObjectPair : AbstractEntityDto
    {
        public BoneType boneType;
        public string objectId;
    }

    [Serializable]
    public class BonePairDictionary : AbstractEntityDto
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
                if (pair_.boneType == pair.boneType)
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
        /// Try to get the Id associated to a certain BoneType. Return true if passed, false otherwise.
        /// </summary>
        public bool TryGetIdValue(BoneType bonetype, out string id)
        {
            foreach (BoneObjectPair pair_ in ObjectPairList)
            {
                if (pair_.boneType == bonetype)
                {
                    id = pair_.objectId;
                    return true;
                }
            }
            id = null;
            return false;
        }

        /// <summary>
        /// Try to get the BoneType associated to a certain Id. Return true if passed, false otherwise.
        /// </summary>
        public bool TryGetTypeKey(string id, out BoneType type)
        {
            foreach (BoneObjectPair pair_ in ObjectPairList)
            {
                if (pair_.objectId == id)
                {
                    type = pair_.boneType;
                    return true;
                }
            }
            type = BoneType.None;
            return false;
        }

        /// <summary>
        /// Check if a BoneType is contained in the ObjectPairList.
        /// </summary>
        public bool ContainsKey(BoneType bonetype)
        {
            foreach (BoneObjectPair pair_ in ObjectPairList)
            {
                if (pair_.boneType == bonetype)
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