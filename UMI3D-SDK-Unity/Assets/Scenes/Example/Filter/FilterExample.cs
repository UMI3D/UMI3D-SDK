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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.edk;

namespace umi3d.example
{
    [RequireComponent(typeof(GenericObject3D))]
    public class FilterExample : VisibilityFilter
    {
        /// <summary>
        /// a UMI3DAsyncProperty<T> is a property that can have different value for each User.
        /// Here we will set the visible state of each User.
        /// </summary>
        public UMI3DAsyncProperty<bool> visible;
        public Hover Hover;

        public bool Invert;

        private void Start()
        {
            //the property handler of a genericObject is call when at least one property has been changed and the object need to send this changed.
            //the second parameter is the default value of the property for all the users.
            //the last one state if the property has the same value for all the user. 
            visible = new UMI3DAsyncProperty<bool>(GetComponent<GenericObject3D>().PropertiesHandler, Invert, true);
            //the onHoverEnter/Exit event is sent when a user start/end hovering the Hover target.
            Hover.onHoverEnter.AddListener(onHoverEnter);
            Hover.onHoverExit.AddListener(onHoverExit);
        }

        void onHoverEnter(UMI3DUser user)
        {
            visible.SetValue(user, !Invert);
        }

        void onHoverExit(UMI3DUser user)
        {
            visible.SetValue(user, Invert);
        }

        /// <summary>
        /// VisibilityFilter Accept Function. Return true if you want the object to be visible
        /// we want to display the object to a user only when its property is evaluate at true.
        /// </summary>
        public override bool Accept(UMI3DUser user)
        {
            return visible.GetValue(user);
        }
    }
}