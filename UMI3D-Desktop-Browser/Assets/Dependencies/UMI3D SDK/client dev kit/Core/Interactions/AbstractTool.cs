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
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk
{
    public abstract class AbstractTool : MonoBehaviour
    {
        public static List<AbstractTool> instances { get; protected set; } = new List<AbstractTool>();

        /// <summary>
        /// Tool Id.
        /// </summary>
        public string id { get { return abstractDto.id; } }
        
        /// <summary>
        /// Toolbox name.
        /// </summary>
        public new string name { get { return abstractDto.name; } }

        /// <summary>
        /// Toolbox description.
        /// </summary>
        public string description { get { return abstractDto.description; } }

        /// <summary>
        /// 2D icon.
        /// </summary>
        public ResourceDto icon2D { get { return abstractDto.icon2D; } }

        /// <summary>
        /// 3D icon.
        /// </summary>
        public ResourceDto icon3D { get { return abstractDto.icon3D; } }

        /// <summary>
        /// Contained tools.
        /// </summary>
        public List<string> interactions = new List<string>();

        /// <summary>
        /// Event raised when the abstract tool is projected.
        /// </summary>
        public UnityEvent onProject = new UnityEvent();

        /// <summary>
        /// Event raised when the abstract tool is released.
        /// </summary>
        public UnityEvent onRelease = new UnityEvent();


        protected abstract AbstractToolDto abstractDto { get; set; }

        /// <summary>
        /// Contruct from dto.
        /// </summary>
        /// <param name="dto">Dto to construct from.</param>
        public virtual void SetFromDto(AbstractToolDto dto)
        {
            this.abstractDto = dto;
            interactions = dto.interactions.ConvertAll(inter => inter.id);
        }

               
        protected virtual void Awake()
        {
            if (!instances.Contains(this))
                instances.Add(this);
        }

        protected virtual void OnDestroy()
        {
            instances.Remove(this);
        }
    }
}