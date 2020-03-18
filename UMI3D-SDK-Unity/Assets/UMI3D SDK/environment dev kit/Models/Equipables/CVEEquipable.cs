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

using System;
using UnityEngine;
using UnityEngine.Events;
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Equipable object.
    /// </summary>
    [RequireComponent(typeof(AbstractObject3D))]
    public class CVEEquipable : MonoBehaviour
    {
        public UMI3DUser equipedUser = null;

        /// <summary>
        /// Event raised when the object has been equiped by the user.
        /// </summary>
        public UnityEvent onEquiped;

        /// <summary>
        /// Event raised when the object has been unequiped by the user.
        /// </summary>
        public UnityEvent onUnequiped;


        public bool isEquiped { get; protected set; }

        /// <summary>
        /// Associated object's id.
        /// </summary>
        protected string objectId { get { return GetComponent<AbstractObject3D>().Id; } }


        /// <summary>
        /// Ask the user to equip the object on a given bone for a given user.
        /// </summary>
        /// <param name="boneId"></param>
        /// <param name="user">User to equip</param>
        /// <see cref="onEquiped"/>
        public void RequestEquip(UMI3DUser user, string boneId)
        {
            if (equipedUser == null)
            {
                EquipeDto dto = ConvertToEquipDto(boneId);
                user.Send(dto);
            }
            else
            {
                RequestUnequip();
                UnityAction onUnequipedCalback = null;
                onUnequipedCalback = () => 
                { 
                    RequestEquip(user, boneId);
                    onUnequiped.RemoveListener(onUnequipedCalback);
                };
                onUnequiped.AddListener(onUnequipedCalback);
            }
        }

        /// <summary>
        /// Ask the user to release the object.
        /// </summary>
        /// <see cref="onUnequiped"/>
        public void RequestUnequip()
        {
            if (equipedUser != null)
            {
                UnequipeDto dto = ConvertToUnequipDto();
                equipedUser.Send(dto);
            }
            else
            {
                throw new InvalidOperationException("This object is not equiped yet!");
            }
        }


        /// <summary>
        /// Convert equipable to Data Transfer Object for a given bone. 
        /// </summary>
        /// <param name="boneId">the bone Id</param>
        /// <returns>an EquipeDto</returns>
        private EquipeDto ConvertToEquipDto(string boneId)
        {
            EquipeDto dto = new EquipeDto
            {
                boneId = boneId,
                objectId = objectId
            };
            return dto;
        }


        /// <summary>
        /// Convert equipable to Data Transfer Object. 
        /// </summary>
        /// <returns>an UnequipeDto</returns>
        private UnequipeDto ConvertToUnequipDto()
        {
            UnequipeDto dto = new UnequipeDto
            {
                objectId = objectId
            };
            return dto;
        }


        protected virtual void Awake()
        {
            onEquiped.AddListener(() => { isEquiped = true; });
            onUnequiped.AddListener(() => { isEquiped = false; });
        }

        protected void OnDestroy()
        {
            if (equipedUser != null)
            {
                equipedUser.userEquipment.Remove(this);
                RequestUnequip();
            }
        }

    }
}