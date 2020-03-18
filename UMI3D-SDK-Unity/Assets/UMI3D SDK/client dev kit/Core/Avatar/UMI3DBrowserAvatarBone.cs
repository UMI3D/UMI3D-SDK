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
using UnityEngine;
using System.Linq;
using umi3d.common;

namespace umi3d.cdk
{
    public class UMI3DBrowserAvatarBone : MonoBehaviour
    {
        public static Dictionary<string, UMI3DBrowserAvatarBone> instances = new Dictionary<string, UMI3DBrowserAvatarBone>();

        public BoneType boneType;
        public virtual string id { get { return this.gameObject.GetInstanceID().ToString(); } }

        /// <summary>
        /// Convert this bone to a dto.
        /// </summary>
        /// <param name="viewpoint">Frame of reference</param>
        /// <returns></returns>
        public BoneDto ToDto(Transform viewpoint)
        {
            return (boneType == BoneType.None) ? null : new BoneDto()
            {
                id = id,
                name = boneType.ToString(),
                isStatic = false,
                type = boneType,
                position = Vector3.Scale(
                    viewpoint.transform.InverseTransformPoint(transform.position),
                    viewpoint.lossyScale),
                rotation = Quaternion.Inverse(viewpoint.transform.rotation) * transform.rotation,
                scale = transform.lossyScale
            };
        }

        protected virtual void Awake()
        {
            if (instances.ContainsKey(id))
            {
                if (this.gameObject.GetComponents<UMI3DBrowserAvatarBone>().Count() > 1)
                    throw new System.Exception("There can be only one bone per gameobject !");
                else
                    throw new System.Exception("Internal error");
            }
            instances.Add(id, this);
        }

        protected virtual void OnDestroy()
        {
            instances.Remove(id);
        }
    }
}
