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
using UnityEngine;
using umi3d.edk;
using umi3d.common;

public class NotificationsExamples : MonoBehaviour
{
    public AbstractObject3D objectNotified;

    public void SendGlobalNotification(UMI3DUser user, string boneId)
    {
        NotificationDto notif = new NotificationDto()
        {
            title = "Global notification",
            content = "This is a global notification.",
            duration = 5f,
        };
        UMI3DNotifier.Notify(user, notif);
    }


    public void SendObjectNotification(UMI3DUser user, string boneId)
    {
        NotificationOnObjectDto notif = new NotificationOnObjectDto()
        {
            title = "Contextual notification",
            content = "This is a contextual notification concerning this object.",
            duration = 3f,
            objectId = objectNotified.Id,
        };
        UMI3DNotifier.Notify(user, notif);
    }
}
