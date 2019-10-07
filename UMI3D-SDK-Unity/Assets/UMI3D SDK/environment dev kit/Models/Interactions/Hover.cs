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
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk
{
    /// <summary>
    /// Hovering interaction.
    /// </summary>
    public class Hover : AbstractInteraction<HoverDto>
    {
               
        /// <summary>
        /// Event for hover information (first parameter is User, second parameter is the hover point local position, third parameter is the point local normal)
        /// </summary>
        [System.Serializable]
        public class HoveredListener : UnityEvent<UMI3DUser, Vector3, Vector3> { }

        /// <summary>
        /// Object to hover.
        /// </summary>
        public GenericObject3D target;

        /// <summary>
        /// If set to true, the browser will send the hover point position and normal.
        /// </summary>
        /// <see cref="onHover"/>
        public bool TrackHoveredPosition;

        /// <summary>
        /// Called the first frame of hoverring.
        /// </summary>
        [SerializeField]
        public UMI3DUserEvent onHoverExit = new UMI3DUserEvent();

        /// <summary>
        /// Called the first frame after hoverring.
        /// </summary>
        [SerializeField]
        public UMI3DUserEvent onHoverEnter = new UMI3DUserEvent();

        /// <summary>
        /// Called each frame of hoverring.
        /// </summary>
        [SerializeField]
        public HoveredListener onHover = new HoveredListener();

        /// <summary>
        /// Is currently hoverred.
        /// </summary>
        bool _IsHover = false;

        /// <summary>
        /// Convert to Dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override HoverDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);

            dto.targetID = target.Id;
            dto.shouldTrackPosition = TrackHoveredPosition;
            return dto;
        }

        /// <summary>
        /// Called on interaction request from user.
        /// </summary>
        /// <param name="user">User who request interaction</param>
        /// <param name="evt">Interaction data</param>
        public override void OnUserInteraction(UMI3DUser user, JSONObject evt)
        {
            if (evt == null)
                return;
            if (evt.IsBool)
            {
                if (evt.b)
                {
                    onHoverEnter.Invoke(user);
                }
                else
                {
                    onHoverExit.Invoke(user);
                }
            }
            else
            { 
                object res = DtoUtility.Deserialize(evt);

                if (res is HoveredDto)
                {
                    HoveredDto h = res as HoveredDto;
                    if (_IsHover != h.State)
                    {
                        _IsHover = h.State;
                        if (_IsHover)
                        {
                            onHoverEnter.Invoke(user);
                        }
                        else
                        {
                            onHoverExit.Invoke(user);
                        }
                    }
                    if (_IsHover)
                        onHover.Invoke(user, target.transform.TransformPoint( h.Position), target.transform.TransformPoint(h.Normal));
                }
            }
        }

        /// <summary>
        /// Create an empty dto.
        /// </summary>
        /// <returns></returns>
        public override HoverDto CreateDto()
        {
            return new HoverDto();
        }

        /// <summary>
        /// Return true if this interaction is available for the given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check availability for</param>
        /// <returns></returns>
        public override bool AvailableAsChildFor(UMI3DUser user)
        {
            if (target == null)
                return false;
            else
                return base.AvailableAsChildFor(user) && target.VisibleFor(user);
        }

        /// <summary>
        /// Return true if this interaction is available for the given user, false otherwise.
        /// </summary>
        /// <param name="user">User to check availability for</param>
        /// <returns></returns>
        public override bool AvailableFor(UMI3DUser user)
        {
            if (target == null)
                return false;
            else
                return base.AvailableFor(user) && target.VisibleFor(user);
        }
    }
}
