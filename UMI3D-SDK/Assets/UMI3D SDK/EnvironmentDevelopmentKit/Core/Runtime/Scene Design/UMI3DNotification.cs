﻿/*
Copyright 2019 - 2021 Inetum

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
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    public class UMI3DNotification : UMI3DLoadableEntity
    {
        public UMI3DAsyncProperty<string> titleProperty;
        public UMI3DAsyncProperty<string> contentProperty;
        public UMI3DAsyncProperty<float> durationProperty;
        public UMI3DAsyncProperty<UMI3DResource> icon2dProperty;
        public UMI3DAsyncProperty<UMI3DResource> icon3dProperty;

        public UMI3DNotification(string title, string content, float duration, UMI3DResource icon2d, UMI3DResource icon3d)
        {
            Register();
            var PropertyEquality = new UMI3DAsyncPropertyEquality();
            this.titleProperty = new UMI3DAsyncProperty<string>(notificationId, UMI3DPropertyKeys.NotificationTitle, title);
            this.contentProperty = new UMI3DAsyncProperty<string>(notificationId, UMI3DPropertyKeys.NotificationContent, content);
            this.durationProperty = new UMI3DAsyncProperty<float>(notificationId, UMI3DPropertyKeys.NotificationDuration, duration, null, PropertyEquality.FloatEquality);
            this.icon2dProperty = new UMI3DAsyncProperty<UMI3DResource>(notificationId, UMI3DPropertyKeys.NotificationContent, icon2d, (r, u) => r.ToDto());
            this.icon3dProperty = new UMI3DAsyncProperty<UMI3DResource>(notificationId, UMI3DPropertyKeys.NotificationContent, icon3d, (r, u) => r.ToDto());
        }

        private ulong notificationId;

        public ulong Id()
        {
            if (notificationId == 0 && UMI3DEnvironment.Exists)
                Register();
            return notificationId;
        }

        private void Register()
        {
            if (notificationId == 0 && UMI3DEnvironment.Exists)
            {
                notificationId = UMI3DEnvironment.Register(this);
            }
        }

        protected virtual NotificationDto CreateDto()
        {
            return new NotificationDto();
        }

        protected virtual void WriteProperties(NotificationDto dto, UMI3DUser user)
        {
            dto.id = Id();
            dto.title = titleProperty.GetValue(user);
            dto.content = contentProperty.GetValue(user);
            dto.duration = durationProperty.GetValue(user);
            dto.icon2D = icon2dProperty.GetValue(user)?.ToDto();
            dto.icon3D = icon3dProperty.GetValue(user)?.ToDto();
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            NotificationDto dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
        }

        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(Id())
                + UMI3DNetworkingHelper.Write(titleProperty.GetValue(user))
                + UMI3DNetworkingHelper.Write(contentProperty.GetValue(user))
                + UMI3DNetworkingHelper.Write(durationProperty.GetValue(user))
                + icon2dProperty.GetValue(user)?.ToByte()
                + icon3dProperty.GetValue(user)?.ToByte();
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };
            return operation;
        }

        /// <summary>
        /// Return delete operation
        /// </summary>
        /// <returns></returns>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSet()
            };
            return operation;
        }

        #region filter
        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }
        #endregion
    }

    public class UMI3DNotificationOnObject : UMI3DNotification
    {

        public UMI3DAsyncProperty<UMI3DNode> objectIdProperty;

        public UMI3DNotificationOnObject(string title, string content, float duration, UMI3DResource icon2d, UMI3DResource icon3d, UMI3DNode objectId) : base(title, content, duration, icon2d, icon3d)
        {
            this.objectIdProperty = new UMI3DAsyncProperty<UMI3DNode>(Id(), UMI3DPropertyKeys.NotificationObjectId, objectId, (n, u) => n.Id());
        }

        ///<inheritdoc/>
        protected override NotificationDto CreateDto()
        {
            return new NotificationOnObjectDto();
        }

        ///<inheritdoc/>
        protected override void WriteProperties(NotificationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Odto = dto as NotificationOnObjectDto;
            if (Odto == null) return;
            Odto.objectId = objectIdProperty.GetValue(user).Id();
        }

    }
}