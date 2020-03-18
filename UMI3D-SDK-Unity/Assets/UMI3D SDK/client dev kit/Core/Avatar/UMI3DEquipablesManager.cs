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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using umi3d.common;

namespace umi3d.cdk
{
    /// <summary>
    /// Manage the equipement behaviours.
    /// </summary>
    public class UMI3DEquipablesManager : Singleton<UMI3DEquipablesManager>
    {

        public class EquipmentEvent : UnityEvent<Equipment> { }

        public EquipmentEvent onEquip = new EquipmentEvent();

        public EquipmentEvent onUnequip = new EquipmentEvent();

        /// <summary>
        /// Angular threeshold to exceed to send a transform update to the environment.
        /// </summary>
        public float angularthreeshold = 1;

        /// <summary>
        /// Position delta threeshold to exceed to send a transform update to the environment.
        /// </summary>
        public float positionthreeshold = 0.01f;

        /// <summary>
        /// Scale delta threeshold to exceed to send a transform update to the environment.
        /// </summary>
        public float scalethreeshold = 0.01f;


        /// <summary>
        /// Equipe an object on a given bone.
        /// </summary>
        /// <param name="objectId">Object to equipe id</param>
        /// <param name="boneId">Bone to equipe on id</param>
        public virtual void Equipe(EquipeDto dto)
        {
            GameObject object3D = UMI3DBrowser.Scene.GetObject(dto.objectId);
            Equipment equipment = object3D.GetComponent<Equipment>();
            if (equipment == null)
                equipment = object3D.AddComponent<Equipment>();

            equipment.Equipe(dto);
            onEquip.Invoke(equipment);
        }

        /// <summary>
        /// Unequipe an object.
        /// </summary>
        /// <param name="objectId">Object to unequipe</param>
        public virtual void Unequipe(UnequipeDto dto)
        {
            Equipment equipment = Equipment.equipedInstances[dto.objectId];
            if (equipment != null)
            {
                equipment.Unequipe();
                //onUnequip.Invoke(equipment);
            }
            else
            {
                throw new InvalidOperationException("The equipment to delete is not found.");
            }
        }

        public void DtoDispatcher(AbstractEquipEventDto dto)
        {
            if (dto is EquipeDto)
                Equipe((dto as EquipeDto));
            else if (dto is UnequipeDto)
                Unequipe((dto as UnequipeDto));
            else
                throw new ArgumentException(dto.Dtype + " is not supported!");
        }
    }
}