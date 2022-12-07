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
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// When this component is destroyed, a DestroyEntity operation will be sent for every UMI3DEntity in children.
    /// </summary>
    public class OnDestroyListener : MonoBehaviour
    {
        private void OnDestroy()
        {
            List<DeleteEntity> deleteOperations = this.GetComponentsInChildren<UMI3DEntity>().ToList()
                .ConvertAll(entity =>
                    new DeleteEntity()
                    {
                        entityId = entity.Id(),
                        users = UMI3DServer.Instance.UserSetWhenHasJoined()
                    });

            if (deleteOperations.Count > 0)
            {
                var deleteTransaction = new Transaction();
                deleteTransaction.AddIfNotNull(deleteOperations);
                deleteTransaction.reliable = true;

                UMI3DServer.Dispatch(deleteTransaction);
            }
        }
    }
}