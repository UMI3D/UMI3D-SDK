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
using UnityEngine;
using UnityEngine.Events;
using umi3d.common;
using System.Collections.Generic;

namespace umi3d.cdk
{
    /// <summary>
    /// Describe objects that are equiped on an avatar bone.
    /// </summary>
    public class Equipment : MonoBehaviour
    {
        /// <summary>
        /// Equiped equipments, id corresponds to the equiped object's id.
        /// </summary>
        public static Dictionary<string, Equipment> equipedInstances { get; protected set; } = new Dictionary<string, Equipment>();


        /// <summary>
        /// Event raised when the equipment has been successfully equiped.
        /// </summary>
        /// <see cref="FromDto(EquipeDto)"/>
        public UnityEvent onEquiped = new UnityEvent();

        /// <summary>
        /// Event raised when the equipment has been successfully unequiped.
        /// </summary>
        public UnityEvent onUnequiped = new UnityEvent();


        public bool isEquiped { get; protected set; }

        /// <summary>
        /// Id of the UMI3D object corresponding to the equipement.
        /// </summary>
        public string objectId { get; protected set; }

        /// <summary>
        /// Coroutine logging the equipment object local transform
        /// </summary>
        private Coroutine transformLogger = null;

        /// <summary>
        /// Bone equiped.
        /// </summary>
        public UMI3DBrowserAvatarBone bone = null;


        /// <summary>
        /// Initialise the component from a given dto.
        /// </summary>
        /// <param name="dto"></param>
        public void Equipe(EquipeDto dto)
        {
            if (!isEquiped)
            {
                objectId = dto.objectId;
                bone = UMI3DBrowserAvatarBone.instances[dto.boneId];

                transformLogger = StartCoroutine(logEquipmentLocalTransform());

                Interactable interactable = this.GetComponent<Interactable>();
                if (interactable != null)
                {
                    if (!AbstractInteractionMapper.Instance.IsToolSelected(interactable.id))
                        AbstractInteractionMapper.Instance.SelectTool(interactable.id, new Equiped() { dto = dto });
                }

                isEquiped = true;
                equipedInstances.Add(objectId, this);
                ConfirmEquipe();
                onEquiped.Invoke();
            }
        }

        void ConfirmEquipe()
        {
            UMI3DWebSocketClient.Send(new EquipeConfirmationDto()
            {
                objectId = this.objectId
            });
        }

        public void Unequipe()
        { 
            if (isEquiped)
            {
                isEquiped = false;
                StopCoroutine(transformLogger);
                transformLogger = null;

                Interactable interactable = this.GetComponent<Interactable>();
                if ((interactable != null) && AbstractInteractionMapper.Instance.IsToolSelected(interactable.id))
                {
                    AbstractInteractionMapper.Instance.ReleaseTool(interactable.id, new Equiped());
                }

                equipedInstances.Remove(objectId);
                ConfirmUnequipe();
                UMI3DEquipablesManager.Instance.onUnequip.Invoke(this);
                onUnequiped.Invoke();
            }
        }

        void ConfirmUnequipe()
        {
            UMI3DWebSocketClient.Send(new UnequipeConfirmationDto()
            {
                objectId = this.objectId
            });
        }


        private Quaternion lastSentRotation;
        private Vector3 lastSentPosition, lastSentScale;

        private bool shouldSentTransform()
        {
            if (Vector3.Distance(this.transform.localScale, lastSentScale) > UMI3DEquipablesManager.Instance.scalethreeshold)
                return true;
            if (Vector3.Distance(this.transform.localPosition, lastSentPosition) > UMI3DEquipablesManager.Instance.positionthreeshold)
                return true;
            if (Quaternion.Angle(this.transform.localRotation, lastSentRotation) > UMI3DEquipablesManager.Instance.angularthreeshold)
                return true;

            return false;
        }

        /// <summary>
        /// Send the equipment local transform to the server.
        /// </summary>
        /// <returns></returns>
        IEnumerator logEquipmentLocalTransform()
        {
            bool firstSend = true;
            while (true)
            {
                if (shouldSentTransform() || firstSend)
                {
                    UpdateEquipmentTransformDto dto = new UpdateEquipmentTransformDto()
                    {
                        objectId = this.objectId,
                        position = this.transform.localPosition,
                        rotation = this.transform.localRotation,
                        scale = this.transform.localScale
                    };

                    UMI3DWebSocketClient.Send(dto);
                    firstSend = false;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Make the GameObject following the equiped bone.
        /// </summary>
        private void FollowBone()
        {
            if (bone != null)
            {
                transform.position = bone.transform.position; // Verify consistency and offset.
                transform.rotation = bone.transform.rotation; // Verify consistency and offset.
            }
            else
            {
                throw new Exception("The bone to follow is not set.");
            }
        }

      

        protected virtual void Update()
        {
            if (isEquiped)
                FollowBone();
        }

    }
}