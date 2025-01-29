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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace umi3d.edk
{
    public class UMI3DGroupAsyncProperty : IEnumerable<UMI3DUser> {

        /// <summary>
        /// Users in this group
        /// </summary>
        public HashSet<UMI3DUser> users { get; protected set; } = new();

        /// <summary>
        /// property using this group
        /// </summary>
        public HashSet<UMI3DAsyncProperty> properties { get; protected set; } = new();

        /// <summary>
        /// Add a user to this group and apply this change to all the property in <see cref="UMI3DGroupAsyncProperty.properties"/>
        /// </summary>
        /// <param name="user">user to add</param>
        /// <returns>A collection of SetEntityProperty describing the change from each property default value to the group default value in it</returns>
        public virtual List<SetEntityProperty> Add(UMI3DUser user)
        {
            if (user == null)
                return null;

            if (users.Add(user))
               return properties.Select(p => p.AddToGroup(user, this)).ToList();

            return null;
        }

        /// <summary>
        /// Remove a user from this group and apply this change to all the property in <see cref="UMI3DGroupAsyncProperty.properties"/>
        /// </summary>
        /// <param name="user">user to remove</param>
        /// <returns>A collection of SetEntityProperty describing the change to each property default value from the group default value in it</returns>
        public virtual List<SetEntityProperty> Remove(UMI3DUser user)
        {
            if (user == null)
                return null;

            if (users.Remove(user))
                return properties.Select(p => p.RemoveFromGroup(user, this)).ToList();

            return null;
        }

        /// <summary>
        /// Add this group to the given property.
        /// </summary>
        /// <param name="property"></param>
        /// <remarks>Does not return a SetEntityProperty as the group value is inited to the default value of the property</remarks>
        public virtual void Add(UMI3DAsyncProperty property)
        {
            if (property == null)
                return;

            if (properties.Add(property))
                property.AddGroup(this);
        }

        /// <summary>
        /// Remove this group from the given property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual SetEntityProperty Remove(UMI3DAsyncProperty property)
        {
            if (property == null)
                return null;

            if (properties.Remove(property))
                return property.RemoveGroup(this);

            return null;
        }

        #region IEnumerator

        public IEnumerator<UMI3DUser> GetEnumerator()
        {
            return users.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return users.GetEnumerator();
        }
        #endregion IEnumerator
    }
}
