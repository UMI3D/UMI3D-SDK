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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{

    public static class UMI3DTransactionDispatcher
    {

        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        /// <summary>
        /// Unpack the transaction and apply the operations.
        /// </summary>
        /// <param name="transaction">Transaction to unpack.</param>
        /// <returns></returns>
        ///  A transaction is composed of a set of operations to be performed on entities (e.g Scenes, Nodes, Materials).
        ///  Operations should be applied in the same order as stored in the transaction.
        public static async Task PerformTransaction(TransactionDto transaction)
        {
            int _transaction = count++;
            int opCount = 0;
            foreach (AbstractOperationDto operation in transaction.operations)
            {
                bool performed = false;
                var ErrorTime = Time.time + secondBeforeError;
                int op = opCount++;

                async void isOk()
                {
                    while (!performed)
                    {
                        if (Time.time > ErrorTime)
                            UMI3DLogger.LogError($"Operation took more than {secondBeforeError} sec it might have failed.\n Transaction count {transaction}.\n Operation count {op}.\n Operation : {operation} ", scope);
                        await UMI3DAsyncManager.Yield();
                    }
                }

                isOk();
                await PerformOperation(operation);
                performed = true;
            }
        }

        static int count = 0;
        const int secondBeforeError = 300;

        /// <summary>
        /// Unpack the transaction from a <paramref name="container"/> and apply the operations.
        /// </summary>
        /// <param name="container">Transaction in a container.</param>
        /// <returns></returns>
        ///  A transaction is composed of a set of operations to be performed on entities (e.g Scenes, Nodes, Materials).
        ///  Operations should be applied in the same order as stored in the transaction.
        public static async Task PerformTransaction(ByteContainer container)
        {
            int transaction = count++;
            int opCount = 0;
            foreach (ByteContainer c in UMI3DNetworkingHelper.ReadIndexesList(container))
            {
                bool performed = false;
                var ErrorTime = Time.time + secondBeforeError;
                int op = opCount++;

                async void isOk()
                {
                    while (!performed)
                    {
                        if (Time.time > ErrorTime)
                        {
                            UMI3DLogger.LogError($"Operation took more than {secondBeforeError} sec it might have failed.\n Transaction count {transaction}.\n Operation count {op}.\n Container : {container}\n SubContainer : {c}", scope);
                            await UMI3DAsyncManager.Delay(1000);
                        }
                        await UMI3DAsyncManager.Yield();
                    }
                }

                isOk();
                await PerformOperation(c);
                performed = true;
            }
        }

        /// <summary>
        /// Apply an <paramref name="operation"/>.
        /// </summary>
        /// <param name="operation">Operation to apply.</param>
        /// <param name="performed">Callback.</param>
        public static async Task PerformOperation(AbstractOperationDto operation)
        {
            switch (operation)
            {
                case LoadEntityDto load:
                    await Task.WhenAll(load.entities.Select(async entity =>
                    {
                        await UMI3DEnvironmentLoader.LoadEntity(entity);
                    }));
                    break;
                case DeleteEntityDto delete:
                    await UMI3DEnvironmentLoader.DeleteEntity(delete.entityId);
                    break;
                case SetEntityPropertyDto set:
                    UMI3DEnvironmentLoader.SetEntity(set);
                    break;
                case MultiSetEntityPropertyDto multiSet:
                    UMI3DEnvironmentLoader.SetMultiEntity(multiSet);
                    break;
                case StartInterpolationPropertyDto interpolationStart:
                    UMI3DEnvironmentLoader.StartInterpolation(interpolationStart);
                    break;
                case StopInterpolationPropertyDto interpolationStop:
                    UMI3DEnvironmentLoader.StopInterpolation(interpolationStop);
                    break;

                default:
                    await UMI3DEnvironmentLoader.Parameters.UnknownOperationHandler(operation);
                    break;
            }
        }

        /// <summary>
        /// Apply an operation in a <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Operation to apply as a container.</param>
        /// <param name="performed">Callback.</param>
        public static async Task PerformOperation(ByteContainer container)
        { 
            uint operationId = UMI3DNetworkingHelper.Read<uint>(container);
            switch (operationId)
            {
                case UMI3DOperationKeys.LoadEntity:
                    await UMI3DEnvironmentLoader.LoadEntity(container);
                    break;
                case UMI3DOperationKeys.DeleteEntity:
                    {
                        ulong entityId = UMI3DNetworkingHelper.Read<ulong>(container);
                        await UMI3DEnvironmentLoader.DeleteEntity(entityId);
                        break;
                    }
                case UMI3DOperationKeys.MultiSetEntityProperty:
                    UMI3DEnvironmentLoader.SetMultiEntity(container);
                    break;
                case UMI3DOperationKeys.StartInterpolationProperty:
                    UMI3DEnvironmentLoader.StartInterpolation(container);
                    break;
                case UMI3DOperationKeys.StopInterpolationProperty:
                    UMI3DEnvironmentLoader.StopInterpolation(container);
                    break;

                default:
                    if (UMI3DOperationKeys.SetEntityProperty <= operationId && operationId <= UMI3DOperationKeys.SetEntityMatrixProperty)
                    {
                        ulong entityId = UMI3DNetworkingHelper.Read<ulong>(container);
                        uint propertyKey = UMI3DNetworkingHelper.Read<uint>(container);
                        UMI3DEnvironmentLoader.SetEntity(operationId, entityId, propertyKey, container);
                    }
                    else
                        await UMI3DEnvironmentLoader.Parameters.UnknownOperationHandler(operationId, container);
                    break;
            }
        }
    }
}