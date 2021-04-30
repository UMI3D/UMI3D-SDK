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

using System;
using System.Collections;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{

    public static class UMI3DTransactionDispatcher
    {

        // About transactions
        // A transaction is composed of a set of operations to be performed on entities (e.g Scenes, Nodes, Materials).
        // Operations should be applied in the same order as stored in the transaction.

        public static IEnumerator PerformTransaction(TransactionDto transaction)
        {
            yield return new WaitForEndOfFrame();
            foreach (AbstractOperationDto operation in transaction.operations)
            {
                bool performed = false;
                PerformOperation(operation, () => performed = true);
                if (performed != true)
                    yield return new WaitUntil(() => performed);
            }
        }

        static public void PerformOperation(AbstractOperationDto operation, Action performed)
        {
            if (performed == null) performed = () => { };
            switch (operation)
            {
                case LoadEntityDto load:
                    UMI3DEnvironmentLoader.LoadEntity(load.entity, performed);
                    break;
                case DeleteEntityDto delete:
                    UMI3DEnvironmentLoader.DeleteEntity(delete.entityId, performed);
                    break;
                case SetEntityPropertyDto set:
                    UMI3DEnvironmentLoader.SetEntity(set);
                    performed.Invoke();
                    break;
                case MultiSetEntityPropertyDto multiSet:
                    UMI3DEnvironmentLoader.SetMultiEntity(multiSet);
                    performed.Invoke();
                    break;

                default:
                    UMI3DEnvironmentLoader.Parameters.UnknownOperationHandler(operation, performed);
                    break;
            }
        }
    }
}