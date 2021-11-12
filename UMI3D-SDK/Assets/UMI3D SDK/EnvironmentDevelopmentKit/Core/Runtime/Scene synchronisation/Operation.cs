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
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    public abstract class Operation : IBytable
    {
        /// <summary>
        /// List of users to which this operation should be send.
        /// </summary>
        public HashSet<UMI3DUser> users = new HashSet<UMI3DUser>();

        /// <summary>
        /// Return the operationDto of this Dto.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public abstract AbstractOperationDto ToOperationDto(UMI3DUser user);

        /// <summary>
        /// Return the byte[] of this operation.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>the size needed ans a function to set the byte array at a position and return the size again</returns>
        public abstract Bytable ToBytable(UMI3DUser user);

        Bytable IBytable.ToBytableArray(params object[] parameters)
        {
            if (parameters.Length < 1)
                return ToBytable(null);
            return ToBytable(parameters[0] as UMI3DUser);
        }

        bool IBytable.IsCountable()
        {
            return false;
        }

        public static Operation operator +(Operation a, Operation b)
        {
            a.users = new HashSet<UMI3DUser>(a.users.Concat(b.users));
            return a;
        }

        public static Operation operator +(Operation a, IEnumerable<UMI3DUser> b)
        {
            a.users = new HashSet<UMI3DUser>(a.users.Concat(b));
            return a;
        }

        public static Operation operator -(Operation a, Operation b)
        {
            foreach (UMI3DUser u in b.users)
            {
                if (a.users.Contains(u)) a.users.Remove(u);
            }
            return a;
        }

        public static Operation operator -(Operation a, IEnumerable<UMI3DUser> b)
        {
            foreach (UMI3DUser u in b)
            {
                if (a.users.Contains(u)) a.users.Remove(u);
            }
            return a;
        }

    }
}