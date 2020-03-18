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
using umi3d.cdk;
using umi3d.common;

public class NotificationManager : AbstractNotificationManager
{
    public Notification NotificationPrefab;

    public override void Notify(NotificationDto notification)
    {
        Notification notif = Instantiate(NotificationPrefab, transform);
        notif.Title = notification.title;
        notif.Content = notification.content;
        notif.SetNotificationTime(notification.duration);
    }

    [ContextMenu("Debug")]
    private void debug()
    {
        for (int i = 0; i < 5; i++)
            Notify(new NotificationDto() { content = "yo ceci est un super test", title = "Titre " + i.ToString(), duration = Random.Range(2f,6f) });
    }

}
