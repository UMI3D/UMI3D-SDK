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

        string notificationId;

        public string Id()
        {
            if (notificationId == null && UMI3DEnvironment.Exists)
                Register();
            return notificationId;
        }

        void Register()
        {
            if (notificationId == null && UMI3DEnvironment.Exists)
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
            var dto = CreateDto();
            WriteProperties(dto, user);
            return dto;
        }


        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entity = this,
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
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
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
        }

        #region filter
        HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

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