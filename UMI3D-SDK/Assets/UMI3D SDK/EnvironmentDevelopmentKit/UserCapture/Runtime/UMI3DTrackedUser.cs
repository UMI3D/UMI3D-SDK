/*
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
using umi3d.common.userCapture;
using umi3d.common;
using UnityEngine;
using UnityEngine.UIElements;
using inetum.unityUtils;
using System;
using UnityEngine.Events;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// <see cref="UMI3DUser"/> with a UMI3D Avatar attached to it.
    /// </summary>
    public class UMI3DTrackedUser : UMI3DUser
    {

        private bool activeAvatarBindings_ = true;

        public UMI3DAsyncListProperty<UMI3DBinding> bindings { get { return _bindings; } protected set => _bindings = value; }
        public UMI3DAsyncProperty<bool> activeBindings { get { return _activeBindings; } protected set => _activeBindings = value; }

        public class OnActivationValueChanged : UnityEvent<ulong, bool> { };

        public static OnActivationValueChanged onActivationValueChanged = new OnActivationValueChanged();
        private UMI3DAsyncListProperty<UMI3DBinding> _bindings;
        private UMI3DAsyncProperty<bool> _activeBindings;

        /// <summary>
        /// User's avatar
        /// </summary>

        public UserTrackingFrameDto CurrentTrackingFrame;

        public UMI3DTrackedUser(ulong id) : base()
        {
            base.userId = id;
            bindings = new UMI3DAsyncListProperty<UMI3DBinding>(base.userId, UMI3DPropertyKeys.UserBindings, new());
            activeBindings = new UMI3DAsyncProperty<bool>(base.userId, UMI3DPropertyKeys.ActiveBindings, new());
        }   

        //To Delete
        //private UMI3DAvatarNode avatar;

        ///// <summary>
        ///// User's avatar
        ///// </summary>
        //public UMI3DAvatarNode Avatar
        //{
        //    get => avatar;
        //    set
        //    {
        //        if (avatar == value)
        //            return;
        //        if (avatar != null)
        //            GameObject.Destroy(avatar.gameObject);
        //        if (value != null)
        //            value.userId = Id();
        //        avatar = value;
        //        UMI3DServer.Instance.NotifyUserChanged(this);
        //    }
        //}
    }
}