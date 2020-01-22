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
using System.Collections.Generic;

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

        public string boneAnchorId;

        public string[] meshes;


        public UMI3DAvatarBone(string userId, string boneId)
        {
            this.boneId = boneId;
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
    }
}