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
            this.icon2dProperty = new UMI3DAsyncProperty<UMI3DResource>(notificationId, UMI3DPropertyKeys.NotificationContent,icon2d,(r,u)=>r.ToDto());
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


        public LoadEntity ToLoadEntity(UMI3DUser user)
        {
            return new LoadEntity() { entity = this, users = new HashSet<UMI3DUser>() { user } };
        }

        public LoadEntity ToLoadEntity(IEnumerable<UMI3DUser> users)
        {
            var load = new LoadEntity() { entity = this, users = new HashSet<UMI3DUser>() };
            load += UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>((u) => users.Contains(u));
            return load;
        }

        public LoadEntity ToLoadEntity()
        {
            var load = new LoadEntity() { entity = this, users = new HashSet<UMI3DUser>() };
            load += UMI3DEnvironment.GetEntities<UMI3DUser>();
            return load;
        }

        public DeleteEntity ToDeleteEntity(UMI3DUser user)
        {
            return new DeleteEntity() { entityId = Id(), users = new HashSet<UMI3DUser>() { user } };
        }

        public DeleteEntity ToDeleteEntity(IEnumerable<UMI3DUser> users)
        {
            var delete = new DeleteEntity() { entityId = Id(), users = new HashSet<UMI3DUser>() };
            delete += UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>((u) => users.Contains(u));
            return delete;
        }

        public DeleteEntity ToDeleteEntity()
        {
            var delete = new DeleteEntity() { entityId = Id(), users = new HashSet<UMI3DUser>() };
            delete += UMI3DEnvironment.GetEntities<UMI3DUser>();
            return delete;
        }


    }

    public class UMI3DNotificationOnObject : UMI3DNotification
    {

        public UMI3DAsyncProperty<UMI3DNode> objectIdProperty;

        public UMI3DNotificationOnObject(string title, string content, float duration, UMI3DResource icon2d, UMI3DResource icon3d, UMI3DNode objectId) : base(title, content,duration,icon2d,icon3d)
        {
            this.objectIdProperty = new UMI3DAsyncProperty<UMI3DNode>(Id(), UMI3DPropertyKeys.NotificationObjectId, objectId, (n, u) => n.Id());
        }

        protected override NotificationDto CreateDto()
        {
            return new NotificationOnObjectDto();
        }

        protected override void WriteProperties(NotificationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Odto = dto as NotificationOnObjectDto;
            if (Odto == null) return;
            Odto.objectId = objectIdProperty.GetValue(user).Id();
        }

    }
}