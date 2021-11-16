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

        public static IEnumerator PerformTransaction(ByteContainer container)
        {
            yield return new WaitForEndOfFrame();
            foreach (ByteContainer c in UMI3DNetworkingHelper.ReadIndexesList(container))
            {
                bool performed = false;
                PerformOperation(c, () => performed = true);
                if (performed != true)
                    yield return new WaitUntil(() => performed);
            }
        }




        public static void PerformOperation(AbstractOperationDto operation, Action performed)
        {
            if (performed == null) performed = () => { };
            switch (operation)
            {
                case LoadEntityDto load:
                    int count = load.entities.Count;
                    int performedCount = 0;
                    Action performed2 = () => { performedCount++; if (performedCount == count) performed.Invoke(); };
                    foreach (IEntity entity in load.entities)
                    {
                        UMI3DEnvironmentLoader.LoadEntity(entity, performed2);
                    }
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
                case StartInterpolationPropertyDto interpolationStart:
                    UMI3DEnvironmentLoader.StartInterpolation(interpolationStart);
                    performed.Invoke();
                    break;
                case StopInterpolationPropertyDto interpolationStop:
                    UMI3DEnvironmentLoader.StopInterpolation(interpolationStop);
                    performed.Invoke();
                    break;

                default:
                    UMI3DEnvironmentLoader.Parameters.UnknownOperationHandler(operation, performed);
                    break;
            }
        }

        public static void PerformOperation(ByteContainer container, Action performed)
        {
            if (performed == null) performed = () => { };

            uint operationId = UMI3DNetworkingHelper.Read<uint>(container);
            switch (operationId)
            {
                case UMI3DOperationKeys.LoadEntity:
                    UMI3DEnvironmentLoader.LoadEntity(container, performed);
                    break;
                case UMI3DOperationKeys.DeleteEntity:
                    {
                        ulong entityId = UMI3DNetworkingHelper.Read<ulong>(container);
                        UMI3DEnvironmentLoader.DeleteEntity(entityId, performed);
                        break;
                    }
                case UMI3DOperationKeys.MultiSetEntityProperty:
                    UMI3DEnvironmentLoader.SetMultiEntity(container);
                    performed.Invoke();
                    break;
                case UMI3DOperationKeys.StartInterpolationProperty:
                    UMI3DEnvironmentLoader.StartInterpolation(container);
                    performed.Invoke();
                    break;
                case UMI3DOperationKeys.StopInterpolationProperty:
                    UMI3DEnvironmentLoader.StopInterpolation(container);
                    performed.Invoke();
                    break;

                default:
                    if (UMI3DOperationKeys.SetEntityProperty <= operationId && operationId <= UMI3DOperationKeys.SetEntityMatrixProperty)
                    {
                        ulong entityId = UMI3DNetworkingHelper.Read<ulong>(container);
                        uint propertyKey = UMI3DNetworkingHelper.Read<uint>(container);
                        UMI3DEnvironmentLoader.SetEntity(operationId, entityId, propertyKey, container);
                        performed.Invoke();
                    }
                    else
                    {
                        UMI3DEnvironmentLoader.Parameters.UnknownOperationHandler(operationId, container, performed);
                    }
                    break;
            }

        }

    }
}