﻿/*
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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for Gameobject DTO loading.
    /// </summary>
    /// <typeparam name="DTO">Data To Object</typeparam>
    public abstract class AbstractObjectDTOLoader<DTO> : AbstractDTOLoader<DTO, GameObject> where DTO : AbstractObject3DDto
    {

        /// <summary>
        /// Load a gameobject from Dto and pass it to a given callback.
        /// </summary>
        /// <param name="dto">Data to load from</param>
        /// <param name="callback">Callback to execute after loading</param>
        public override void LoadDTO(DTO dto, Action<GameObject> callback)
        {
            GameObject obj = new GameObject();
            callback(obj);
            InitObjectFromDto(obj, dto);
        }

        /// <summary>
        /// Init a gameobject from the last received dto.
        /// /!\ Should not be used in Load methods. Use InitObjectFromDto in that case!
        /// </summary>
        /// <param name="go">GameObject to update</param>
        /// <param name="dto">a dto describing the gameobject at a given time.</param>
        protected void InitObjectFromDto( GameObject obj , DTO dto)
        {
            if(obj != null && dto != null)
            {
                DTO old = UMI3DBrowser.Scene.GetDto(dto.Id) as DTO;
                DTO last = old.Time > dto.Time ? old : dto;
                UpdateFromDTO(obj, null, last);
            }
        }

        /// <summary>
        /// Update a gameobject from dto.
        /// /!\ Should not be used in Load methods. Use InitObjectFromDto in that case!
        /// </summary>
        /// <param name="go">GameObject to update</param>
        /// <param name="olddto">Old dto describing the gameobject</param>
        /// <param name="newdto">Dto to update the gameobject to</param>
        public override void UpdateFromDTO(GameObject go, DTO olddto, DTO newdto)
        {
            if (newdto == null)
            {
                Debug.LogWarning("Warning ! no dto to update to : " + go.name);
                return;
            }

            go.name = newdto.Name;
            UpdateTransform(go, olddto, newdto);
            UpdateARTracker(go, newdto);
            UpdateBillboard(go, newdto);
            UpdateHierarchy(go, olddto, newdto);
        }

        /// <summary>
        /// Update the tranform component of an gameobject using dto.
        /// </summary>
        /// <param name="go">GameObject to update transform</param>
        /// <param name="olddto">Old dto describing the gameobject</param>
        /// <param name="newdto">Dto to update the gameobject transform to</param>
        protected virtual void UpdateTransform(GameObject go, DTO olddto, DTO newdto)
        {
            if(AbstractScene.IsImmersiveDevice || newdto.TrackerDto == null)
                go.transform.localPosition = newdto.Position;
            go.transform.localScale = newdto.Scale;
            if (!newdto.Billboard)
                go.transform.localRotation = newdto.Rotation;
        }

        /// <summary>
        /// Update the tranform component of an gameobject using dto.
        /// </summary>
        /// <param name="go">GameObject to update transform</param>
        /// <param name="olddto">Old dto describing the gameobject</param>
        /// <param name="newdto">Dto to update the gameobject transform to</param>
        protected virtual void UpdateARTracker(GameObject go, DTO newdto)
        {
            var arTracker = go.GetComponent<ARTrackerObject>();
            if (!AbstractScene.IsImmersiveDevice && newdto.TrackerDto != null)
            {
                if (arTracker == null)
                {
                    GetComponentInParent<ARTrackerDtoLoader>()?.LoadDTO(newdto.TrackerDto, (ARTrackerObject) =>
                    {
                        arTracker = go.AddComponent<ARTrackerObject>();
                        arTracker.Set(ARTrackerObject);
                        Destroy(ARTrackerObject);
                    });
                }

                arTracker.UpdateTracker(newdto.TrackerDto);

                arTracker.position = newdto.Position;
                arTracker.rotation = newdto.Rotation;
                arTracker.scale = newdto.Scale;
            }
            else
            {
                if (arTracker != null)
                    Destroy(arTracker);
            }
        }


        /// <summary>
        /// Update the billboard component of an gameobject using dto.
        /// </summary>
        /// <param name="go">GameObject to update billboard</param>
        /// <param name="newdto">Dto to update the gameobject billboard to</param>
        protected virtual void UpdateBillboard(GameObject go, DTO newdto)
        {
            var billboard = go.GetComponent<Billboard>();
            if (newdto.Billboard)
            {
                if (billboard == null)
                    go.AddComponent<Billboard>();
            }
            else
            {
                if (billboard != null)
                    Destroy(billboard);
            }
        }

        /// <summary>
        /// Update a gameobject hierarchy using dto.
        /// </summary>
        /// <param name="go">GameObject to update hierarchy</param>
        /// <param name="olddto">Old dto describing the gameobject</param>
        /// <param name="newdto">Dto to update the gameobject hierarchy to</param>
        protected virtual void UpdateHierarchy(GameObject go, DTO olddto, DTO newdto)
        {

            if (olddto == null || newdto.Pid != olddto.Pid)
            {
                GameObject _p = null;
                if (newdto.Pid == null || newdto.Pid.Length == 0)
                    _p = gameObject;
                else
                    _p = UMI3DBrowser.Scene.GetObject(newdto.Pid);

                if (_p == null)
                    UMI3DBrowser.Scene.RemoveObject(newdto.Id);

                else if (go.transform.parent != _p.transform)
                    go.transform.SetParent(_p.transform, false);
            }
        }

    }
}