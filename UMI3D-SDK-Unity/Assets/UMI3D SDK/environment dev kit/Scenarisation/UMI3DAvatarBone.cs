using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;
using System.Linq;

namespace umi3d.edk
{
    /// <summary>
    /// User's avatar bone on environment.
    /// </summary>
    public class UMI3DAvatarBone 
    {
        public static Dictionary<string, Dictionary<string, UMI3DAvatarBone>> instancesByUserId { get; protected set; } = new Dictionary<string, Dictionary<string, UMI3DAvatarBone>>();

        public string userId { get; protected set; }
        public string boneId { get; protected set; }

        public BoneType boneType { get; protected set; }

        public string boneAnchorId;

        public string[] meshes;


        public UMI3DAvatarBone(string userId, string boneId, BoneType boneType)
        {
            this.boneId = boneId;
            this.boneType = boneType;
            this.userId = userId;
        }

        /// <summary>
        /// Register the UMI3DAvatarBone instance in its associated user's dictionnary.
        /// </summary>
        public void Register() 
        { 
            if (instancesByUserId.TryGetValue(userId, out Dictionary<string, UMI3DAvatarBone> userBoneDictionary))
            {
                instancesByUserId.Remove(userId);
            }

            if (userBoneDictionary.ContainsKey(boneId))
                throw new System.Exception("Bone already exists !");
            userBoneDictionary.Add(boneId, this);
            instancesByUserId.Add(userId, userBoneDictionary);
        }


        /// <summary>
        /// Unregister the UMI3DAvatarBone instance from its associated user's dictionnary.
        /// </summary>
        public void UnRegister()
        {
            if (instancesByUserId.TryGetValue(this.userId, out Dictionary<string, UMI3DAvatarBone> skeleton))
            {
                if (skeleton.TryGetValue(boneId, out UMI3DAvatarBone bone))
                {
                    //instancesByUserId.Remove(userId);
                    skeleton.Remove(boneId);
                    //instancesByUserId.Add(userId, skeleton);
                }
                else
                {
                    throw new System.Exception("Internal error");
                }
            }
            else
            {
                throw new System.Exception("Internal error");
            }
        }

        public static UMI3DAvatarBone GetUserBoneByType(string userId, BoneType boneType)
        {
            if (instancesByUserId.TryGetValue(userId, out Dictionary<string, UMI3DAvatarBone> userBoneDictionary))
            {
                foreach (KeyValuePair<string, UMI3DAvatarBone> item in userBoneDictionary)
                {
                    if (item.Value.boneType == boneType)
                    {
                        return item.Value;
                    }
                }
            }
            return null;
        }

    }
}